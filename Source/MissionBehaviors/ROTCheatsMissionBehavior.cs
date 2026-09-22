using System;
using RoTCheats.Config;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.MissionBehaviors
{
    public class ROTCheatsMissionBehavior : MissionBehavior
    {
        private float _lastPlayerSpeed = -1f;
        private float _lastSquadSpeed = -1f;
        private float _lastHorseSpeed = -1f;
        private float _periodicTimer = 0f;

        public override MissionBehaviorType BehaviorType
        {
            get { return MissionBehaviorType.Other; }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            Agent mainAgent = Agent.Main;
            if (mainAgent == null || !mainAgent.IsActive()) return;

            CheatSettings settings = CheatSettings.Instance;

            // God Mode continuous check
            if (settings.GodMode)
            {
                mainAgent.Health = mainAgent.HealthLimit;
                if (mainAgent.MountAgent != null && mainAgent.MountAgent.IsActive())
                {
                    mainAgent.MountAgent.Health = mainAgent.MountAgent.HealthLimit;
                }
            }

            // Unlimited Ammo
            if (settings.UnlimitedAmmo)
            {
                for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
                {
                    MissionWeapon weapon = mainAgent.Equipment[i];
                    if (!weapon.IsEmpty && weapon.IsAnyConsumable())
                    {
                        short maxAmount = weapon.ModifiedMaxAmount;
                        if (weapon.Amount < maxAmount)
                        {
                            mainAgent.SetWeaponAmountInSlot(i, maxAmount, true);
                        }
                    }
                }
            }

            // Speed Modifier enforcement
            float pSpeed = settings.PlayerSpeedMultiplier;
            float sSpeed = settings.SquadSpeedMultiplier;
            float hSpeed = settings.HorseSpeedMultiplier;

            if (pSpeed > 1.0f)
            {
                mainAgent.SetMaximumSpeedLimit(pSpeed, true);
            }

            if (mainAgent.MountAgent != null && mainAgent.MountAgent.IsActive() && hSpeed > 1.0f)
            {
                mainAgent.MountAgent.SetMaximumSpeedLimit(hSpeed, true);
            }

            // Periodic driven property refresh & squad speed limit update
            _periodicTimer += dt;
            bool speedChanged = Math.Abs(pSpeed - _lastPlayerSpeed) > 0.01f ||
                                Math.Abs(sSpeed - _lastSquadSpeed) > 0.01f ||
                                Math.Abs(hSpeed - _lastHorseSpeed) > 0.01f;

            if (speedChanged || _periodicTimer > 1.5f)
            {
                _periodicTimer = 0f;
                _lastPlayerSpeed = pSpeed;
                _lastSquadSpeed = sSpeed;
                _lastHorseSpeed = hSpeed;

                if (Mission.Current != null && Mission.Current.Agents != null)
                {
                    foreach (Agent agent in Mission.Current.Agents)
                    {
                        if (agent == null || !agent.IsActive()) continue;

                        bool isPlayer = agent.IsMainAgent;
                        bool isPlayerMount = agent.IsMount && agent.RiderAgent != null && agent.RiderAgent.IsMainAgent;
                        bool isSquad = agent.Team != null && agent.Team.IsPlayerTeam && !agent.IsMainAgent;
                        bool isSquadMount = agent.IsMount && agent.RiderAgent != null && agent.RiderAgent.Team != null && agent.RiderAgent.Team.IsPlayerTeam;

                        if (settings.PartyGodMode && (isSquad || isSquadMount))
                        {
                            agent.Health = agent.HealthLimit;
                        }

                        if (isPlayer && pSpeed > 1.0f)
                        {
                            agent.SetMaximumSpeedLimit(pSpeed, true);
                            agent.UpdateAgentProperties();
                        }
                        else if (isPlayerMount && hSpeed > 1.0f)
                        {
                            agent.SetMaximumSpeedLimit(hSpeed, true);
                            agent.UpdateAgentProperties();
                        }
                        else if (isSquad && sSpeed > 1.0f)
                        {
                            agent.SetMaximumSpeedLimit(sSpeed, true);
                            agent.UpdateAgentProperties();
                        }
                        else if (isSquadMount && hSpeed > 1.0f)
                        {
                            agent.SetMaximumSpeedLimit(hSpeed, true);
                            agent.UpdateAgentProperties();
                        }
                    }
                }
            }
        }
    }
}
