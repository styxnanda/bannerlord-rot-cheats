using System;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.Cheats
{
    public static class CombatCheats
    {
        public static void HealPlayerAndMount()
        {
            if (Agent.Main != null)
            {
                Agent.Main.Health = Agent.Main.HealthLimit;
                if (Agent.Main.MountAgent != null)
                {
                    Agent.Main.MountAgent.Health = Agent.Main.MountAgent.HealthLimit;
                }
                CheatNotify.Show("[RoT Cheats] Player & Mount/Dragon restored to 100% Health!");
            }
            else if (MobileParty.MainParty != null && MobileParty.MainParty.LeaderHero != null)
            {
                MobileParty.MainParty.LeaderHero.HitPoints = MobileParty.MainParty.LeaderHero.MaxHitPoints;
                CheatNotify.Show("[RoT Cheats] Main Hero restored to 100% Health!");
            }
        }

        public static void HealParty()
        {
            if (Mission.Current != null && Agent.Main != null)
            {
                int count = 0;
                foreach (Agent agent in Mission.Current.Agents)
                {
                    if (agent != null && agent.IsActive() && agent.Team != null && agent.Team.IsPlayerTeam)
                    {
                        agent.Health = agent.HealthLimit;
                        count++;
                    }
                }
                CheatNotify.Show(string.Format("[RoT Cheats] Healed {0} allied battle units to full health!", count));
            }

            if (MobileParty.MainParty != null)
            {
                for (int i = 0; i < MobileParty.MainParty.MemberRoster.Count; i++)
                {
                    int wounded = MobileParty.MainParty.MemberRoster.GetElementWoundedNumber(i);
                    if (wounded > 0)
                    {
                        CharacterObject troop = MobileParty.MainParty.MemberRoster.GetCharacterAtIndex(i);
                        MobileParty.MainParty.MemberRoster.AddToCounts(troop, 0, false, -wounded, 0, true, -1);
                    }
                }
                CheatNotify.Show("[RoT Cheats] All wounded party troops restored to active duty!");
            }
        }

        public static void WoundAllEnemies()
        {
            BattleSkills.CastDeathlyHallows();
        }
    }
}
