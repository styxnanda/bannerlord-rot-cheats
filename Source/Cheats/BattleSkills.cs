using System;
using System.Collections.Generic;
using RoTCheats.UI;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.Cheats
{
    public static class BattleSkills
    {
        #region 1. Sweeping Force (Fus-Ro-Dah)
        /// <summary>
        /// Fus-ro-dah-like skill: Medium-range frontal blast throwing away and killing enemies in front of the player.
        /// </summary>
        public static void CastSweepingForce()
        {
            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive())
            {
                CheatNotify.Show("[RoT Cheats] Sweeping Force can only be unleashed during battle missions.");
                return;
            }

            Agent player = Agent.Main;
            Vec3 playerPos = player.Position;
            Vec3 eyePos = player.GetEyeGlobalPosition();
            Vec3 lookDir = player.LookDirection;
            lookDir.z = 0f;
            if (lookDir.LengthSquared > 0.001f)
            {
                lookDir.Normalize();
            }
            else
            {
                lookDir = new Vec3(0, 1, 0);
            }

            // Visual shockwave bursts extending forward in a cone
            try
            {
                float[] stepDistances = new float[] { 3f, 7f, 13f, 20f, 28f };
                foreach (float dist in stepDistances)
                {
                    Vec3 burstPos = playerPos + (lookDir * dist);
                    float gh = Mission.Current.Scene.GetGroundHeightAtPosition(burstPos, BodyFlags.None);
                    burstPos.z = Math.Max(burstPos.z, gh) + 0.3f;

                    MatrixFrame frame = MatrixFrame.Identity;
                    frame.origin = burstPos;
                    Mission.Current.AddParticleSystemBurstByName("psys_game_boulder_stone_coll", frame, false);
                    Mission.Current.AddParticleSystemBurstByName("psys_game_stone_dust_a", frame, false);
                }
            }
            catch { }

            // Audio shockwave
            try
            {
                SoundEvent.PlaySound2D("event:/map/ambient/node/siege/boulder_hit");
                SoundEvent.PlaySound2D("event:/mission/movement/foley/taunt/hit");
            }
            catch { }

            // Blast and eliminate enemies in medium range frontal cone (~28m, ~75 degrees)
            int count = 0;
            float maxRange = 28f;
            float coneDotThreshold = 0.55f;

            List<Agent> targets = new List<Agent>();
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent != null && agent.IsActive() && agent != player && agent.IsEnemyOf(player))
                {
                    Vec3 toTarget = agent.Position - playerPos;
                    float dist = toTarget.Length;
                    if (dist <= maxRange)
                    {
                        Vec3 toTargetH = toTarget;
                        toTargetH.z = 0f;
                        if (toTargetH.LengthSquared > 0.001f)
                        {
                            toTargetH.Normalize();
                            if (Vec3.DotProduct(lookDir, toTargetH) >= coneDotThreshold)
                            {
                                targets.Add(agent);
                            }
                        }
                    }
                }
            }

            foreach (Agent target in targets)
            {
                if (!target.IsActive()) continue;

                Vec3 diff = target.Position - playerPos;
                float d = Math.Max(1f, diff.Length);
                Vec3 blastDir = diff;
                blastDir.Normalize();
                blastDir.z += 0.45f; // Upward angle to launch into air
                blastDir.Normalize();

                float forceMagnitude = Math.Max(120f, 400f * (1f - (d / (maxRange + 5f))));

                Blow blow = new Blow(player.Index);
                blow.InflictedDamage = 99999;
                blow.DamageType = DamageTypes.Blunt;
                blow.BlowFlag = BlowFlags.KnockBack | BlowFlags.KnockDown | BlowFlags.CrushThrough | BlowFlags.CanDismount;
                blow.BaseMagnitude = forceMagnitude;
                blow.GlobalPosition = target.Position;
                blow.Direction = blastDir;
                blow.SwingDirection = blastDir;

                try
                {
                    target.Die(blow, Agent.KillInfo.Invalid);
                    Vec3 ragdollForce = blastDir * (forceMagnitude * 0.85f);
                    target.ApplyForceOnRagdoll((sbyte)(-1), ref ragdollForce);
                }
                catch { }

                // If target has mount, defeat and launch mount too
                if (target.MountAgent != null && target.MountAgent.IsActive())
                {
                    try
                    {
                        Blow mountBlow = new Blow(player.Index);
                        mountBlow.InflictedDamage = 99999;
                        mountBlow.DamageType = DamageTypes.Blunt;
                        mountBlow.BlowFlag = BlowFlags.KnockBack | BlowFlags.KnockDown;
                        mountBlow.BaseMagnitude = forceMagnitude * 0.7f;
                        mountBlow.GlobalPosition = target.MountAgent.Position;
                        mountBlow.Direction = blastDir;
                        target.MountAgent.Die(mountBlow, Agent.KillInfo.Invalid);
                    }
                    catch { }
                }

                count++;
            }

            CheatNotify.Show(string.Format("[Sweeping Force] Fus-Ro-Dah blasted and hurled {0} enemies backward!", count));
        }
        #endregion

        #region 2. R'hllor's Light (Area Firestorm Barrage)
        /// <summary>
        /// Aimed circular firestorm barrage: Casts a massive fiery vortex at the aimed ground or siege surface, incinerating all enemies within a large circular area.
        /// </summary>
        public static void CastRhllorLight()
        {
            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive())
            {
                CheatNotify.Show("[RoT Cheats] R'hllor's Light can only be unleashed during battle missions.");
                return;
            }

            Agent player = Agent.Main;
            Vec3 eyePos = player.GetEyeGlobalPosition();
            Vec3 aimDir = player.LookDirection;

            // Use camera forward vector if available for exact crosshair aim
            try
            {
                MatrixFrame camFrame = Mission.Current.GetCameraFrame();
                if (camFrame.rotation.f.LengthSquared > 0.001f)
                {
                    aimDir = camFrame.rotation.f;
                }
            }
            catch { }

            aimDir.Normalize();

            // Raycast for closest entity, terrain, castle wall, or battlement
            Vec3 rayEnd = eyePos + (aimDir * 250f);
            float hitDistance = 0f;
            Vec3 targetCenter = Vec3.Zero;
            bool hit = false;
            try
            {
                hit = Mission.Current.Scene.RayCastForClosestEntityOrTerrain(eyePos, rayEnd, out hitDistance, out targetCenter, 0.05f, BodyFlags.None);
            }
            catch
            {
                hit = false;
                targetCenter = eyePos + (aimDir * 35f);
            }

            if (!hit || hitDistance <= 0.2f)
            {
                Vec3 projectedPos = eyePos + (aimDir * 40f);
                float gh = Mission.Current.Scene.GetGroundHeightAtPosition(projectedPos, BodyFlags.None);
                targetCenter = new Vec3(projectedPos.x, projectedPos.y, Math.Max(projectedPos.z, gh));
            }

            // Spawn great barrage of fire particles in swirling concentric rings (radius ~22m)
            float aoeRadius = 22f;
            try
            {
                // Central explosion pillar
                MatrixFrame centerFrame = MatrixFrame.Identity;
                centerFrame.origin = targetCenter;
                Mission.Current.AddParticleSystemBurstByName("psys_game_burning_boulder_coll", centerFrame, false);
                Mission.Current.AddParticleSystemBurstByName("psys_fire_vertical", centerFrame, false);

                // Swirling barrage rings (Rasenshuriken / Firestorm pattern)
                int numBursts = 14;
                for (int i = 0; i < numBursts; i++)
                {
                    float angle = (float)(i * (Math.PI * 2.0 / numBursts));
                    float r = 5f + ((i % 3) * 6.5f); // Distribute bursts at 5m, 11.5m, 18m
                    Vec3 bPos = new Vec3(targetCenter.x + (float)Math.Cos(angle) * r, targetCenter.y + (float)Math.Sin(angle) * r, targetCenter.z);
                    try
                    {
                        float gh = Mission.Current.Scene.GetGroundHeightAtPosition(bPos, BodyFlags.None);
                        bPos.z = Math.Max(bPos.z, gh) + 0.3f;
                    }
                    catch { }

                    MatrixFrame bFrame = MatrixFrame.Identity;
                    bFrame.origin = bPos;
                    Mission.Current.AddParticleSystemBurstByName("psys_game_burning_boulder_coll", bFrame, false);
                    Mission.Current.AddParticleSystemBurstByName("psys_game_burning_jar_trail", bFrame, false);
                    Mission.Current.AddParticleSystemBurstByName("psys_battleground_env_fire", bFrame, false);
                }
            }
            catch { }

            // Audio: Great barrage of fire & catapult explosion
            try
            {
                SoundEvent.PlaySound2D("event:/map/ambient/node/siege/mangonel_fire");
                SoundEvent.PlaySound2D("event:/mission/ambient/detail/fire/fire_big");
            }
            catch { }

            // Incinerate all enemies in circular area
            int count = 0;
            List<Agent> victims = new List<Agent>();
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent != null && agent.IsActive() && agent != player && agent.IsEnemyOf(player))
                {
                    if (agent.Position.Distance(targetCenter) <= aoeRadius)
                    {
                        victims.Add(agent);
                    }
                }
            }

            foreach (Agent victim in victims)
            {
                if (!victim.IsActive()) continue;

                Blow blow = new Blow(player.Index);
                blow.InflictedDamage = 99999;
                blow.DamageType = DamageTypes.Cut;
                blow.BlowFlag = BlowFlags.CrushThrough | BlowFlags.KnockDown;
                blow.GlobalPosition = victim.Position;
                blow.Direction = new Vec3(0, 0, 1);

                try
                {
                    victim.Die(blow, Agent.KillInfo.Invalid);
                    MatrixFrame burnFrame = MatrixFrame.Identity;
                    burnFrame.origin = victim.Position;
                    Mission.Current.AddParticleSystemBurstByName("psys_game_burning_agent", burnFrame, false);
                }
                catch { }

                if (victim.MountAgent != null && victim.MountAgent.IsActive())
                {
                    try
                    {
                        Blow mountBlow = new Blow(player.Index);
                        mountBlow.InflictedDamage = 99999;
                        mountBlow.DamageType = DamageTypes.Cut;
                        mountBlow.GlobalPosition = victim.MountAgent.Position;
                        victim.MountAgent.Die(mountBlow, Agent.KillInfo.Invalid);
                    }
                    catch { }
                }

                count++;
            }

            CheatNotify.Show(string.Format("[R'hllor's Light] The Lord of Light engulfed the area in fire, incinerating {0} enemies!", count));
        }
        #endregion

        #region 3. Cannibal's Wisp (Spray Dragon Fire)
        /// <summary>
        /// Spray fire at aimed direction: Emits a roaring jet of dragon flame along the crosshair/aim direction, scorching all enemies caught in its path.
        /// </summary>
        public static void CastCannibalWisp()
        {
            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive())
            {
                CheatNotify.Show("[RoT Cheats] Cannibal's Wisp can only be unleashed during battle missions.");
                return;
            }

            Agent player = Agent.Main;
            Vec3 eyePos = player.GetEyeGlobalPosition();
            Vec3 playerPos = player.Position;
            Vec3 aimDir = player.LookDirection;

            try
            {
                MatrixFrame camFrame = Mission.Current.GetCameraFrame();
                if (camFrame.rotation.f.LengthSquared > 0.001f)
                {
                    aimDir = camFrame.rotation.f;
                }
            }
            catch { }

            aimDir.Normalize();

            // Spawn torrent of dragon fire bursts along the aimed vector
            float sprayRange = 36f;
            try
            {
                for (float d = 2.5f; d <= sprayRange; d += 2.5f)
                {
                    Vec3 firePoint = eyePos + (aimDir * d);
                    try
                    {
                        float gh = Mission.Current.Scene.GetGroundHeightAtPosition(firePoint, BodyFlags.None);
                        if (firePoint.z < gh + 0.3f)
                        {
                            firePoint.z = gh + 0.3f;
                        }
                    }
                    catch { }

                    MatrixFrame flameFrame = MatrixFrame.Identity;
                    flameFrame.origin = firePoint;
                    Mission.Current.AddParticleSystemBurstByName("psys_game_burning_boulder_coll", flameFrame, false);
                    Mission.Current.AddParticleSystemBurstByName("psys_torch_fire_moving", flameFrame, false);
                    Mission.Current.AddParticleSystemBurstByName("psys_game_missile_flame", flameFrame, false);
                }
            }
            catch { }

            // Roaring flame sound
            try
            {
                SoundEvent.PlaySound2D("event:/mission/ambient/detail/fire/fire_big");
            }
            catch { }

            // Eliminate enemies within the flame spray cone
            int count = 0;
            List<Agent> victims = new List<Agent>();
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent != null && agent.IsActive() && agent != player && agent.IsEnemyOf(player))
                {
                    Vec3 toTarget = agent.Position - eyePos;
                    float dist = toTarget.Length;
                    if (dist <= sprayRange)
                    {
                        Vec3 toTargetNorm = toTarget;
                        toTargetNorm.Normalize();
                        // Cone check: wide enough to catch clusters in the line of fire
                        if (Vec3.DotProduct(aimDir, toTargetNorm) >= 0.72f)
                        {
                            victims.Add(agent);
                        }
                    }
                }
            }

            foreach (Agent victim in victims)
            {
                if (!victim.IsActive()) continue;

                Blow blow = new Blow(player.Index);
                blow.InflictedDamage = 99999;
                blow.DamageType = DamageTypes.Cut;
                blow.BlowFlag = BlowFlags.CrushThrough;
                blow.GlobalPosition = victim.Position;
                blow.Direction = aimDir;

                try
                {
                    victim.Die(blow, Agent.KillInfo.Invalid);
                    MatrixFrame burnFrame = MatrixFrame.Identity;
                    burnFrame.origin = victim.Position;
                    Mission.Current.AddParticleSystemBurstByName("psys_game_burning_agent", burnFrame, false);
                }
                catch { }

                if (victim.MountAgent != null && victim.MountAgent.IsActive())
                {
                    try
                    {
                        Blow mountBlow = new Blow(player.Index);
                        mountBlow.InflictedDamage = 99999;
                        mountBlow.DamageType = DamageTypes.Cut;
                        mountBlow.GlobalPosition = victim.MountAgent.Position;
                        victim.MountAgent.Die(mountBlow, Agent.KillInfo.Invalid);
                    }
                    catch { }
                }

                count++;
            }

            CheatNotify.Show(string.Format("[Cannibal's Wisp] Cannibal's dragon flames scorched {0} enemies to cinder!", count));
        }
        #endregion

        #region 4. Deathly Hallows (Kill All Enemies)
        /// <summary>
        /// Kill all enemies: Last resort ultimate to win the battle immediately when bored of the battle.
        /// </summary>
        public static void CastDeathlyHallows()
        {
            if (Mission.Current == null || Agent.Main == null)
            {
                CheatNotify.Show("[RoT Cheats] Deathly Hallows can only be invoked during battle missions.");
                return;
            }

            Agent player = Agent.Main;

            // Ominous doom chime sound
            try
            {
                SoundEvent.PlaySound2D("event:/map/ambient/node/siege/trebuchet_fire");
            }
            catch { }

            int count = 0;
            Blow blow = new Blow(player.Index);
            blow.InflictedDamage = 99999;
            blow.DamageType = DamageTypes.Blunt;

            List<Agent> enemies = new List<Agent>();
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent != null && agent.IsActive() && agent != player && agent.IsEnemyOf(player))
                {
                    enemies.Add(agent);
                }
            }

            foreach (Agent enemy in enemies)
            {
                if (!enemy.IsActive()) continue;

                blow.GlobalPosition = enemy.Position;
                try
                {
                    enemy.Die(blow, Agent.KillInfo.Invalid);
                    count++;
                }
                catch { }
            }

            CheatNotify.Show(string.Format("[Deathly Hallows] Master of Death: {0} enemies eliminated! The battle is won.", count));
        }
        #endregion

        #region Battle Skills Quick Selection Menu
        public static void OpenSkillCastMenu()
        {
            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive())
            {
                CheatNotify.Show("[RoT Cheats] Battle Skills can only be cast during battle.");
                return;
            }

            List<InquiryElement> elements = new List<InquiryElement>();
            elements.Add(new InquiryElement("sweeping_force", "💨 Sweeping Force [Numpad 1 / U]", null, true, "Fus-ro-dah shout: Hurls and kills all enemies in front within medium range."));
            elements.Add(new InquiryElement("rhllor_light", "🔥 R'hllor's Light [Numpad 2 / I]", null, true, "Aimed firestorm barrage: Unleashes a swirling vortex of fire across a large circular area."));
            elements.Add(new InquiryElement("cannibal_wisp", "🐉 Cannibal's Wisp [Numpad 3 / O]", null, true, "Dragon flame spray: Fires a stream of roaring dragon fire scorching enemies in the aimed direction."));
            elements.Add(new InquiryElement("deathly_hallows", "💀 Deathly Hallows [Numpad 4 / P]", null, true, "Last resort: Instantly obliterates all enemies on the battlefield to win immediately."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "⚡ Battle Skills & Magic (Realm of Thrones)",
                "Select a skill to unleash immediately (or use hotkeys in combat):",
                elements,
                true,
                1,
                1,
                "Unleash Skill",
                "Cancel",
                delegate(List<InquiryElement> selected)
                {
                    if (selected == null || selected.Count == 0) return;
                    string id = selected[0].Identifier as string;
                    switch (id)
                    {
                        case "sweeping_force":
                            CastSweepingForce();
                            break;
                        case "rhllor_light":
                            CastRhllorLight();
                            break;
                        case "cannibal_wisp":
                            CastCannibalWisp();
                            break;
                        case "deathly_hallows":
                            CastDeathlyHallows();
                            break;
                    }
                },
                null,
                "",
                false
            );

            MBInformationManager.ShowMultiSelectionInquiry(data, true, false);
        }
        #endregion
    }
}
