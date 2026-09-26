using System;
using RoTCheats.Cheats;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Localization;

namespace RoTCheats.Behaviors
{
    public class ROTCheatsCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddMenuOptions(starter);
        }

        private void AddMenuOptions(CampaignGameStarter starter)
        {
            try
            {
                // Camp menu option
                starter.AddGameMenuOption(
                    "camp",
                    "rot_cheats_camp_opt",
                    "{=rot_cheats_menu}Realm of Thrones Cheats",
                    delegate(MenuCallbackArgs args)
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                        return true;
                    },
                    delegate(MenuCallbackArgs args)
                    {
                        CheatMenuManager.OpenMainMenu();
                    },
                    false,
                    1
                );

                // Town menu option
                starter.AddGameMenuOption(
                    "town",
                    "rot_cheats_town_opt",
                    "{=rot_cheats_menu}Realm of Thrones Cheats",
                    delegate(MenuCallbackArgs args)
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                        return true;
                    },
                    delegate(MenuCallbackArgs args)
                    {
                        CheatMenuManager.OpenMainMenu();
                    },
                    false,
                    5
                );

                // Castle menu option
                starter.AddGameMenuOption(
                    "castle",
                    "rot_cheats_castle_opt",
                    "{=rot_cheats_menu}Realm of Thrones Cheats",
                    delegate(MenuCallbackArgs args)
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                        return true;
                    },
                    delegate(MenuCallbackArgs args)
                    {
                        CheatMenuManager.OpenMainMenu();
                    },
                    false,
                    5
                );

                // Imprisonment menus: Escape Mysteriously
                string[] captivityMenus = new string[]
                {
                    "prisoner_wait",
                    "settlement_wait",
                    "menu_captivity_castle_remain",
                    "menu_captivity_transfer_to_town",
                    "taken_prisoner",
                    "defeated_and_taken_prisoner"
                };

                foreach (string menuId in captivityMenus)
                {
                    try
                    {
                        starter.AddGameMenuOption(
                            menuId,
                            "rot_cheats_escape_" + menuId,
                            "{=rot_cheats_escape}✨ Escape mysteriously (RoT Cheats)",
                            delegate(MenuCallbackArgs args)
                            {
                                args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                                return true;
                            },
                            delegate(MenuCallbackArgs args)
                            {
                                ImprisonmentCheats.EscapeMysteriously();
                            },
                            false,
                            0
                        );
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
    }
}
