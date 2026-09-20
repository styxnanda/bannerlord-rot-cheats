using System;
using HarmonyLib;
using RoTCheats.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;

namespace RoTCheats.Patches
{
    [HarmonyPatch(typeof(DefaultSmithingModel), "GetEnergyCostForRefining")]
    public static class EnergyCostForRefiningPatch
    {
        public static void Postfix(ref int __result)
        {
            if (CheatSettings.Instance.InfiniteStamina)
            {
                __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(DefaultSmithingModel), "GetEnergyCostForSmithing")]
    public static class EnergyCostForSmithingPatch
    {
        public static void Postfix(ref int __result)
        {
            if (CheatSettings.Instance.InfiniteStamina)
            {
                __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(DefaultSmithingModel), "GetEnergyCostForSmelting")]
    public static class EnergyCostForSmeltingPatch
    {
        public static void Postfix(ref int __result)
        {
            if (CheatSettings.Instance.InfiniteStamina)
            {
                __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(CraftingCampaignBehavior), "GetHeroCraftingStamina")]
    public static class HeroCraftingStaminaPatch
    {
        public static void Postfix(CraftingCampaignBehavior __instance, Hero hero, ref int __result)
        {
            if (CheatSettings.Instance.InfiniteStamina && hero != null)
            {
                __result = __instance.GetMaxHeroCraftingStamina(hero);
            }
        }
    }
}
