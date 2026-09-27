using System;
using System.Collections.Generic;
using RoTCheats.Config;
using RoTCheats.UI;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace RoTCheats.Cheats
{
    public static class NavigationEnhancerManager
    {
        private static CampaignVec2 _lastClickedPosition = CampaignVec2.Zero;
        private static object _lastClickedVisual = null;
        private static bool _hasClickedPosition = false;

        public static void OnMapLeftClicked(object visual, CampaignVec2 intersectionPoint)
        {
            if (intersectionPoint.IsValid() && intersectionPoint != CampaignVec2.Zero)
            {
                _lastClickedPosition = intersectionPoint;
                _lastClickedVisual = visual;
                _hasClickedPosition = true;
            }
        }

        public static bool HasActiveDestination()
        {
            CampaignVec2 dest;
            string name;
            return HasDestination(out dest, out name);
        }

        public static bool HasDestination(out CampaignVec2 destination, out string destinationName)
        {
            destination = CampaignVec2.Zero;
            destinationName = "None";

            MobileParty main = MobileParty.MainParty;
            if (main == null) return false;

            // Target Settlement
            if (main.TargetSettlement != null)
            {
                destination = main.TargetSettlement.GatePosition;
                destinationName = main.TargetSettlement.Name.ToString();
                return true;
            }
            if (main.ShortTermTargetSettlement != null)
            {
                destination = main.ShortTermTargetSettlement.GatePosition;
                destinationName = main.ShortTermTargetSettlement.Name.ToString();
                return true;
            }

            // Target Party
            if (main.TargetParty != null)
            {
                destination = main.TargetParty.Position;
                destinationName = main.TargetParty.Name.ToString();
                return true;
            }
            if (main.ShortTermTargetParty != null)
            {
                destination = main.ShortTermTargetParty.Position;
                destinationName = main.ShortTermTargetParty.Name.ToString();
                return true;
            }

            // Map target coordinates while moving
            if (main.IsMoving && main.TargetPosition.IsValid() && main.TargetPosition != CampaignVec2.Zero && main.TargetPosition.DistanceSquared(main.Position) > 0.25f)
            {
                destination = main.TargetPosition;
                destinationName = string.Format("Map Location ({0:0.0}, {1:0.0})", destination.X, destination.Y);
                return true;
            }

            if (main.IsMoving && main.AiBehaviorTarget.IsValid() && main.AiBehaviorTarget != CampaignVec2.Zero && main.AiBehaviorTarget.DistanceSquared(main.Position) > 0.25f)
            {
                destination = main.AiBehaviorTarget;
                destinationName = string.Format("Target Location ({0:0.0}, {1:0.0})", destination.X, destination.Y);
                return true;
            }

            // Fallback: Recorded clicked position if moving
            if (main.IsMoving && _hasClickedPosition && _lastClickedPosition.IsValid() && _lastClickedPosition != CampaignVec2.Zero && _lastClickedPosition.DistanceSquared(main.Position) > 0.25f)
            {
                destination = _lastClickedPosition;
                destinationName = string.Format("Clicked Location ({0:0.0}, {1:0.0})", destination.X, destination.Y);
                return true;
            }

            return false;
        }

        public static void OpenNavigationEnhancerMenu()
        {
            CheatSettings settings = CheatSettings.Instance;
            float currentBonus = settings.PartyBaseSpeedBonus;

            CampaignVec2 destPos;
            string destName;
            bool hasDest = HasDestination(out destPos, out destName);

            List<InquiryElement> elements = new List<InquiryElement>();

            // Option 1: Add +5 Base Speed
            float nextAdd = currentBonus + 5f;
            elements.Add(new InquiryElement(
                "add_speed",
                string.Format("➕ Add +5 to Base Speed (Now: {0:+0.0;-0.0;0.0} ➔ {1:+0.0;-0.0;0.0})", currentBonus, nextAdd),
                null,
                true,
                "Adds +5.0 to your party's base movement speed on the campaign map. All multipliers apply on top."
            ));

            // Option 2: Reduce -5 Base Speed
            float nextReduce = currentBonus - 5f;
            elements.Add(new InquiryElement(
                "reduce_speed",
                string.Format("➖ Reduce -5 from Base Speed (Now: {0:+0.0;-0.0;0.0} ➔ {1:+0.0;-0.0;0.0})", currentBonus, nextReduce),
                null,
                true,
                "Reduces party base movement speed by 5.0."
            ));

            // Option 3: Reset Base Speed to 0
            if (Math.Abs(currentBonus) > 0.05f)
            {
                elements.Add(new InquiryElement(
                    "reset_speed",
                    "🔄 Reset Base Speed Bonus to 0",
                    null,
                    true,
                    "Restores default base movement speed (removes speed bonus)."
                ));
            }

            // Option 4: Teleport to Clicked Location
            string teleportTitle;
            string teleportHint;
            if (hasDest)
            {
                teleportTitle = string.Format("📍 Teleport to Clicked Destination: {0}", destName);
                teleportHint = string.Format("Instantly teleports your entire party (and attached army) to {0}.", destName);
            }
            else
            {
                teleportTitle = "📍 Teleport to Clicked Location (No destination set)";
                teleportHint = "This option is currently greyed out because no destination is set yet. Left-click anywhere on the world map to set a destination first.";
            }

            elements.Add(new InquiryElement(
                "teleport",
                teleportTitle,
                null,
                hasDest,
                teleportHint
            ));

            // Option 5: Close Menu
            elements.Add(new InquiryElement(
                "close",
                "❌ Close Menu",
                null,
                true,
                "Close this menu and return to the game."
            ));

            string subTitle = string.Format("Current Base Speed Bonus: {0:+0.0;-0.0;0.0} | Destination: {1}",
                currentBonus,
                hasDest ? destName : "None (click map with Left Mouse)");

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "🧭 Navigation Enhancer",
                subTitle,
                elements,
                true,
                1,
                1,
                "Select",
                "Close",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string action = selected[0].Identifier as string;

                    switch (action)
                    {
                        case "add_speed":
                            settings.PartyBaseSpeedBonus += 5f;
                            settings.Save();
                            RefreshPartySpeed();
                            CheatNotify.Show(string.Format("[RoT Cheats] Party base speed bonus increased to {0:+0.0;-0.0;0.0}!", settings.PartyBaseSpeedBonus));
                            OpenNavigationEnhancerMenu();
                            break;

                        case "reduce_speed":
                            settings.PartyBaseSpeedBonus -= 5f;
                            settings.Save();
                            RefreshPartySpeed();
                            CheatNotify.Show(string.Format("[RoT Cheats] Party base speed bonus reduced to {0:+0.0;-0.0;0.0}!", settings.PartyBaseSpeedBonus));
                            OpenNavigationEnhancerMenu();
                            break;

                        case "reset_speed":
                            settings.PartyBaseSpeedBonus = 0f;
                            settings.Save();
                            RefreshPartySpeed();
                            CheatNotify.Show("[RoT Cheats] Party base speed bonus reset to 0.0!");
                            OpenNavigationEnhancerMenu();
                            break;

                        case "teleport":
                            if (hasDest)
                            {
                                TeleportPartyToDestination(destPos, destName);
                            }
                            break;

                        case "close":
                            // Simply exit without re-opening
                            break;
                    }
                },
                delegate(List<InquiryElement> closed)
                {
                    // Negative option (Close button clicked) - simply exit
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }

        public static void RefreshPartySpeed()
        {
            try
            {
                if (MobileParty.MainParty != null)
                {
                    MobileParty.MainParty.UpdateVersionNo();
                }
            }
            catch
            {
            }
        }

        public static void TeleportPartyToDestination()
        {
            CampaignVec2 dest;
            string name;
            if (HasDestination(out dest, out name))
            {
                TeleportPartyToDestination(dest, name);
            }
            else
            {
                CheatNotify.Show("[RoT Cheats] No destination set! Left-click a destination on the world map first.");
            }
        }

        public static void TeleportPartyToDestination(CampaignVec2 destination, string destinationName)
        {
            MobileParty party = MobileParty.MainParty;
            if (party == null || !destination.IsValid() || destination == CampaignVec2.Zero) return;

            try
            {
                // If player leads an army, teleport all attached parties together
                if (party.Army != null && party.Army.LeaderParty == party)
                {
                    foreach (MobileParty attached in party.Army.Parties)
                    {
                        if (attached != null && attached != party)
                        {
                            attached.Position = destination;
                        }
                    }
                }

                party.Position = destination;
                party.SetMoveModeHold();

                if (MapScreen.Instance != null)
                {
                    MapScreen.Instance.TeleportCameraToMainParty();
                }

                _hasClickedPosition = false;
                CheatNotify.Show(string.Format("[RoT Cheats] Teleported party to {0}!", destinationName));
            }
            catch (Exception ex)
            {
                CheatNotify.Show("[RoT Cheats] Teleport failed: " + ex.Message);
            }
        }

        public static void LocateHeroInWorld(Hero hero)
        {
            if (hero == null) return;

            CampaignVec2 targetPos = CampaignVec2.Zero;
            string locationName = "Unknown";

            if (hero.PartyBelongedTo != null)
            {
                targetPos = hero.PartyBelongedTo.Position;
                locationName = hero.PartyBelongedTo.Name.ToString();
            }
            else if (hero.CurrentSettlement != null)
            {
                targetPos = hero.CurrentSettlement.GatePosition;
                locationName = hero.CurrentSettlement.Name.ToString();
            }
            else if (hero.StayingInSettlement != null)
            {
                targetPos = hero.StayingInSettlement.GatePosition;
                locationName = hero.StayingInSettlement.Name.ToString();
            }
            else if (hero.PartyBelongedToAsPrisoner != null)
            {
                targetPos = hero.PartyBelongedToAsPrisoner.Position;
                locationName = string.Format("Prisoner of {0}", hero.PartyBelongedToAsPrisoner.Name.ToString());
            }
            else if (hero.LastKnownClosestSettlement != null)
            {
                targetPos = hero.LastKnownClosestSettlement.GatePosition;
                locationName = string.Format("Near {0}", hero.LastKnownClosestSettlement.Name.ToString());
            }
            else if (hero.HomeSettlement != null)
            {
                targetPos = hero.HomeSettlement.GatePosition;
                locationName = hero.HomeSettlement.Name.ToString();
            }

            if (!targetPos.IsValid() || targetPos == CampaignVec2.Zero)
            {
                CheatNotify.Show(string.Format("[RoT Cheats] Unable to locate position for {0} (Hero may not be spawned on map).", hero.Name));
                return;
            }

            // Register visual tracker so the hero is highlighted on the map
            try
            {
                if (Campaign.Current != null && Campaign.Current.VisualTrackerManager != null)
                {
                    Campaign.Current.VisualTrackerManager.RegisterObject(hero);
                    if (hero.PartyBelongedTo != null)
                    {
                        Campaign.Current.VisualTrackerManager.RegisterObject(hero.PartyBelongedTo);
                    }
                    else if (hero.CurrentSettlement != null)
                    {
                        Campaign.Current.VisualTrackerManager.RegisterObject(hero.CurrentSettlement);
                    }
                }
            }
            catch
            {
            }

            // Close the Encyclopedia to return to the world view
            try
            {
                if (MapScreen.Instance != null)
                {
                    MapEncyclopediaView encView = MapScreen.Instance.GetMapView<MapEncyclopediaView>();
                    if (encView != null)
                    {
                        encView.CloseEncyclopedia();
                    }
                }
            }
            catch
            {
            }

            try
            {
                if (ScreenManager.TopScreen != null && ScreenManager.TopScreen != MapScreen.Instance)
                {
                    ScreenManager.PopScreen();
                }
            }
            catch
            {
            }

            // Smoothly pan camera to target position
            try
            {
                if (MapScreen.Instance != null)
                {
                    if (MapScreen.Instance.MapCameraView != null)
                    {
                        MapScreen.Instance.MapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.Free);
                    }
                    MapScreen.Instance.FastMoveCameraToPosition(targetPos);
                }
            }
            catch
            {
                try
                {
                    if (MapScreen.Instance != null)
                    {
                        MapScreen.Instance.FastMoveCameraToPosition(targetPos);
                    }
                }
                catch
                {
                }
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Panned camera to {0} ({1})!", hero.Name, locationName));
        }

        public static void OpenExecuteHeroPrompt(Hero hero, Action onExecuted)
        {
            if (hero == null) return;
            if (hero.IsDead)
            {
                CheatNotify.Show(string.Format("[RoT Cheats] {0} is already deceased!", hero.Name));
                return;
            }
            if (hero == Hero.MainHero)
            {
                CheatNotify.Show("[RoT Cheats] You cannot execute your own player character!");
                return;
            }

            string playerName = Hero.MainHero != null ? Hero.MainHero.Name.ToString() : "Player Character";

            List<InquiryElement> elements = new List<InquiryElement>();
            elements.Add(new InquiryElement(
                "player_execution",
                string.Format("🗡️ Executed by {0} (Player Character)", playerName),
                null,
                true,
                "Hero is formally executed by the player character with standard execution journal entry."
            ));
            elements.Add(new InquiryElement(
                "natural_death",
                "🕊️ Natural Death (Old Age / Natural Causes)",
                null,
                true,
                "Hero quietly passes away from natural causes without execution blame or diplomatic consequences."
            ));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                string.Format("Execute Character - {0}", hero.Name.ToString()),
                "Choose the manner of death:",
                elements,
                true,
                1,
                1,
                "Execute",
                "Cancel",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string choice = selected[0].Identifier as string;

                    try
                    {
                        if (choice == "player_execution")
                        {
                            KillCharacterAction.ApplyByExecution(hero, Hero.MainHero, true, true);
                            CheatNotify.Show(string.Format("[RoT Cheats] {0} was executed by {1}!", hero.Name, playerName));
                        }
                        else if (choice == "natural_death")
                        {
                            KillCharacterAction.ApplyByOldAge(hero, true);
                            CheatNotify.Show(string.Format("[RoT Cheats] {0} passed away peacefully of natural causes.", hero.Name));
                        }

                        if (onExecuted != null)
                        {
                            onExecuted();
                        }
                    }
                    catch (Exception ex)
                    {
                        CheatNotify.Show("[RoT Cheats] Execution failed: " + ex.Message);
                    }
                },
                delegate(List<InquiryElement> closed)
                {
                    // Cancelled execution prompt
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
    }
}
