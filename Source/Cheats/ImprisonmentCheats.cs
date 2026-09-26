using System;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;

namespace RoTCheats.Cheats
{
    public static class ImprisonmentCheats
    {
        public static bool IsPlayerImprisoned()
        {
            try
            {
                return PlayerCaptivity.IsCaptive || (Hero.MainHero != null && Hero.MainHero.IsPrisoner);
            }
            catch
            {
                return false;
            }
        }

        public static void EscapeMysteriously()
        {
            try
            {
                if (PlayerCaptivity.IsCaptive)
                {
                    PlayerCaptivity.EndCaptivity();
                    CheatNotify.Show("[RoT Cheats] ✨ You vanish like a shadow into the dark and walk free!");
                    return;
                }

                if (Hero.MainHero != null && Hero.MainHero.IsPrisoner)
                {
                    EndCaptivityAction.ApplyByEscape(Hero.MainHero, null, true);
                    CheatNotify.Show("[RoT Cheats] ✨ You slip past the guards unseen and break out of captivity!");
                    return;
                }

                CheatNotify.Show("[RoT Cheats] You are not currently held prisoner.");
            }
            catch (Exception ex)
            {
                CheatNotify.Show("[RoT Cheats] Mysterious escape attempted: " + ex.Message);
            }
        }
    }
}
