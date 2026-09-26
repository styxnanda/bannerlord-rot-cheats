using System;
using System.Collections.Generic;
using HarmonyLib;
using RoTCheats.Behaviors;
using RoTCheats.Cheats;
using RoTCheats.Config;
using RoTCheats.MissionBehaviors;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace RoTCheats
{
    public class ROTCheatsSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                Harmony harmony = new Harmony("mod.rot.cheats");
                foreach (Type type in typeof(ROTCheatsSubModule).Assembly.GetTypes())
                {
                    try
                    {
                        object[] attrs = type.GetCustomAttributes(typeof(HarmonyPatch), false);
                        if (attrs != null && attrs.Length > 0)
                        {
                            harmony.CreateClassProcessor(type).Patch();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            base.InitializeGameStarter(game, starterObject);
            CampaignGameStarter campaignStarter = starterObject as CampaignGameStarter;
            if (campaignStarter != null)
            {
                campaignStarter.AddBehavior(new ROTCheatsCampaignBehavior());
            }
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (mission != null)
            {
                mission.AddMissionBehavior(new ROTCheatsMissionBehavior());
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);

            try
            {
                // Hotkey 1: F10
                // Hotkey 2: Ctrl + Shift + C
                bool f10Pressed = Input.IsKeyPressed(InputKey.F10);
                bool ctrlShiftCPressed = (Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl)) &&
                                         (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift)) &&
                                         Input.IsKeyPressed(InputKey.C);

                if (f10Pressed || ctrlShiftCPressed)
                {
                    CheatMenuManager.OpenMainMenu();
                }
            }
            catch
            {
            }
        }

        #region Console Commands
        [CommandLineFunctionality.CommandLineArgumentFunction("menu", "rotcheats")]
        public static string CommandOpenMenu(List<string> strings)
        {
            CheatMenuManager.OpenMainMenu();
            return "Realm of Thrones Cheat Menu opened.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("godmode", "rotcheats")]
        public static string CommandToggleGodMode(List<string> strings)
        {
            CheatSettings.Instance.GodMode = !CheatSettings.Instance.GodMode;
            CheatSettings.Instance.Save();
            return "God Mode is now " + (CheatSettings.Instance.GodMode ? "ENABLED" : "DISABLED");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("dragon", "rotcheats")]
        public static string CommandSpawnDragon(List<string> strings)
        {
            string dragonId = "dragon_black";
            if (strings != null && strings.Count > 0)
            {
                dragonId = strings[0];
            }
            bool success = DragonSpawner.SpawnDragon(dragonId, true);
            return success ? "Spawned and equipped " + dragonId : "Failed to spawn " + dragonId;
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("all_dragons", "rotcheats")]
        public static string CommandSpawnAllDragons(List<string> strings)
        {
            DragonSpawner.SpawnAllDragons();
            return "All dragons and saddles added to party!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("weapons", "rotcheats")]
        public static string CommandGiveWeapons(List<string> strings)
        {
            ItemCheats.GiveAllLegendaryWeapons();
            return "All RoT legendary Valyrian and unique weapons added to party!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("maxskills", "rotcheats")]
        public static string CommandMaxSkills(List<string> strings)
        {
            if (Hero.MainHero != null)
            {
                SkillCheats.MaxAllSkills(Hero.MainHero, 300);
                return "All skills set to 300 for " + Hero.MainHero.Name.ToString();
            }
            return "Main hero not available.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("onehit", "rotcheats")]
        public static string CommandToggleOneHit(List<string> strings)
        {
            CheatSettings.Instance.OneHitKill = !CheatSettings.Instance.OneHitKill;
            CheatSettings.Instance.Save();
            return "One-Hit Kill (Shield & Guard Piercing) is now " + (CheatSettings.Instance.OneHitKill ? "ENABLED" : "DISABLED");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("speed", "rotcheats")]
        public static string CommandSetSpeed(List<string> strings)
        {
            if (strings != null && strings.Count > 0)
            {
                float pSpeed = 1f;
                if (float.TryParse(strings[0], out pSpeed))
                {
                    CheatSettings.Instance.PlayerSpeedMultiplier = pSpeed;
                    if (strings.Count > 1)
                    {
                        float sSpeed = 1f;
                        if (float.TryParse(strings[1], out sSpeed))
                        {
                            CheatSettings.Instance.SquadSpeedMultiplier = sSpeed;
                        }
                    }
                    if (strings.Count > 2)
                    {
                        float hSpeed = 1f;
                        if (float.TryParse(strings[2], out hSpeed))
                        {
                            CheatSettings.Instance.HorseSpeedMultiplier = hSpeed;
                        }
                    }
                    else
                    {
                        CheatSettings.Instance.HorseSpeedMultiplier = pSpeed;
                    }
                    CheatSettings.Instance.Save();
                    return string.Format("Speeds updated: Player={0:0.0}x, Squad={1:0.0}x, Horse={2:0.0}x",
                        CheatSettings.Instance.PlayerSpeedMultiplier,
                        CheatSettings.Instance.SquadSpeedMultiplier,
                        CheatSettings.Instance.HorseSpeedMultiplier);
                }
            }
            return "Usage: rotcheats.speed <player_multiplier> [squad_multiplier] [horse_multiplier]";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("weight", "rotcheats")]
        public static string CommandToggleWeight(List<string> strings)
        {
            CheatSettings.Instance.UnlimitedWeight = !CheatSettings.Instance.UnlimitedWeight;
            CheatSettings.Instance.Save();
            return "Unlimited Weight Limit is now " + (CheatSettings.Instance.UnlimitedWeight ? "ENABLED" : "DISABLED");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("sweeping_force", "rotcheats")]
        public static string CommandSweepingForce(List<string> strings)
        {
            BattleSkills.CastSweepingForce();
            return "Unleashed Sweeping Force!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("rhllor_light", "rotcheats")]
        public static string CommandRhllorLight(List<string> strings)
        {
            BattleSkills.CastRhllorLight();
            return "Unleashed R'hllor's Light!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("cannibal_wisp", "rotcheats")]
        public static string CommandCannibalWisp(List<string> strings)
        {
            BattleSkills.CastCannibalWisp();
            return "Unleashed Cannibal's Wisp!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("deathly_hallows", "rotcheats")]
        public static string CommandDeathlyHallows(List<string> strings)
        {
            BattleSkills.CastDeathlyHallows();
            return "Unleashed Deathly Hallows!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("skills", "rotcheats")]
        public static string CommandSkillsMenu(List<string> strings)
        {
            BattleSkills.OpenSkillCastMenu();
            return "Battle Skills Quick Cast Menu opened.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("escape", "rotcheats")]
        public static string CommandEscape(List<string> strings)
        {
            ImprisonmentCheats.EscapeMysteriously();
            return "Escaped imprisonment mysteriously!";
        }
        #endregion
    }
}
