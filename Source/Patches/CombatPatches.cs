using System;
using HarmonyLib;
using RoTCheats.Config;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.Patches
{
    public static class CombatPatchesHelper
    {
        public static bool IsAttackerPlayerOrSquad(Agent attacker)
        {
            if (attacker == null) return false;
            return attacker.IsMainAgent ||
                   (attacker.RiderAgent != null && attacker.RiderAgent.IsMainAgent) ||
                   (attacker.Team != null && attacker.Team.IsPlayerTeam);
        }

        public static bool IsVictimEnemy(Agent victim, Agent attacker)
        {
            if (victim == null) return false;
            if (victim.IsMainAgent) return false;
            if (victim.Team != null && victim.Team.IsPlayerTeam) return false;
            return Agent.Main != null ? victim.IsEnemyOf(Agent.Main) : (attacker != null && victim.IsEnemyOf(attacker));
        }
    }

    [HarmonyPatch(typeof(Agent), "RegisterBlow")]
    public static class AgentRegisterBlowPatch
    {
        public static void Prefix(Agent __instance, ref Blow blow, ref AttackCollisionData collisionData)
        {
            if (__instance == null) return;

            CheatSettings settings = CheatSettings.Instance;

            // Check if victim is Main Hero or Main Hero's Mount/Dragon
            bool isPlayer = __instance.IsMainAgent || (__instance.RiderAgent != null && __instance.RiderAgent.IsMainAgent);

            if (settings.GodMode && isPlayer)
            {
                blow.InflictedDamage = 0;
                blow.DamagedPercentage = 0f;
                blow.SelfInflictedDamage = 0;
                __instance.Health = __instance.HealthLimit;
                if (__instance.MountAgent != null && __instance.MountAgent.IsActive())
                {
                    __instance.MountAgent.Health = __instance.MountAgent.HealthLimit;
                }
                return;
            }

            // Check if victim is player's team/squad
            if (settings.PartyGodMode && __instance.Team != null && __instance.Team.IsPlayerTeam)
            {
                blow.InflictedDamage = 0;
                blow.DamagedPercentage = 0f;
                blow.SelfInflictedDamage = 0;
                return;
            }

            // One-Hit Kill: Applies to Player and Squad against enemies ONLY
            if (settings.OneHitKill && Mission.Current != null)
            {
                Agent attacker = Mission.Current.FindAgentWithIndex(blow.OwnerId);
                if (attacker != null && CombatPatchesHelper.IsAttackerPlayerOrSquad(attacker) && CombatPatchesHelper.IsVictimEnemy(__instance, attacker))
                {
                    blow.InflictedDamage = 99999;
                    blow.DamagedPercentage = 1f;
                    blow.BlowFlag |= BlowFlags.CrushThrough;
                    blow.BlowFlag |= BlowFlags.KnockDown;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MissionCombatMechanicsHelper), "GetDefendCollisionResults")]
    public static class MissionCombatMechanicsHelperGetDefendCollisionResultsPatch
    {
        public static void Postfix(Agent attackerAgent, Agent defenderAgent, ref float defenderStunPeriod, ref float attackerStunPeriod, ref bool crushedThrough)
        {
            if (CheatSettings.Instance.OneHitKill && attackerAgent != null && defenderAgent != null)
            {
                if (CombatPatchesHelper.IsAttackerPlayerOrSquad(attackerAgent) && CombatPatchesHelper.IsVictimEnemy(defenderAgent, attackerAgent))
                {
                    crushedThrough = true;
                    attackerStunPeriod = 0f;
                    defenderStunPeriod = 2.0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Mission), "GetDefendCollisionResults")]
    public static class MissionGetDefendCollisionResultsPatch
    {
        public static void Postfix(Agent attackerAgent, Agent defenderAgent, ref float defenderStunPeriod, ref float attackerStunPeriod, ref bool crushedThrough)
        {
            if (CheatSettings.Instance.OneHitKill && attackerAgent != null && defenderAgent != null)
            {
                if (CombatPatchesHelper.IsAttackerPlayerOrSquad(attackerAgent) && CombatPatchesHelper.IsVictimEnemy(defenderAgent, attackerAgent))
                {
                    crushedThrough = true;
                    attackerStunPeriod = 0f;
                    defenderStunPeriod = 2.0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Mission), "RegisterBlow")]
    public static class MissionRegisterBlowPatch
    {
        public static void Prefix(Agent attacker, Agent victim, ref Blow b)
        {
            if (victim == null) return;

            CheatSettings settings = CheatSettings.Instance;

            // God Mode check for victim
            bool isPlayer = victim.IsMainAgent || (victim.RiderAgent != null && victim.RiderAgent.IsMainAgent);
            if (settings.GodMode && isPlayer)
            {
                b.InflictedDamage = 0;
                b.DamagedPercentage = 0f;
                b.SelfInflictedDamage = 0;
                victim.Health = victim.HealthLimit;
                if (victim.MountAgent != null && victim.MountAgent.IsActive())
                {
                    victim.MountAgent.Health = victim.MountAgent.HealthLimit;
                }
                return;
            }

            // Party God Mode check for victim
            if (settings.PartyGodMode && victim.Team != null && victim.Team.IsPlayerTeam)
            {
                b.InflictedDamage = 0;
                b.DamagedPercentage = 0f;
                b.SelfInflictedDamage = 0;
                return;
            }

            // One-Hit Kill: attacker is Player/Squad and victim is enemy
            if (settings.OneHitKill && attacker != null && CombatPatchesHelper.IsAttackerPlayerOrSquad(attacker) && CombatPatchesHelper.IsVictimEnemy(victim, attacker))
            {
                b.InflictedDamage = 99999;
                b.DamagedPercentage = 1f;
                b.BlowFlag |= BlowFlags.CrushThrough;
                b.BlowFlag |= BlowFlags.KnockDown;
            }
        }
    }
}
