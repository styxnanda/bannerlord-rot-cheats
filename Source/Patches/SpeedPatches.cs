using System;
using HarmonyLib;
using RoTCheats.Config;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.Patches
{
    [HarmonyPatch(typeof(AgentDrivenProperties), "UpdateDrivenProperties")]
    public static class AgentDrivenPropertiesSpeedPatch
    {
        public static void Postfix(AgentDrivenProperties __instance, Agent agent)
        {
            if (__instance == null || agent == null) return;

            CheatSettings settings = CheatSettings.Instance;
            float playerSpeed = settings.PlayerSpeedMultiplier;
            float squadSpeed = settings.SquadSpeedMultiplier;
            float horseSpeed = settings.HorseSpeedMultiplier;

            // Player character on foot or as rider
            if (agent.IsMainAgent)
            {
                if (playerSpeed > 1.0f)
                {
                    __instance.MaxSpeedMultiplier *= playerSpeed;
                    __instance.CombatMaxSpeedMultiplier *= playerSpeed;
                    __instance.CrouchedSpeedMultiplier *= playerSpeed;
                }
                if (horseSpeed > 1.0f)
                {
                    __instance.MountSpeed *= horseSpeed;
                    __instance.MountManeuver *= horseSpeed;
                    __instance.MountDashAccelerationMultiplier *= horseSpeed;
                }
                if (settings.UnlimitedWeight)
                {
                    __instance.ArmorEncumbrance = 0f;
                    __instance.WeaponsEncumbrance = 0f;
                }
            }
            // Mount / Horse / Dragon agent
            else if (agent.IsMount)
            {
                Agent rider = agent.RiderAgent;
                bool riderIsPlayerOrSquad = (rider != null && (rider.IsMainAgent || (rider.Team != null && rider.Team.IsPlayerTeam)));

                if (riderIsPlayerOrSquad && horseSpeed > 1.0f)
                {
                    __instance.MountSpeed *= horseSpeed;
                    __instance.MountManeuver *= horseSpeed;
                    __instance.MountDashAccelerationMultiplier *= horseSpeed;
                    __instance.MaxSpeedMultiplier *= horseSpeed;
                }
            }
            // Allied squad soldiers on foot
            else if (agent.Team != null && agent.Team.IsPlayerTeam)
            {
                if (squadSpeed > 1.0f)
                {
                    __instance.MaxSpeedMultiplier *= squadSpeed;
                    __instance.CombatMaxSpeedMultiplier *= squadSpeed;
                    __instance.CrouchedSpeedMultiplier *= squadSpeed;
                }
                if (horseSpeed > 1.0f)
                {
                    __instance.MountSpeed *= horseSpeed;
                    __instance.MountManeuver *= horseSpeed;
                    __instance.MountDashAccelerationMultiplier *= horseSpeed;
                }
                if (settings.UnlimitedWeight)
                {
                    __instance.ArmorEncumbrance = 0f;
                    __instance.WeaponsEncumbrance = 0f;
                }
            }
        }
    }
}
