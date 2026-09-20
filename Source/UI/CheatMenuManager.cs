using System;
using System.Collections.Generic;
using RoTCheats.Cheats;
using RoTCheats.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace RoTCheats.UI
{
    public static class CheatMenuManager
    {
        public static void OpenMainMenu()
        {
            CheatSettings settings = CheatSettings.Instance;

            List<InquiryElement> elements = new List<InquiryElement>();
            elements.Add(new InquiryElement("dragons", "🐉 Realm of Thrones Dragons", null, true, "Spawn Drogon, Rhaegal, Viserion, Draeghar (Ground or Flying) & Armored Dragon Saddle."));
            elements.Add(new InquiryElement("combat", "🛡️ God Mode & Combat Cheats", null, true, string.Format("God Mode: {0} | Party God: {1} | Ammo: {2} | One-Hit: {3}",
                settings.GodMode ? "ON" : "OFF",
                settings.PartyGodMode ? "ON" : "OFF",
                settings.UnlimitedAmmo ? "ON" : "OFF",
                settings.OneHitKill ? "ON" : "OFF")));
            elements.Add(new InquiryElement("speed", "🏃 Speed Modifiers (Player, Squad, Mount)", null, true, string.Format("Player: {0:0.0}x | Squad: {1:0.0}x | Horseback: {2:0.0}x",
                settings.PlayerSpeedMultiplier, settings.SquadSpeedMultiplier, settings.HorseSpeedMultiplier)));
            elements.Add(new InquiryElement("items", "⚔️ Unlimited Items & Spawner", null, true, string.Format("Unlimited Weight: {0} | In-game item spawner, Valyrian steel, armor, materials.",
                settings.UnlimitedWeight ? "ON" : "OFF")));
            elements.Add(new InquiryElement("stamina", "⚒️ Unlimited Stamina & Crafting", null, true, string.Format("Infinite Smithing Stamina: {0} | Unlock all crafting pieces.", settings.InfiniteStamina ? "ON" : "OFF")));
            elements.Add(new InquiryElement("skills", "⭐ Modify Skills, Levels & XP", null, true, "Set skills to 300+, add focus/attribute points, and level up Main Hero or Companions."));
            elements.Add(new InquiryElement("party", "👑 Party & Kingdom Cheats", null, true, "Max party morale, instant troop upgrades, renown, and influence."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "Realm of Thrones Cheats (v7.1)",
                "Select a cheat category:",
                elements,
                false,
                1,
                1,
                "Open",
                "Close",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;
                    switch (id)
                    {
                        case "dragons":
                            OpenDragonsMenu();
                            break;
                        case "combat":
                            OpenCombatMenu();
                            break;
                        case "speed":
                            OpenSpeedMenu();
                            break;
                        case "items":
                            OpenItemsMenu();
                            break;
                        case "stamina":
                            OpenStaminaMenu();
                            break;
                        case "skills":
                            OpenSkillsHeroPicker();
                            break;
                        case "party":
                            OpenPartyMenu();
                            break;
                    }
                },
                null,
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }

        #region Dragons Menu
        public static void OpenDragonsMenu()
        {
            List<InquiryElement> elements = new List<InquiryElement>();

            elements.Add(new InquiryElement("all_dragons", "⭐ SPAWN ALL DRAGONS & SADDLES BUNDLE", null, true, "Adds 1 of every ground and flying dragon plus 5 armored saddles to party inventory!"));
            elements.Add(new InquiryElement("armored_dragon_saddle", "Armored Dragon Saddle", null, true, "Official heavy armored saddle for dragons."));

            foreach (DragonInfo info in DragonSpawner.Dragons)
            {
                elements.Add(new InquiryElement(info.Id, info.Name, null, true, string.Format("Spawns {0} into party and equips as active battle mount.", info.Name)));
            }

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "Realm of Thrones - Dragon Spawner",
                "Choose a dragon or saddle to spawn:",
                elements,
                true,
                1,
                1,
                "Spawn & Equip",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;

                    if (id == "all_dragons")
                    {
                        DragonSpawner.SpawnAllDragons();
                    }
                    else if (id == DragonSpawner.SaddleId)
                    {
                        ItemObject saddle = DragonSpawner.FindItem(DragonSpawner.SaddleId);
                        if (saddle != null && MobileParty.MainParty != null)
                        {
                            MobileParty.MainParty.ItemRoster.AddToCounts(saddle, 1);
                            if (Hero.MainHero != null)
                            {
                                Hero.MainHero.BattleEquipment[EquipmentIndex.HorseHarness] = new EquipmentElement(saddle);
                            }
                            CheatNotify.Show("[RoT Cheats] Armored Dragon Saddle added and equipped!");
                        }
                    }
                    else
                    {
                        DragonSpawner.SpawnDragon(id, true);
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenMainMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
        #endregion

        #region Combat Menu
        public static void OpenCombatMenu()
        {
            CheatSettings settings = CheatSettings.Instance;
            List<InquiryElement> elements = new List<InquiryElement>();

            elements.Add(new InquiryElement("toggle_god", string.Format("Toggle God Mode (Player & Mount) [{0}]", settings.GodMode ? "ENABLED" : "DISABLED"), null, true, "Blocks all incoming damage to you and your dragon/mount and maintains 100% HP."));
            elements.Add(new InquiryElement("toggle_party_god", string.Format("Toggle Party God Mode [{0}]", settings.PartyGodMode ? "ENABLED" : "DISABLED"), null, true, "Blocks all incoming damage to your entire army."));
            elements.Add(new InquiryElement("toggle_ammo", string.Format("Toggle Unlimited Ammo [{0}]", settings.UnlimitedAmmo ? "ENABLED" : "DISABLED"), null, true, "Continuously refills all arrows, bolts, and throwing weapons in battle."));
            elements.Add(new InquiryElement("toggle_onehit", string.Format("Toggle One-Hit Kill (Pierces Shields & Guards) [{0}]", settings.OneHitKill ? "ENABLED" : "DISABLED"), null, true, "Inflicts 99,999 lethal damage, breaks enemy shields and crushes through enemy parries and blocks. Never applies to enemies."));
            elements.Add(new InquiryElement("open_speed", string.Format("🏃 Battle Speed Modifiers (P:{0:0.0}x | S:{1:0.0}x | H:{2:0.0}x)", settings.PlayerSpeedMultiplier, settings.SquadSpeedMultiplier, settings.HorseSpeedMultiplier), null, true, "Adjust movement speed for player on foot, squad on foot, and when on horseback/mount."));
            elements.Add(new InquiryElement("heal_player", "Heal Player & Dragon Now", null, true, "Instantly sets Health to 100% for you and your mount."));
            elements.Add(new InquiryElement("heal_party", "Heal Party & Revive All Wounded Troops", null, true, "Heals all active units in battle and clears wounded status in campaign party."));
            elements.Add(new InquiryElement("wound_enemies", "Wound / Defeat All Enemies (Win Battle)", null, true, "Instantly eliminates all active enemy soldiers in the current battle."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "God Mode & Combat Cheats",
                "Select a combat cheat to toggle or trigger:",
                elements,
                false,
                1,
                1,
                "Select",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;

                    switch (id)
                    {
                        case "toggle_god":
                            settings.GodMode = !settings.GodMode;
                            settings.Save();
                            CheatNotify.Show("[RoT Cheats] God Mode is now " + (settings.GodMode ? "ENABLED" : "DISABLED"));
                            OpenCombatMenu();
                            break;
                        case "toggle_party_god":
                            settings.PartyGodMode = !settings.PartyGodMode;
                            settings.Save();
                            CheatNotify.Show("[RoT Cheats] Party God Mode is now " + (settings.PartyGodMode ? "ENABLED" : "DISABLED"));
                            OpenCombatMenu();
                            break;
                        case "toggle_ammo":
                            settings.UnlimitedAmmo = !settings.UnlimitedAmmo;
                            settings.Save();
                            CheatNotify.Show("[RoT Cheats] Unlimited Ammo is now " + (settings.UnlimitedAmmo ? "ENABLED" : "DISABLED"));
                            OpenCombatMenu();
                            break;
                        case "toggle_onehit":
                            settings.OneHitKill = !settings.OneHitKill;
                            settings.Save();
                            CheatNotify.Show("[RoT Cheats] One-Hit Kill (Shield & Guard Piercing) is now " + (settings.OneHitKill ? "ENABLED" : "DISABLED"));
                            OpenCombatMenu();
                            break;
                        case "open_speed":
                            OpenSpeedMenu();
                            break;
                        case "heal_player":
                            CombatCheats.HealPlayerAndMount();
                            break;
                        case "heal_party":
                            CombatCheats.HealParty();
                            break;
                        case "wound_enemies":
                            CombatCheats.WoundAllEnemies();
                            break;
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenMainMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }

        public static void OpenSpeedMenu()
        {
            CheatSettings settings = CheatSettings.Instance;
            List<InquiryElement> elements = new List<InquiryElement>();

            elements.Add(new InquiryElement("player_speed", string.Format("Player Foot Speed: {0:0.0}x", settings.PlayerSpeedMultiplier), null, true, "Configure player character running/walking speed multiplier in battle."));
            elements.Add(new InquiryElement("squad_speed", string.Format("Squad Foot Speed: {0:0.0}x", settings.SquadSpeedMultiplier), null, true, "Configure allied soldiers and companions speed multiplier on foot in battle."));
            elements.Add(new InquiryElement("horse_speed", string.Format("Horseback & Mount Speed: {0:0.0}x", settings.HorseSpeedMultiplier), null, true, "Configure speed and maneuver multiplier for player and squad mounts/dragons."));
            elements.Add(new InquiryElement("reset_speeds", "Reset All Speeds to Normal (1.0x)", null, true, "Restores default speeds for player, squad, and mounts."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "Speed Modifiers (Battle & Horseback)",
                "Select a speed category to modify:",
                elements,
                false,
                1,
                1,
                "Select",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;

                    switch (id)
                    {
                        case "player_speed":
                            OpenSpeedPresetPicker("Player Foot Speed", settings.PlayerSpeedMultiplier, delegate(float val)
                            {
                                settings.PlayerSpeedMultiplier = val;
                                settings.Save();
                                CheatNotify.Show(string.Format("[RoT Cheats] Player Foot Speed set to {0:0.0}x", val));
                                OpenSpeedMenu();
                            });
                            break;
                        case "squad_speed":
                            OpenSpeedPresetPicker("Squad Foot Speed", settings.SquadSpeedMultiplier, delegate(float val)
                            {
                                settings.SquadSpeedMultiplier = val;
                                settings.Save();
                                CheatNotify.Show(string.Format("[RoT Cheats] Squad Foot Speed set to {0:0.0}x", val));
                                OpenSpeedMenu();
                            });
                            break;
                        case "horse_speed":
                            OpenSpeedPresetPicker("Horseback & Mount Speed", settings.HorseSpeedMultiplier, delegate(float val)
                            {
                                settings.HorseSpeedMultiplier = val;
                                settings.Save();
                                CheatNotify.Show(string.Format("[RoT Cheats] Horseback Speed set to {0:0.0}x", val));
                                OpenSpeedMenu();
                            });
                            break;
                        case "reset_speeds":
                            settings.PlayerSpeedMultiplier = 1.0f;
                            settings.SquadSpeedMultiplier = 1.0f;
                            settings.HorseSpeedMultiplier = 1.0f;
                            settings.Save();
                            CheatNotify.Show("[RoT Cheats] All speeds reset to 1.0x (Normal)!");
                            OpenSpeedMenu();
                            break;
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenMainMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }

        private static void OpenSpeedPresetPicker(string title, float current, Action<float> onSelected)
        {
            List<InquiryElement> elements = new List<InquiryElement>();
            elements.Add(new InquiryElement(1.0f, "1.0x - Normal Speed" + (Math.Abs(current - 1.0f) < 0.05f ? " [CURRENT]" : ""), null, true, "Default game speed."));
            elements.Add(new InquiryElement(1.25f, "1.25x - Fast Speed" + (Math.Abs(current - 1.25f) < 0.05f ? " [CURRENT]" : ""), null, true, "25% speed increase."));
            elements.Add(new InquiryElement(1.5f, "1.5x - Super Fast" + (Math.Abs(current - 1.5f) < 0.05f ? " [CURRENT]" : ""), null, true, "50% speed increase."));
            elements.Add(new InquiryElement(2.0f, "2.0x - Double Speed" + (Math.Abs(current - 2.0f) < 0.05f ? " [CURRENT]" : ""), null, true, "100% speed increase (2x)."));
            elements.Add(new InquiryElement(3.0f, "3.0x - Triple Speed" + (Math.Abs(current - 3.0f) < 0.05f ? " [CURRENT]" : ""), null, true, "200% speed increase (3x)."));
            elements.Add(new InquiryElement(5.0f, "5.0x - Sonic Speed" + (Math.Abs(current - 5.0f) < 0.05f ? " [CURRENT]" : ""), null, true, "Extreme speed (5x)."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                title,
                "Select a speed multiplier preset:",
                elements,
                false,
                1,
                1,
                "Apply",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    if (selected[0].Identifier is float)
                    {
                        float val = (float)selected[0].Identifier;
                        onSelected(val);
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenSpeedMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
        #endregion

        #region Items Menu
        public static void OpenItemsMenu()
        {
            CheatSettings settings = CheatSettings.Instance;
            List<InquiryElement> elements = new List<InquiryElement>();

            elements.Add(new InquiryElement("toggle_weight", string.Format("Toggle Unlimited Weight Limit [{0}]", settings.UnlimitedWeight ? "ENABLED" : "DISABLED"), null, true, "Sets carrying capacity to 10,000,000 and eliminates overburdened movement penalties on the world map."));
            elements.Add(new InquiryElement("open_spawner", "📦 Open Full In-Game Item Spawner Screen", null, true, "Opens the native loot transfer screen populated with ALL items in the game (100x each)."));
            elements.Add(new InquiryElement("rot_weapons", "🗡️ Give All 28 Legendary RoT Valyrian & Unique Weapons", null, true, "Adds Longclaw, Ice, Dark Sister, Blackfyre, Heartsbane, Oathkeeper, Widow's Wail, Dawn, and more."));
            elements.Add(new InquiryElement("rot_armors", "🛡️ Give All RoT Valyrian & Targaryen Armor Sets", null, true, "Adds Blackfyre plate, Rhaegar plate, Valyrian soldier armor, helmets, and shields."));
            elements.Add(new InquiryElement("smithing_mats", "⚒️ Give 999x of All Smithing Materials", null, true, "Adds 999x Tamaskene steel, fine steel, steel, iron, wrought iron, crude iron, hardwood, and charcoal."));
            elements.Add(new InquiryElement("food", "🍞 Give 100x of All Food Provisions", null, true, "Adds 100x grain, meat, fish, cheese, butter, grapes, olives, beer, wine, and dates."));
            elements.Add(new InquiryElement("wealth", "💰 Give 1,000,000 Gold, 10,000 Renown, 10,000 Influence", null, true, "Boosts personal gold and clan renown & influence."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "Unlimited Items & Spawner",
                "Choose an item cheat option:",
                elements,
                false,
                1,
                1,
                "Select",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;

                    switch (id)
                    {
                        case "toggle_weight":
                            settings.UnlimitedWeight = !settings.UnlimitedWeight;
                            settings.Save();
                            CheatNotify.Show("[RoT Cheats] Unlimited Weight Limit is now " + (settings.UnlimitedWeight ? "ENABLED" : "DISABLED"));
                            OpenItemsMenu();
                            break;
                        case "open_spawner":
                            ItemCheats.OpenCheatInventory();
                            break;
                        case "rot_weapons":
                            ItemCheats.GiveAllLegendaryWeapons();
                            break;
                        case "rot_armors":
                            ItemCheats.GiveAllArmors();
                            break;
                        case "smithing_mats":
                            ItemCheats.GiveSmithingMaterials(999);
                            break;
                        case "food":
                            ItemCheats.GiveFoodProvisions(100);
                            break;
                        case "wealth":
                            ItemCheats.GiveWealthAndStats(1000000, 10000f, 10000f);
                            break;
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenMainMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
        #endregion

        #region Stamina Menu
        public static void OpenStaminaMenu()
        {
            CheatSettings settings = CheatSettings.Instance;
            List<InquiryElement> elements = new List<InquiryElement>();

            elements.Add(new InquiryElement("toggle_stamina", string.Format("Toggle Infinite Smithing Stamina [{0}]", settings.InfiniteStamina ? "ENABLED" : "DISABLED"), null, true, "Reduces crafting, refining, and smelting energy costs to 0 and maintains full stamina."));
            elements.Add(new InquiryElement("unlock_parts", "🔓 Unlock ALL Weapon Crafting Parts & Templates", null, true, "Instantly unlocks all blades, guards, handles, and pommels in the smithy."));
            elements.Add(new InquiryElement("refill_stamina", "⚡ Refill Smithing Stamina (100% Full)", null, true, "Refills current smithing stamina for all heroes in party immediately."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "Unlimited Stamina & Smithing",
                "Choose a stamina cheat option:",
                elements,
                false,
                1,
                1,
                "Select",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;

                    switch (id)
                    {
                        case "toggle_stamina":
                            settings.InfiniteStamina = !settings.InfiniteStamina;
                            settings.Save();
                            CheatNotify.Show("[RoT Cheats] Infinite Smithing Stamina is now " + (settings.InfiniteStamina ? "ENABLED" : "DISABLED"));
                            OpenStaminaMenu();
                            break;
                        case "unlock_parts":
                            ItemCheats.UnlockAllCraftingPieces();
                            break;
                        case "refill_stamina":
                            CheatNotify.Show("[RoT Cheats] Smithing stamina refilled!");
                            break;
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenMainMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
        #endregion

        #region Skills Menu
        public static void OpenSkillsHeroPicker()
        {
            List<InquiryElement> elements = new List<InquiryElement>();

            if (Hero.MainHero != null)
            {
                elements.Add(new InquiryElement("main_hero", string.Format("{0} (Main Hero)", Hero.MainHero.Name.ToString()), null, true, "Modify player character skills & XP."));
            }

            if (Hero.MainHero != null && Hero.MainHero.CompanionsInParty != null)
            {
                foreach (Hero companion in Hero.MainHero.CompanionsInParty)
                {
                    if (companion != null && companion != Hero.MainHero)
                    {
                        elements.Add(new InquiryElement(companion, string.Format("{0} (Companion)", companion.Name.ToString()), null, true, string.Format("Level {0}", companion.Level)));
                    }
                }
            }

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "Modify Skills, Levels & XP - Select Hero",
                "Select which character to modify:",
                elements,
                false,
                1,
                1,
                "Select",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    Hero targetHero = null;
                    if (selected[0].Identifier as string == "main_hero")
                    {
                        targetHero = Hero.MainHero;
                    }
                    else
                    {
                        targetHero = selected[0].Identifier as Hero;
                    }

                    if (targetHero != null)
                    {
                        OpenSkillsActionMenu(targetHero);
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenMainMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }

        public static void OpenSkillsActionMenu(Hero hero)
        {
            List<InquiryElement> elements = new List<InquiryElement>();

            elements.Add(new InquiryElement("max_300", "Max All Skills to 300", null, true, "Sets One-Handed, Two-Handed, Polearm, Bow, Riding, Smithing, etc. to 300."));
            elements.Add(new InquiryElement("max_500", "Set All Skills to 500 (Master)", null, true, "Sets all 18 skills to 500 for extreme performance."));
            elements.Add(new InquiryElement("add_xp", "Add +50,000 XP to All Skills", null, true, "Grants 50,000 XP across all skill categories."));
            elements.Add(new InquiryElement("add_points", "Add +50 Focus Points & +20 Attribute Points", null, true, "Grants unspent points to assign freely."));
            elements.Add(new InquiryElement("level_5", "Level Up (+5 Levels)", null, true, "Increases character level by 5."));
            elements.Add(new InquiryElement("level_20", "Level Up (+20 Levels)", null, true, "Increases character level by 20."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                string.Format("Modify Skills - {0}", hero.Name.ToString()),
                string.Format("Current Level: {0} | Unspent Focus: {1} | Unspent Attr: {2}", hero.Level, hero.HeroDeveloper != null ? hero.HeroDeveloper.UnspentFocusPoints : 0, hero.HeroDeveloper != null ? hero.HeroDeveloper.UnspentAttributePoints : 0),
                elements,
                false,
                1,
                1,
                "Apply",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string action = selected[0].Identifier as string;

                    switch (action)
                    {
                        case "max_300":
                            SkillCheats.MaxAllSkills(hero, 300);
                            break;
                        case "max_500":
                            SkillCheats.MaxAllSkills(hero, 500);
                            break;
                        case "add_xp":
                            SkillCheats.AddSkillXpToAll(hero, 50000f);
                            break;
                        case "add_points":
                            SkillCheats.AddPoints(hero, 50, 20);
                            break;
                        case "level_5":
                            SkillCheats.LevelUp(hero, 5);
                            break;
                        case "level_20":
                            SkillCheats.LevelUp(hero, 20);
                            break;
                    }
                    OpenSkillsActionMenu(hero);
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenSkillsHeroPicker();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
        #endregion

        #region Party Menu
        public static void OpenPartyMenu()
        {
            List<InquiryElement> elements = new List<InquiryElement>();

            elements.Add(new InquiryElement("max_morale", "Set Party Morale to 100 (Max)", null, true, "Sets party morale to 100 so troops never desert."));
            elements.Add(new InquiryElement("upgrade_troops", "Upgrade All Ready Troops Instantly", null, true, "Upgrades all troops currently eligible for promotion."));
            elements.Add(new InquiryElement("add_influence", "Add +10,000 Clan Influence", null, true, "Grants influence to vote on kingdom policies and armies."));
            elements.Add(new InquiryElement("add_renown", "Add +10,000 Clan Renown", null, true, "Increases Clan Tier and party size limits."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "Party & Kingdom Cheats",
                "Select a party cheat:",
                elements,
                false,
                1,
                1,
                "Select",
                "Back",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;

                    switch (id)
                    {
                        case "max_morale":
                            if (MobileParty.MainParty != null)
                            {
                                MobileParty.MainParty.RecentEventsMorale = 100f;
                                CheatNotify.Show("[RoT Cheats] Party morale boosted to 100!");
                            }
                            break;
                        case "upgrade_troops":
                            if (MobileParty.MainParty != null)
                            {
                                int upgraded = 0;
                                for (int i = 0; i < MobileParty.MainParty.MemberRoster.Count; i++)
                                {
                                    var element = MobileParty.MainParty.MemberRoster.GetElementCopyAtIndex(i);
                                    if (element.Character != null && element.Character.UpgradeTargets != null && element.Character.UpgradeTargets.Length > 0)
                                    {
                                        int upgradeable = MobileParty.MainParty.MemberRoster.GetElementNumber(i);
                                        if (upgradeable > 0)
                                        {
                                            CharacterObject nextTroop = element.Character.UpgradeTargets[0];
                                            MobileParty.MainParty.MemberRoster.AddToCounts(element.Character, -upgradeable, false, 0, 0, true, -1);
                                            MobileParty.MainParty.MemberRoster.AddToCounts(nextTroop, upgradeable, false, 0, 0, true, -1);
                                            upgraded += upgradeable;
                                        }
                                    }
                                }
                                CheatNotify.Show(string.Format("[RoT Cheats] Upgraded {0} troops in party!", upgraded));
                            }
                            break;
                        case "add_influence":
                            if (Clan.PlayerClan != null)
                            {
                                ChangeClanInfluenceAction.Apply(Clan.PlayerClan, 10000f);
                                CheatNotify.Show("[RoT Cheats] Granted +10,000 Clan Influence!");
                            }
                            break;
                        case "add_renown":
                            if (Clan.PlayerClan != null)
                            {
                                Clan.PlayerClan.AddRenown(10000f, true);
                                CheatNotify.Show("[RoT Cheats] Granted +10,000 Clan Renown!");
                            }
                            break;
                    }
                },
                delegate(List<InquiryElement> cancel)
                {
                    OpenMainMenu();
                },
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
        #endregion
    }
}
