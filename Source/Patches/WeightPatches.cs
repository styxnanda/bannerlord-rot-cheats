using System;
using HarmonyLib;
using RoTCheats.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace RoTCheats.Patches
{
    public static class WeightPatchesHelper
    {
        public static bool IsPlayerParty(MobileParty party)
        {
            if (party == null)
            {
                return MobileParty.MainParty != null;
            }
            return party.IsMainParty ||
                   party == MobileParty.MainParty ||
                   party.LeaderHero == Hero.MainHero ||
                   (party.ActualClan != null && Clan.PlayerClan != null && party.ActualClan == Clan.PlayerClan);
        }
    }

    [HarmonyPatch(typeof(MobileParty), "InventoryCapacity", MethodType.Getter)]
    public static class MobilePartyInventoryCapacityPatch
    {
        public static void Postfix(MobileParty __instance, ref int __result)
        {
            if (__instance == null) return;
            if (CheatSettings.Instance.UnlimitedWeight && WeightPatchesHelper.IsPlayerParty(__instance))
            {
                __result = 10000000;
            }
        }
    }

    [HarmonyPatch(typeof(MobileParty), "TotalWeightCarried", MethodType.Getter)]
    public static class MobilePartyTotalWeightCarriedPatch
    {
        public static void Postfix(MobileParty __instance, ref float __result)
        {
            if (__instance == null) return;
            if (CheatSettings.Instance.UnlimitedWeight && WeightPatchesHelper.IsPlayerParty(__instance))
            {
                __result = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(DefaultInventoryCapacityModel), "CalculateInventoryCapacity")]
    public static class DefaultInventoryCapacityModelPatch
    {
        public static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
        {
            if (CheatSettings.Instance.UnlimitedWeight && WeightPatchesHelper.IsPlayerParty(mobileParty))
            {
                __result.Add(10000000f, new TextObject("{=rot_cheats_unlimited_weight}Unlimited Weight (RoT Cheats)", null));
            }
        }
    }

    [HarmonyPatch(typeof(DefaultInventoryCapacityModel), "CalculateTotalWeightCarried")]
    public static class DefaultInventoryCalculateTotalWeightPatch
    {
        public static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
        {
            if (CheatSettings.Instance.UnlimitedWeight && WeightPatchesHelper.IsPlayerParty(mobileParty))
            {
                __result = new ExplainedNumber(0f);
            }
        }
    }

    [HarmonyPatch(typeof(DefaultPartySpeedCalculatingModel), "GetOverburdenedEffect")]
    public static class DefaultPartySpeedOverburdenedPatch
    {
        public static bool Prefix(MobileParty party, ref ExplainedNumber __result)
        {
            if (CheatSettings.Instance.UnlimitedWeight && WeightPatchesHelper.IsPlayerParty(party))
            {
                __result = new ExplainedNumber(0f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DefaultPartySpeedCalculatingModel), "GetCargoEffect")]
    public static class DefaultPartySpeedCargoEffectPatch
    {
        public static bool Prefix(float weightCarried, int partyCapacity, ref float __result)
        {
            if (CheatSettings.Instance.UnlimitedWeight)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }
}
