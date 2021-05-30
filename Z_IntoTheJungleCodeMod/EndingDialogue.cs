using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    [CustomEvent("IntoTheJungleCodeMod/EndingDialogue")]
    class EndingDialogue : CutsceneEntity {
        private SoundSource phoneSfx;

        private static string lastEnding;
        private static long lastEndingTimestamp = -100000;

        public EndingDialogue() : base(fadeInOnSkip: false, endingChapterAfter: true) { }

        public override void OnBegin(Level level) {
            level.RegisterAreaComplete();
            Add(new Coroutine(cutscene()));
            Add(phoneSfx = new SoundSource());
        }

        private IEnumerator cutscene() {
            Player player = Scene.Tracker.GetEntity<Player>();
            Payphone payphone = Scene.Tracker.GetEntity<Payphone>();
            if (player != null) {
                // replicate the ch2 ending cutscene.
                player.StateMachine.State = Player.StDummy;
                player.Dashes = 1;
                while (player.Light.Alpha > 0f) {
                    player.Light.Alpha -= Engine.DeltaTime * 1.25f;
                    yield return null;
                }
                yield return 1f;

                // align with the phone
                yield return player.DummyWalkTo(payphone.X - 4f);
                yield return 0.2f;

                // turn around
                player.Facing = Facings.Right;
                yield return 0.5f;

                // pick it up
                player.Visible = false;
                Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
                yield return payphone.Sprite.PlayRoutine("pickUp");
                yield return 0.25f;

                // wait for sound effect to finish
                phoneSfx.Position = player.Position;
                phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop");
                yield return 6f;

                phoneSfx.Stop();
                payphone.Sprite.Play("talkPhone");
            }

            string dialogue;

            if (Environment.TickCount - lastEndingTimestamp > 60000) {
                // draw a number between 0 and 1
                double rng = new Random().NextDouble();
                if (IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter > 50) {
                    // more than 50 endings without a rare one => force a rare ending (0.98 to 1)
                    rng = (rng / 50) + 0.98;
                }

                LevelSetStats stats = SaveData.Instance.GetLevelSetStats();
                double redBerryPercent = stats.TotalStrawberries / (double) stats.MaxStrawberries;

                if (SaveData.Instance.Time < TimeSpan.FromMinutes(30).Ticks && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_Speedrun_Fast")) {
                    // Finishing the A-sides with a game file under 30 minutes For the first time.
                    dialogue = "JungleEnd_Speedrun_Fast";

                    // blacklist both speedrun endings
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Speedrun_Fast");
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Speedrun_Normal");

                } else if (SaveData.Instance.Time < TimeSpan.FromHours(1).Ticks && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_Speedrun_Normal")) {
                    // Finishing the A-sides with a game file under 1 hour for the first time.
                    dialogue = "JungleEnd_Speedrun_Normal";
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add(dialogue);

                } else if (stats.TotalGoldenStrawberries >= 15 /* intentionally hardcoded */ && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_All_Golden_A")) {
                    // After collecting all 10 golden berries the FIRST time and then completing 5A (not counting grabless 1a)
                    dialogue = "JungleEnd_All_Golden_A";
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add(dialogue);

                } else if (IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_All_Golden_A") && IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_All_Red")
                    && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_All_Golden_B")) {
                    // Will happen once after getting both JungleEnd_All_Golden_A AND JungleEnd_All_Red to display and then completing 5A again.
                    dialogue = "JungleEnd_All_Golden_B";
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add(dialogue);

                } else if (!IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_First_Run")) {
                    // First playthrough on a game file. 
                    dialogue = "JungleEnd_First_Run";

                } else if (redBerryPercent >= 1 && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_All_Red")) {
                    // After collecting all XX Red berries for the first time
                    dialogue = "JungleEnd_All_Red";

                    // ban all worse berry endings
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_All_Red");
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_High");
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_Mid");
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_Low");

                } else if (redBerryPercent >= 0.75 && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_Berry_High")) {
                    // Collecting 75% of berries for the first time
                    dialogue = "JungleEnd_Berry_High";

                    // ban all worse berry endings
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_High");
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_Mid");
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_Low");

                } else if (redBerryPercent >= 0.5 && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_Berry_Mid")) {
                    // Collecting 50% of berries for the first time
                    dialogue = "JungleEnd_Berry_Mid";

                    // ban all worse berry endings
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_Mid");
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_Berry_Low");

                } else if (redBerryPercent >= 0.25 && !IntoTheJungleCodeModule.EndingDialogueBlacklist.Contains("JungleEnd_Berry_Low")) {
                    // Collecting 25% of berries for the first time
                    dialogue = "JungleEnd_Berry_Low";
                    IntoTheJungleCodeModule.EndingDialogueBlacklist.Add(dialogue);

                } else if (rng < 0.5) {
                    // 50% chance if no other endings trigger.
                    dialogue = "JungleEnd_Percent_A";
                    IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter++;

                } else if (rng < 0.9) {
                    // 40% chance if no other endings trigger.
                    dialogue = "JungleEnd_Percent_B";
                    IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter++;

                } else if (rng < 0.98) {
                    // 8% chance if no other endings trigger.
                    dialogue = "JungleEnd_Percent_C";
                    IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter++;

                } else if (rng < 0.985) {
                    // 0.5% chance if no other endings trigger.
                    dialogue = "JungleEnd_Percent_D";
                    IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter = 0;

                } else if (rng < 0.99) {
                    // 0.5% chance if no other endings trigger.
                    dialogue = "JungleEnd_Percent_E";
                    IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter = 0;

                } else if (rng < 0.995) {
                    // 0.5% chance if no other endings trigger.
                    dialogue = "JungleEnd_Percent_F";
                    IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter = 0;

                } else {
                    // 0.5% chance if no other endings trigger.
                    dialogue = "JungleEnd_Percent_G";
                    IntoTheJungleCodeModule.ModSaveData.NoRareEndingCounter = 0;

                }

                // whatever the dialogue is, the first playthrough is now done.
                IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_First_Run");

                // save the dialogue we picked in memory to prevent the player from teleporting repeatedly to see all endings.
                lastEnding = dialogue;
                lastEndingTimestamp = Environment.TickCount;
            } else {
                // the player last reached the ending dialogue less than a minute ago!
                // replay the same dialogue.
                dialogue = lastEnding;
            }

            // change music
            Level level = SceneAs<Level>();
            level.Session.Audio.Music.Event = "event:/JungleMusicByTg90/tg90_cjc_epilogue";
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);

            // play dialogue
            yield return Textbox.Say(dialogue, crashSounds);
            EndCutscene(Level);
        }

        private IEnumerator crashSounds() {
            Audio.Play("event:/junglehelper/sfx/TheoFridgeCrash");
            yield break;
        }

        public override void OnEnd(Level level) {
            level.CompleteArea(spotlightWipe: true, skipScreenWipe: false, skipCompleteScreen: false);
        }
    }
}
