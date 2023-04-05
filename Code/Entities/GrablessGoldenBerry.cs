using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.JungleHelper.Entities {
    [CustomEntity("JungleHelper/TreeDepthController")] // memorialTextController but jungle
    [RegisterStrawberry(tracked: false, blocksCollection: true)]
    [Tracked]
    public class GrablessGoldenBerry : Strawberry {
        private static List<ILHook> manualILHooks = new List<ILHook>();

        public static void Load() {
            On.Celeste.Level.Reload += onLevelReload;
            On.Celeste.Player.ClimbUpdate += onPlayerClimbUpdate;

            // we can't hook UpdateLevelStartDashes because it's too short! So let's hook its usages instead.
            IL.Celeste.SummitCheckpoint.Update += hookUpdateLevelStartDashes;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += hookUpdateLevelStartDashes;
            IL.Celeste.HeartGem.RegisterAsCollected += hookUpdateLevelStartDashes;
            IL.Celeste.Key.OnPlayer += hookUpdateLevelStartDashes;

            manualILHooks.Add(new ILHook(typeof(Cassette).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), hookUpdateLevelStartDashes));
            manualILHooks.Add(new ILHook(typeof(Strawberry).GetMethod("orig_OnCollect"), hookUpdateLevelStartDashes));
            manualILHooks.Add(new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), hookUpdateLevelStartDashes));
        }

        public static void Unload() {
            On.Celeste.Level.Reload -= onLevelReload;
            On.Celeste.Player.ClimbUpdate -= onPlayerClimbUpdate;

            IL.Celeste.SummitCheckpoint.Update -= hookUpdateLevelStartDashes;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= hookUpdateLevelStartDashes;
            IL.Celeste.HeartGem.RegisterAsCollected -= hookUpdateLevelStartDashes;
            IL.Celeste.Key.OnPlayer -= hookUpdateLevelStartDashes;

            foreach (ILHook hook in manualILHooks) {
                hook.Dispose();
            }
            manualILHooks.Clear();
        }


        private static void hookUpdateLevelStartDashes(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Session>("UpdateLevelStartDashes"))) {
                Logger.Log("JungleHelper/GrablessGoldenBerry", $"Injecting code after UpdateLevelStartDashes call at {cursor.Index} in IL for {il.Method.FullName}");

                // when UpdateLevelStartDashes is called, "commit" the grabless berry state
                cursor.EmitDelegate<Action>(() => JungleHelperModule.Session.GrablessBerryFlewAway = JungleHelperModule.Session.GrablessBerryWillFlyAway);
            }
        }

        private static void onLevelReload(On.Celeste.Level.orig_Reload orig, Level self) {
            if (!self.Completed) {
                // "reset" the grabless berry state
                JungleHelperModule.Session.GrablessBerryWillFlyAway = JungleHelperModule.Session.GrablessBerryFlewAway;
            }
            orig(self);
        }

        private static int onPlayerClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self) {
            // climbing is illegal!!!!111!!1
            JungleHelperModule.Session.GrablessBerryWillFlyAway = true;
            self.Scene?.Tracker.GetEntity<GrablessGoldenBerry>()?.flyAway?.Invoke();
            return orig(self);
        }


        private Action flyAway;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // when instanciating the berry, pretend it's a Memorial Text Controller (tm), so that we get a shiny dashless golden berry.
            entityData.Name = "memorialTextController";
            GrablessGoldenBerry result = new GrablessGoldenBerry(entityData, offset, new EntityID(levelData.Name, entityData.ID));
            entityData.Name = "JungleHelper/TreeDepthController";
            return result;
        }

        public GrablessGoldenBerry(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset, id) {

            DashListener dashListener = Get<DashListener>();
            if (dashListener != null) {
                // prevent the dashless golden from reacting to dash, but save the dash handler somewhere
                // so that we can call it when the player is climbing a wall...
                Remove(dashListener);
                flyAway = () => dashListener.OnDash(Vector2.Zero);
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (!SceneAs<Level>().Session.StartedFromBeginning || JungleHelperModule.Session.GrablessBerryFlewAway) {
                // grabless golden is gone :crab: because the player started from a checkpoint or grabbed a wall.
                RemoveSelf();
            }
        }
    }
}
