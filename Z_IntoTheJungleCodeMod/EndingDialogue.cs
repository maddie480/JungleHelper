using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.IntoTheJungleCodeMod {
    [CustomEvent("IntoTheJungleCodeMod/EndingDialogue")]
    class EndingDialogue : CutsceneEntity {
        public EndingDialogue() : base(fadeInOnSkip: false, endingChapterAfter: true) { }

        public override void OnBegin(Level level) {
            Add(new Coroutine(cutscene()));
        }

        private IEnumerator cutscene() {
            Player p = Scene.Tracker.GetEntity<Player>();
            if (p != null) {
                p.StateMachine.State = Player.StDummy;
            }

            string dialogue;
            double rng = new Random().NextDouble();
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

            } else if (rng < 0.9) {
                // 40% chance if no other endings trigger.
                dialogue = "JungleEnd_Percent_B";

            } else if (rng < 0.98) {
                // 8% chance if no other endings trigger.
                dialogue = "JungleEnd_Percent_C";

            } else if (rng < 0.985) {
                // 0.5% chance if no other endings trigger.
                dialogue = "JungleEnd_Percent_D";

            } else if (rng < 0.99) {
                // 0.5% chance if no other endings trigger.
                dialogue = "JungleEnd_Percent_E";

            } else if (rng < 0.995) {
                // 0.5% chance if no other endings trigger.
                dialogue = "JungleEnd_Percent_F";

            } else {
                // 0.5% chance if no other endings trigger.
                dialogue = "JungleEnd_Percent_G";

            }

            // whatever the dialogue is, the first playthrough is now done.
            IntoTheJungleCodeModule.EndingDialogueBlacklist.Add("JungleEnd_First_Run");

            yield return Textbox.Say(dialogue);
            EndCutscene(Level);
        }

        public override void OnEnd(Level level) {
            level.CompleteArea(spotlightWipe: true, skipScreenWipe: false, skipCompleteScreen: false);
        }
    }
}
