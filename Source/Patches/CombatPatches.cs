using System;
using System.Reflection;
using HarmonyLib;
using RoTCheats.Config;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.Patches
{
    public static class CombatPatchesHelper
    {
        private static readonly FieldInfo AttackBlockedWithShieldField =
            typeof(AttackCollisionData).GetField("_attackBlockedWithShield", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo CollisionResultField =
            typeof(AttackCollisionData).GetField("_collisionResult", BindingFlags.Instance | BindingFlags.NonPublic);

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

        public static void SetShieldBlocked(ref AttackCollisionData acd, bool blocked)
        {
            if (AttackBlockedWithShieldField != null)
            {
                object boxed = acd;
                AttackBlockedWithShieldField.SetValue(boxed, blocked);
                acd = (AttackCollisionData)boxed;
            }
        }

        public static void SetCollisionResult(ref AttackCollisionData acd, CombatCollisionResult result)
        {
            if (CollisionResultField != null)
            {
                object boxed = acd;
                CollisionResultField.SetValue(boxed, (int)result);
                acd = (AttackCollisionData)boxed;
            }
        }
    }

    [HarmonyPatch(typeof(Agent), "RegisterBlow")]
    public static class AgentRegisterBlowPatch
    {
        public static bool Prefix(Agent __instance, ref Blow blow, ref AttackCollisionData collisionData)
        {
            if (__instance == null) return true;

            CheatSettings settings = CheatSettings.Instance;

            // Check if victim is Main Hero or Main Hero's Mount/Dragon
            bool isPlayer = (__instance.IsMainAgent || (__instance.RiderAgent != null && __instance.RiderAgent.IsMainAgent));

            if (settings.GodMode && isPlayer)
            {
                blow.InflictedDamage = 0;
                blow.DamagedPercentage = 0f;
                __instance.Health = __instance.HealthLimit;
                return false;
            }

            // Check if victim is player's team/squad
            if (settings.PartyGodMode && __instance.Team != null && __instance.Team.IsPlayerTeam)
            {
                blow.InflictedDamage = 0;
                blow.DamagedPercentage = 0f;
                return false;
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

            return true;
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

    [HarmonyPatch(typeof(Mission), "MeleeHitCallback")]
    public static class MissionMeleeHitCallbackPatch
    {
        public static void Prefix(ref AttackCollisionData collisionData, Agent attacker, Agent victim, ref float inOutMomentumRemaining, ref MeleeCollisionReaction colReaction, ref CrushThroughState crushThroughState)
        {
            if (CheatSettings.Instance.OneHitKill && attacker != null && victim != null)
            {
                if (CombatPatchesHelper.IsAttackerPlayerOrSquad(attacker) && CombatPatchesHelper.IsVictimEnemy(victim, attacker))
                {
                    if (collisionData.AttackBlockedWithShield)
                    {
                        try
                        {
                            EquipmentIndex offhand = victim.GetOffhandWieldedItemIndex();
                            if (offhand != EquipmentIndex.None && victim.Equipment != null && !victim.Equipment[offhand].IsEmpty)
                            {
                                victim.ChangeWeaponHitPoints(offhand, 0);
                                victim.RemoveEquippedWeapon(offhand);
                            }
                        }
                        catch
                        {
                        }

                        collisionData.IsShieldBroken = true;
                        CombatPatchesHelper.SetShieldBlocked(ref collisionData, false);
                    }

                    CombatCollisionResult res = collisionData.CollisionResult;
                    if (res == CombatCollisionResult.Blocked || res == CombatCollisionResult.Parried || res == CombatCollisionResult.ChamberBlocked)
                    {
                        CombatPatchesHelper.SetCollisionResult(ref collisionData, CombatCollisionResult.StrikeAgent);
                    }

                    colReaction = MeleeCollisionReaction.SlicedThrough;
                    inOutMomentumRemaining = 1f;
                    crushThroughState = CrushThroughState.CrushedThisFrame;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Mission), "MissileHitCallback")]
    public static class MissionMissileHitCallbackPatch
    {
        public static void Prefix(ref AttackCollisionData collisionData, Agent attacker, Agent victim)
        {
            if (CheatSettings.Instance.OneHitKill && attacker != null && victim != null)
            {
                if (CombatPatchesHelper.IsAttackerPlayerOrSquad(attacker) && CombatPatchesHelper.IsVictimEnemy(victim, attacker))
                {
                    if (collisionData.AttackBlockedWithShield)
                    {
                        try
                        {
                            EquipmentIndex offhand = victim.GetOffhandWieldedItemIndex();
                            if (offhand != EquipmentIndex.None && victim.Equipment != null && !victim.Equipment[offhand].IsEmpty)
                            {
                                victim.ChangeWeaponHitPoints(offhand, 0);
                                victim.RemoveEquippedWeapon(offhand);
                            }
                        }
                        catch
                        {
                        }

                        collisionData.IsShieldBroken = true;
                        CombatPatchesHelper.SetShieldBlocked(ref collisionData, false);
                        CombatPatchesHelper.SetCollisionResult(ref collisionData, CombatCollisionResult.StrikeAgent);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Mission), "RegisterBlow")]
    public static class MissionRegisterBlowPatch
    {
        public static void Prefix(Agent attacker, Agent victim, ref Blow b, ref AttackCollisionData collisionData)
        {
            if (CheatSettings.Instance.OneHitKill && attacker != null && victim != null)
            {
                if (CombatPatchesHelper.IsAttackerPlayerOrSquad(attacker) && CombatPatchesHelper.IsVictimEnemy(victim, attacker))
                {
                    if (collisionData.AttackBlockedWithShield)
                    {
                        collisionData.IsShieldBroken = true;
                        CombatPatchesHelper.SetShieldBlocked(ref collisionData, false);
                    }

                    CombatPatchesHelper.SetCollisionResult(ref collisionData, CombatCollisionResult.StrikeAgent);
                    collisionData.InflictedDamage = 99999;
                    b.InflictedDamage = 99999;
                    b.DamagedPercentage = 1f;
                    b.BlowFlag |= BlowFlags.CrushThrough;
                }
            }
        }
    }
}
