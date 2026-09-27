using System;
using HarmonyLib;
using RoTCheats.Cheats;
using RoTCheats.Config;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace RoTCheats.Patches
{
    [HarmonyPatch(typeof(MapScreen), "HandleLeftMouseButtonClick")]
    public static class MapScreenHandleLeftClickPatch
    {
        public static void Postfix(CampaignVec2 intersectionPoint)
        {
            try
            {
                NavigationEnhancerManager.OnMapLeftClicked(null, intersectionPoint);
            }
            catch
            {
            }
        }
    }

    [HarmonyPatch(typeof(DefaultPartySpeedCalculatingModel), "CalculateBaseSpeed")]
    public static class DefaultPartySpeedCalculateBaseSpeedPatch
    {
        public static void Postfix(ref ExplainedNumber __result, MobileParty mobileParty)
        {
            if (mobileParty == null || !mobileParty.IsMainParty) return;

            float bonus = CheatSettings.Instance.PartyBaseSpeedBonus;
            if (Math.Abs(bonus) > 0.01f)
            {
                __result.Add(bonus, new TextObject("{=rot_nav_enhancer_speed}Navigation Enhancer"));
            }
        }
    }
}
