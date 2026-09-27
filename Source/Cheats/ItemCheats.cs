using System;
using System.Collections.Generic;
using System.Reflection;
using Helpers;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace RoTCheats.Cheats
{
    public static class ItemCheats
    {
        public static readonly string[] RoTLegendaryWeapons = new string[]
        {
            "rot_rhllors_light",
            "longclaw_sword",
            "ice_sword",
            "oathkeeper_sword",
            "darksister",
            "blackfyre",
            "heartsbane",
            "widows_wail",
            "dawn",
            "lightbringer",
            "ww2_sword",
            "needle",
            "viper_spear",
            "unsullied_spear",
            "baratheon_hammer",
            "mountain_sword",
            "nightfall",
            "celtigar_axe",
            "weirwood_bow",
            "vigilance_sword",
            "lady_forlorn2",
            "red_rain",
            "barristan_sword",
            "brightroar",
            "bolton_sword",
            "renly_sword",
            "lamentation",
            "truth",
            "euron_axe"
        };

        public static readonly string[] RoTArmorItems = new string[]
        {
            "targ_armor",
            "targ_bracers",
            "targ_pauldrons",
            "targ_gorget",
            "targ_helmet",
            "targ_boots",
            "targ_shield",
            "targaryen_shield3",
            "blackfyre_plate",
            "blackfyre_helmet",
            "blackfyre_pauldrons",
            "blackfyre_gauntlets",
            "blackfyre_boots",
            "rhaegar_plate",
            "rhaegar_plate2",
            "rhaegar_pauldrons",
            "rhaegar_gauntlets",
            "rhaegar_boots",
            "armored_dragon_saddle"
        };

        public static readonly string[] SmithingMaterials = new string[]
        {
            "iron_ingot_1", // Crude Iron
            "iron_ingot_2", // Wrought Iron
            "iron_ingot_3", // Iron
            "iron_ingot_4", // Steel
            "iron_ingot_5", // Fine Steel
            "iron_ingot_6", // Thamaskene Steel
            "hardwood",
            "charcoal"
        };

        public static readonly string[] FoodItems = new string[]
        {
            "grain",
            "fish",
            "cheese",
            "butter",
            "meat",
            "grapes",
            "olives",
            "beer",
            "wine",
            "date_fruit"
        };

        public static void OpenCheatInventory()
        {
            if (Campaign.Current == null || MobileParty.MainParty == null) return;

            try
            {
                ItemRoster cheatRoster = new ItemRoster();
                var allItems = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
                foreach (ItemObject item in allItems)
                {
                    if (item != null)
                    {
                        cheatRoster.AddToCounts(item, 100);
                    }
                }

                InventoryScreenHelper.OpenScreenAsReceiveItems(cheatRoster, new TextObject("Realm of Thrones - Item Spawner"), null);
            }
            catch (Exception ex)
            {
                CheatNotify.Show("[RoT Cheats] Could not open item spawner: " + ex.Message);
            }
        }

        public static void GiveAllLegendaryWeapons()
        {
            if (MobileParty.MainParty == null) return;

            int count = 0;
            foreach (string weaponId in RoTLegendaryWeapons)
            {
                ItemObject item = Game.Current.ObjectManager.GetObject<ItemObject>(weaponId);
                if (item != null)
                {
                    MobileParty.MainParty.ItemRoster.AddToCounts(item, 1);
                    count++;
                }
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Added {0} legendary Valyrian & unique weapons to inventory!", count));
        }

        public static void GiveAllArmors()
        {
            if (MobileParty.MainParty == null) return;

            int count = 0;
            foreach (string armorId in RoTArmorItems)
            {
                ItemObject item = Game.Current.ObjectManager.GetObject<ItemObject>(armorId);
                if (item != null)
                {
                    MobileParty.MainParty.ItemRoster.AddToCounts(item, 1);
                    count++;
                }
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Added {0} Valyrian & Targaryen armor pieces to inventory!", count));
        }

        public static void GiveSmithingMaterials(int amount = 999)
        {
            if (MobileParty.MainParty == null) return;

            foreach (string matId in SmithingMaterials)
            {
                ItemObject item = Game.Current.ObjectManager.GetObject<ItemObject>(matId);
                if (item != null)
                {
                    MobileParty.MainParty.ItemRoster.AddToCounts(item, amount);
                }
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Added {0}x of all smithing materials to inventory!", amount));
        }

        public static void GiveFoodProvisions(int amount = 100)
        {
            if (MobileParty.MainParty == null) return;

            foreach (string foodId in FoodItems)
            {
                ItemObject item = Game.Current.ObjectManager.GetObject<ItemObject>(foodId);
                if (item != null)
                {
                    MobileParty.MainParty.ItemRoster.AddToCounts(item, amount);
                }
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Added {0}x of all food types to inventory!", amount));
        }

        public static void GiveWealthAndStats(int gold = 1000000, float renown = 10000f, float influence = 10000f)
        {
            if (Hero.MainHero == null || Clan.PlayerClan == null) return;

            Hero.MainHero.ChangeHeroGold(gold);
            Clan.PlayerClan.AddRenown(renown, true);
            ChangeClanInfluenceAction.Apply(Clan.PlayerClan, influence);

            CheatNotify.Show(string.Format("[RoT Cheats] Granted +{0:N0} Gold, +{1:N0} Renown, and +{2:N0} Influence!", gold, renown, influence));
        }

        public static void UnlockAllCraftingPieces()
        {
            if (Campaign.Current == null) return;

            CraftingCampaignBehavior craftingBehavior = Campaign.Current.GetCampaignBehavior<CraftingCampaignBehavior>();
            if (craftingBehavior == null)
            {
                CheatNotify.Show("[RoT Cheats] Crafting system behavior not found.");
                return;
            }

            MethodInfo openPartMethod = typeof(CraftingCampaignBehavior).GetMethod("OpenPart", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (openPartMethod == null)
            {
                CheatNotify.Show("[RoT Cheats] OpenPart method not accessible.");
                return;
            }

            int unlockedCount = 0;
            object[] invokeParams = new object[3];
            invokeParams[2] = false;

            foreach (CraftingTemplate template in CraftingTemplate.All)
            {
                if (template == null) continue;
                invokeParams[1] = template;

                foreach (CraftingPiece piece in CraftingPiece.All)
                {
                    if (piece != null && !craftingBehavior.IsOpened(piece, template))
                    {
                        invokeParams[0] = piece;
                        openPartMethod.Invoke(craftingBehavior, invokeParams);
                        unlockedCount++;
                    }
                }
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Unlocked all {0} weapon crafting pieces and templates!", unlockedCount));
        }

        public const string RhllorLightId = "rot_rhllors_light";

        public static ItemObject FindRhllorItem()
        {
            if (Game.Current == null || Game.Current.ObjectManager == null) return null;
            return Game.Current.ObjectManager.GetObject<ItemObject>(RhllorLightId)
                ?? Game.Current.ObjectManager.GetObject<ItemObject>("lightbringer");
        }

        public static bool GiveRhllorsLight(bool autoEquip = true)
        {
            if (MobileParty.MainParty == null || Hero.MainHero == null) return false;

            ItemObject item = FindRhllorItem();
            if (item == null)
            {
                CheatNotify.Show("[RoT Cheats] R'hllor's Light item not found in game objects.");
                return false;
            }

            MobileParty.MainParty.ItemRoster.AddToCounts(item, 1);

            if (autoEquip)
            {
                Hero.MainHero.BattleEquipment[EquipmentIndex.WeaponItemBeginSlot] = new EquipmentElement(item);
            }

            string itemName = item.Name != null ? item.Name.ToString() : "R'hllor's Light";
            CheatNotify.Show(string.Format("[RoT Cheats] {0} added to party inventory{1}!",
                itemName, autoEquip ? " and equipped as primary weapon" : ""));
            return true;
        }
    }
}
