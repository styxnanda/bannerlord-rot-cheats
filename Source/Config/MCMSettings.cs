using System;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using RoTCheats.Cheats;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace RoTCheats.Config
{
    public class MCMSettings : AttributeGlobalSettings<MCMSettings>
    {
        public override string Id
        {
            get { return "ROTCheats_v71"; }
        }

        public override string DisplayName
        {
            get { return "Realm of Thrones Cheats (v7.1)"; }
        }

        public override string FolderName
        {
            get { return "ROTCheats"; }
        }

        public override string FormatType
        {
            get { return "json2"; }
        }

        #region 1. Combat Cheats
        [SettingPropertyBool("God Mode (Player & Mount)", Order = 1, RequireRestart = false, HintText = "Makes you and your dragon/mount immune to all incoming battle damage and keeps health at 100%.")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public bool GodMode { get; set; }

        [SettingPropertyBool("Party God Mode", Order = 2, RequireRestart = false, HintText = "Makes all allied soldiers and companions in your squad immune to battle damage.")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public bool PartyGodMode { get; set; }

        [SettingPropertyBool("Unlimited Ammo", Order = 3, RequireRestart = false, HintText = "Continuously refills all arrows, bolts, and throwing weapons during battle.")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public bool UnlimitedAmmo { get; set; }

        [SettingPropertyBool("One-Hit Kill (Shield & Guard Piercing)", Order = 4, RequireRestart = false, HintText = "Strikes dealt by you or squad inflict lethal 99,999 damage, piercing enemy shields, parries, and blocks. Never applies to enemies.")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public bool OneHitKill { get; set; }

        [SettingPropertyFloatingInteger("Player Speed Multiplier", 1.0f, 5.0f, "#0.0x", Order = 5, RequireRestart = false, HintText = "Movement speed multiplier for the player character on foot in battle (1.0x = Normal).")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public float PlayerSpeedMultiplier { get; set; }

        [SettingPropertyFloatingInteger("Squad Speed Multiplier", 1.0f, 5.0f, "#0.0x", Order = 6, RequireRestart = false, HintText = "Movement speed multiplier for allied squad soldiers on foot in battle (1.0x = Normal).")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public float SquadSpeedMultiplier { get; set; }

        [SettingPropertyFloatingInteger("Horseback & Mount Speed Multiplier", 1.0f, 5.0f, "#0.0x", Order = 7, RequireRestart = false, HintText = "Speed and maneuver multiplier for player and squad mounts/dragons when riding (1.0x = Normal).")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public float HorseSpeedMultiplier { get; set; }

        [SettingPropertyButton("Heal Player & Dragon Now", Content = "Heal Now", Order = 8, RequireRestart = false, HintText = "Instantly sets Health to 100% for you and your dragon/mount.")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public Action HealPlayerButton
        {
            get { return delegate { CombatCheats.HealPlayerAndMount(); }; }
            set { }
        }

        [SettingPropertyButton("Heal Entire Squad / Party", Content = "Heal Squad", Order = 9, RequireRestart = false, HintText = "Heals all active units in battle and clears wounded status in campaign party.")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public Action HealPartyButton
        {
            get { return delegate { CombatCheats.HealParty(); }; }
            set { }
        }

        [SettingPropertyButton("Defeat All Enemies", Content = "Win Battle", Order = 10, RequireRestart = false, HintText = "Instantly eliminates all active enemy soldiers in the current battle.")]
        [SettingPropertyGroup("1. Combat Cheats")]
        public Action DefeatEnemiesButton
        {
            get { return delegate { CombatCheats.WoundAllEnemies(); }; }
            set { }
        }
        #endregion

        #region 2. Dragons (Realm of Thrones)
        [SettingPropertyButton("⭐ Spawn ALL Dragons & Saddles", Content = "Spawn All", Order = 1, RequireRestart = false, HintText = "Adds 1 of every ground & flying dragon plus 5 armored saddles to party inventory!")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnAllDragonsButton
        {
            get { return delegate { DragonSpawner.SpawnAllDragons(); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Drogon (Ground)", Content = "Spawn & Equip", Order = 2, RequireRestart = false, HintText = "Spawns Drogon (walking) into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnDrogonGroundButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_black", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Drogon (Flying)", Content = "Spawn & Equip", Order = 3, RequireRestart = false, HintText = "Spawns Flying Drogon into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnDrogonFlyButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_black2", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Rhaegal (Ground)", Content = "Spawn & Equip", Order = 4, RequireRestart = false, HintText = "Spawns Rhaegal (walking) into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnRhaegalGroundButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_brown", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Rhaegal (Flying)", Content = "Spawn & Equip", Order = 5, RequireRestart = false, HintText = "Spawns Flying Rhaegal into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnRhaegalFlyButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_brown2", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Viserion (Ground)", Content = "Spawn & Equip", Order = 6, RequireRestart = false, HintText = "Spawns Viserion (walking) into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnViserionGroundButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_green", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Viserion (Flying)", Content = "Spawn & Equip", Order = 7, RequireRestart = false, HintText = "Spawns Flying Viserion into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnViserionFlyButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_green2", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Draeghar (Ground)", Content = "Spawn & Equip", Order = 8, RequireRestart = false, HintText = "Spawns Draeghar (walking) into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnDraegharGroundButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_red", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Draeghar (Flying)", Content = "Spawn & Equip", Order = 9, RequireRestart = false, HintText = "Spawns Flying Draeghar into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnDraegharFlyButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_red2", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Gold Dragon", Content = "Spawn & Equip", Order = 10, RequireRestart = false, HintText = "Spawns Gold Dragon into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnGoldDragonButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_gold2", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Sea Smoke", Content = "Spawn & Equip", Order = 11, RequireRestart = false, HintText = "Spawns Sea Smoke into party and equips as active mount.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnSeaSmokeButton
        {
            get { return delegate { DragonSpawner.SpawnDragon("dragon_smoke2", true); }; }
            set { }
        }

        [SettingPropertyButton("Spawn Armored Dragon Saddle", Content = "Spawn & Equip", Order = 12, RequireRestart = false, HintText = "Spawns and equips the heavy Armored Dragon Saddle.")]
        [SettingPropertyGroup("2. Dragons (Realm of Thrones)")]
        public Action SpawnSaddleButton
        {
            get
            {
                return delegate
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
                };
            }
            set { }
        }
        #endregion

        #region 3. Items & Economy
        [SettingPropertyBool("Unlimited Weight Limit", Order = 0, RequireRestart = false, HintText = "Sets party carrying capacity to 10,000,000 and eliminates all overburdened and cargo weight penalties.")]
        [SettingPropertyGroup("3. Items & Economy")]
        public bool UnlimitedWeight { get; set; }

        [SettingPropertyButton("Open Full Item Spawner Screen", Content = "Open Spawner", Order = 1, RequireRestart = false, HintText = "Opens the native loot screen with 100x of ALL items in the game.")]
        [SettingPropertyGroup("3. Items & Economy")]
        public Action OpenSpawnerButton
        {
            get { return delegate { ItemCheats.OpenCheatInventory(); }; }
            set { }
        }

        [SettingPropertyButton("Give All 28 Legendary RoT Weapons", Content = "Add Weapons", Order = 2, RequireRestart = false, HintText = "Adds Longclaw, Ice, Dark Sister, Blackfyre, Heartsbane, Oathkeeper, Widow's Wail, Dawn, and more.")]
        [SettingPropertyGroup("3. Items & Economy")]
        public Action GiveWeaponsButton
        {
            get { return delegate { ItemCheats.GiveAllLegendaryWeapons(); }; }
            set { }
        }

        [SettingPropertyButton("Give All RoT Valyrian & Targaryen Armors", Content = "Add Armors", Order = 3, RequireRestart = false, HintText = "Adds Blackfyre plate, Rhaegar plate, Valyrian soldier armor, helmets, and shields.")]
        [SettingPropertyGroup("3. Items & Economy")]
        public Action GiveArmorsButton
        {
            get { return delegate { ItemCheats.GiveAllArmors(); }; }
            set { }
        }

        [SettingPropertyButton("Give 999x All Smithing Materials", Content = "Add Materials", Order = 4, RequireRestart = false, HintText = "Adds 999x Tamaskene steel, fine steel, steel, iron, crude iron, hardwood, and charcoal.")]
        [SettingPropertyGroup("3. Items & Economy")]
        public Action GiveSmithingMaterialsButton
        {
            get { return delegate { ItemCheats.GiveSmithingMaterials(999); }; }
            set { }
        }

        [SettingPropertyButton("Give 100x All Food Provisions", Content = "Add Food", Order = 5, RequireRestart = false, HintText = "Adds 100x grain, meat, fish, cheese, butter, grapes, olives, beer, wine, and dates.")]
        [SettingPropertyGroup("3. Items & Economy")]
        public Action GiveFoodButton
        {
            get { return delegate { ItemCheats.GiveFoodProvisions(100); }; }
            set { }
        }

        [SettingPropertyButton("Give Wealth & Clan Stats (+1M Gold)", Content = "Add Wealth", Order = 6, RequireRestart = false, HintText = "Grants +1,000,000 Gold, +10,000 Clan Renown, and +10,000 Clan Influence.")]
        [SettingPropertyGroup("3. Items & Economy")]
        public Action GiveWealthButton
        {
            get { return delegate { ItemCheats.GiveWealthAndStats(1000000, 10000f, 10000f); }; }
            set { }
        }
        #endregion

        #region 4. Crafting & Stamina
        [SettingPropertyBool("Infinite Smithing Stamina", Order = 1, RequireRestart = false, HintText = "Reduces crafting, refining, and smelting energy costs to 0 and maintains full stamina.")]
        [SettingPropertyGroup("4. Crafting & Stamina")]
        public bool InfiniteStamina { get; set; }

        [SettingPropertyButton("Unlock ALL Weapon Crafting Parts", Content = "Unlock All Parts", Order = 2, RequireRestart = false, HintText = "Instantly unlocks all blades, guards, handles, and pommels in the smithy.")]
        [SettingPropertyGroup("4. Crafting & Stamina")]
        public Action UnlockPartsButton
        {
            get { return delegate { ItemCheats.UnlockAllCraftingPieces(); }; }
            set { }
        }
        #endregion

        #region 5. Skills & Progression
        [SettingPropertyButton("Max All Skills to 300 (Main Hero)", Content = "Max to 300", Order = 1, RequireRestart = false, HintText = "Sets all 18 skills to 300 for the Main Hero.")]
        [SettingPropertyGroup("5. Skills & Progression")]
        public Action MaxSkills300Button
        {
            get { return delegate { if (Hero.MainHero != null) SkillCheats.MaxAllSkills(Hero.MainHero, 300); }; }
            set { }
        }

        [SettingPropertyButton("Set All Skills to 500 (Master Hero)", Content = "Set to 500", Order = 2, RequireRestart = false, HintText = "Sets all 18 skills to 500 for extreme performance.")]
        [SettingPropertyGroup("5. Skills & Progression")]
        public Action MaxSkills500Button
        {
            get { return delegate { if (Hero.MainHero != null) SkillCheats.MaxAllSkills(Hero.MainHero, 500); }; }
            set { }
        }

        [SettingPropertyButton("Add +50,000 XP to All Skills", Content = "Add XP", Order = 3, RequireRestart = false, HintText = "Grants +50,000 XP across all skill categories for Main Hero.")]
        [SettingPropertyGroup("5. Skills & Progression")]
        public Action AddSkillXpButton
        {
            get { return delegate { if (Hero.MainHero != null) SkillCheats.AddSkillXpToAll(Hero.MainHero, 50000f); }; }
            set { }
        }

        [SettingPropertyButton("Add +50 Focus & +20 Attribute Points", Content = "Add Points", Order = 4, RequireRestart = false, HintText = "Grants unspent points to assign freely on character sheet.")]
        [SettingPropertyGroup("5. Skills & Progression")]
        public Action AddPointsButton
        {
            get { return delegate { if (Hero.MainHero != null) SkillCheats.AddPoints(Hero.MainHero, 50, 20); }; }
            set { }
        }

        [SettingPropertyButton("Level Up Hero (+5 Levels)", Content = "Level Up +5", Order = 5, RequireRestart = false, HintText = "Increases Main Hero character level by 5.")]
        [SettingPropertyGroup("5. Skills & Progression")]
        public Action LevelUp5Button
        {
            get { return delegate { if (Hero.MainHero != null) SkillCheats.LevelUp(Hero.MainHero, 5); }; }
            set { }
        }
        #endregion

        #region 6. Party & Kingdom
        [SettingPropertyButton("Set Party Morale to 100 (Max)", Content = "Max Morale", Order = 1, RequireRestart = false, HintText = "Boosts party morale to 100 so troops never desert.")]
        [SettingPropertyGroup("6. Party & Kingdom")]
        public Action MaxMoraleButton
        {
            get
            {
                return delegate
                {
                    if (MobileParty.MainParty != null)
                    {
                        MobileParty.MainParty.RecentEventsMorale = 100f;
                        CheatNotify.Show("[RoT Cheats] Party morale boosted to 100!");
                    }
                };
            }
            set { }
        }

        [SettingPropertyButton("Upgrade All Ready Troops", Content = "Upgrade All", Order = 2, RequireRestart = false, HintText = "Upgrades all troops currently eligible for promotion.")]
        [SettingPropertyGroup("6. Party & Kingdom")]
        public Action UpgradeTroopsButton
        {
            get
            {
                return delegate
                {
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
                };
            }
            set { }
        }
        #endregion

        public MCMSettings()
        {
            GodMode = false;
            PartyGodMode = false;
            InfiniteStamina = true;
            UnlimitedAmmo = false;
            OneHitKill = false;
            PlayerSpeedMultiplier = 1.0f;
            SquadSpeedMultiplier = 1.0f;
            HorseSpeedMultiplier = 1.0f;
            UnlimitedWeight = true;
        }
    }
}
