using System;
using RoTCheats.Cheats;
using RoTCheats.Config;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.MissionBehaviors
{
    public class ROTCheatsMissionBehavior : MissionBehavior
    {
        private float _lastPlayerSpeed = -1f;
        private float _lastSquadSpeed = -1f;
        private float _lastHorseSpeed = -1f;
        private float _periodicTimer = 0f;
        private float _skillCooldownTimer = 0f;

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

            // Continuous Battle Skills Ticking (R'hllor's Light radial reticle/missiles & Cannibal's Wisp hand spray)
            BattleSkills.TickRhllorsLight(dt);
            BattleSkills.TickCannibalWisp(dt);

            // Battle Skills Hotkeys Check
            if (settings.EnableBattleSkillHotkeys)
            {
                _skillCooldownTimer -= dt;
                if (_skillCooldownTimer <= 0f)
                {
                    bool num1 = settings.EnableNumpadSkillKeys && Input.IsKeyPressed(InputKey.Numpad1);
                    bool letU = settings.EnableLetterSkillKeys && Input.IsKeyPressed(InputKey.U);

                    bool num2 = settings.EnableNumpadSkillKeys && Input.IsKeyPressed(InputKey.Numpad2);
                    bool letI = settings.EnableLetterSkillKeys && Input.IsKeyPressed(InputKey.I);

                    bool num3 = settings.EnableNumpadSkillKeys && Input.IsKeyPressed(InputKey.Numpad3);
                    bool letO = settings.EnableLetterSkillKeys && Input.IsKeyPressed(InputKey.O);

                    bool num4 = settings.EnableNumpadSkillKeys && Input.IsKeyPressed(InputKey.Numpad4);
                    bool letP = settings.EnableLetterSkillKeys && Input.IsKeyPressed(InputKey.P);

                    bool menuK = Input.IsKeyPressed(InputKey.K);

                    if (num1 || letU)
                    {
                        _skillCooldownTimer = 0.35f;
                        BattleSkills.CastSweepingForce();
                    }
                    else if (num2 || letI)
                    {
                        _skillCooldownTimer = 0.35f;
                        BattleSkills.LaunchRhllorMissiles();
                    }
                    else if (num3 || letO)
                    {
                        _skillCooldownTimer = 0.35f;
                        BattleSkills.ToggleCannibalWisp();
                    }
                    else if (num4 || letP)
                    {
                        _skillCooldownTimer = 0.35f;
                        BattleSkills.CastDeathlyHallows();
                    }
                    else if (menuK)
                    {
                        _skillCooldownTimer = 0.35f;
                        BattleSkills.OpenSkillCastMenu();
                    }
                }
            }

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

        protected override void OnEndMission()
        {
            base.OnEndMission();
            BattleSkills.ResetState();
        }
    }
}
