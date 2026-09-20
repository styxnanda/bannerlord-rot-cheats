using System;
using System.Collections.Generic;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace RoTCheats.Cheats
{
    public class DragonInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsFlying { get; set; }

        public DragonInfo(string id, string name, bool isFlying)
        {
            Id = id;
            Name = name;
            IsFlying = isFlying;
        }
    }

    public static class DragonSpawner
    {
        public static readonly List<DragonInfo> Dragons = new List<DragonInfo>()
        {
            new DragonInfo("dragon_black", "Drogon (Ground)", false),
            new DragonInfo("dragon_brown", "Rhaegal (Ground)", false),
            new DragonInfo("dragon_green", "Viserion (Ground)", false),
            new DragonInfo("dragon_red", "Draeghar (Ground)", false),
            new DragonInfo("dragon_gold", "Gold Dragon (Ground)", false),
            new DragonInfo("dragon_smoke", "Sea Smoke (Ground)", false),

            new DragonInfo("dragon_black2", "Drogon (Flying)", true),
            new DragonInfo("dragon_brown2", "Rhaegal (Flying)", true),
            new DragonInfo("dragon_green2", "Viserion (Flying)", true),
            new DragonInfo("dragon_red2", "Draeghar (Flying)", true),
            new DragonInfo("dragon_gold2", "Gold Dragon (Flying)", true),
            new DragonInfo("dragon_smoke2", "Sea Smoke (Flying)", true)
        };

        public const string SaddleId = "armored_dragon_saddle";

        public static ItemObject FindItem(string stringId)
        {
            if (Game.Current == null || Game.Current.ObjectManager == null) return null;
            return Game.Current.ObjectManager.GetObject<ItemObject>(stringId);
        }

        public static bool SpawnDragon(string dragonId, bool autoEquip)
        {
            if (MobileParty.MainParty == null || Hero.MainHero == null) return false;

            ItemObject dragon = FindItem(dragonId);
            if (dragon == null)
            {
                CheatNotify.Show(string.Format("[RoT Cheats] Dragon '{0}' not found in game objects.", dragonId));
                return false;
            }

            // Add to Party Inventory
            MobileParty.MainParty.ItemRoster.AddToCounts(dragon, 1);

            // Add Armored Dragon Saddle
            ItemObject saddle = FindItem(SaddleId);
            if (saddle != null)
            {
                MobileParty.MainParty.ItemRoster.AddToCounts(saddle, 1);
            }

            // Auto-equip if requested
            if (autoEquip)
            {
                Hero.MainHero.BattleEquipment[EquipmentIndex.Horse] = new EquipmentElement(dragon);
                if (saddle != null)
                {
                    Hero.MainHero.BattleEquipment[EquipmentIndex.HorseHarness] = new EquipmentElement(saddle);
                }
            }

            string msg = string.Format("[RoT Cheats] Bound with {0}! Added to party{1}.",
                dragon.Name != null ? dragon.Name.ToString() : dragonId,
                autoEquip ? " and equipped as battle mount" : "");

            CheatNotify.Show(msg);
            return true;
        }

        public static void SpawnAllDragons()
        {
            if (MobileParty.MainParty == null) return;

            int count = 0;
            foreach (DragonInfo info in Dragons)
            {
                ItemObject item = FindItem(info.Id);
                if (item != null)
                {
                    MobileParty.MainParty.ItemRoster.AddToCounts(item, 1);
                    count++;
                }
            }

            ItemObject saddle = FindItem(SaddleId);
            if (saddle != null)
            {
                MobileParty.MainParty.ItemRoster.AddToCounts(saddle, 5);
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Spawned {0} dragons and saddles into party inventory!", count));
        }
    }
}
