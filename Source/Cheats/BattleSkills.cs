using System;
using System.Collections.Generic;
using RoTCheats.UI;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace RoTCheats.Cheats
{
    public class FireMissile
    {
        public Vec3 StartPos;
        public Vec3 TargetPos;
        public float Duration;
        public float Elapsed;
        public float ArcHeight;
        public bool HasExploded;

        public FireMissile(Vec3 start, Vec3 target, float duration, float arcHeight)
        {
            StartPos = start;
            TargetPos = target;
            Duration = duration;
            Elapsed = 0f;
            ArcHeight = arcHeight;
            HasExploded = false;
        }
    }

    public static class BattleSkills
    {
        #region State
        public static bool IsCannibalWispActive = false;
        private static float _wispSoundTimer = 0f;
        private static float _aimReticleTimer = 0f;
        private static float _rhllorAttackCooldown = 0f;
        private static readonly List<FireMissile> _activeMissiles = new List<FireMissile>();
        #endregion

        #region Reset
        public static void ResetState()
        {
            IsCannibalWispActive = false;
            _wispSoundTimer = 0f;
            _aimReticleTimer = 0f;
            _rhllorAttackCooldown = 0f;
            _activeMissiles.Clear();
        }
        #endregion

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
                blastDir.z += 0.45f;
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

        #region 2. R'hllor's Light (Item with Radial Aim & Fire Missiles)

        public static bool IsPlayerHoldingRhllorLight()
        {
            Agent player = Agent.Main;
            if (player == null || !player.IsActive()) return false;

            if (!player.WieldedWeapon.IsEmpty && player.WieldedWeapon.Item != null)
            {
                string id = player.WieldedWeapon.Item.StringId;
                if (id == ItemCheats.RhllorLightId || id == "lightbringer")
                {
                    return true;
                }
            }

            if (!player.WieldedOffhandWeapon.IsEmpty && player.WieldedOffhandWeapon.Item != null)
            {
                string offId = player.WieldedOffhandWeapon.Item.StringId;
                if (offId == ItemCheats.RhllorLightId || offId == "lightbringer")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PlayerHasRhllorLightInEquipment()
        {
            Agent player = Agent.Main;
            if (player == null) return false;

            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                MissionWeapon w = player.Equipment[i];
                if (!w.IsEmpty && w.Item != null)
                {
                    string id = w.Item.StringId;
                    if (id == ItemCheats.RhllorLightId || id == "lightbringer")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Raycasts to find the exact target ground/wall/object point where the player is aiming.
        /// </summary>
        public static Vec3 GetAimTargetPoint(float maxDistance = 250f)
        {
            Agent player = Agent.Main;
            if (player == null) return Vec3.Zero;

            Vec3 eyePos = player.GetEyeGlobalPosition();
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

            Vec3 rayEnd = eyePos + (aimDir * maxDistance);
            float hitDistance = 0f;
            Vec3 targetPoint = Vec3.Zero;
            bool hit = false;
            try
            {
                hit = Mission.Current.Scene.RayCastForClosestEntityOrTerrain(eyePos, rayEnd, out hitDistance, out targetPoint, 0.05f, BodyFlags.None);
            }
            catch
            {
                hit = false;
            }

            if (!hit || hitDistance <= 0.2f)
            {
                Vec3 projectedPos = eyePos + (aimDir * 40f);
                float gh = Mission.Current.Scene.GetGroundHeightAtPosition(projectedPos, BodyFlags.None);
                targetPoint = new Vec3(projectedPos.x, projectedPos.y, Math.Max(projectedPos.z, gh));
            }

            return targetPoint;
        }

        /// <summary>
        /// Renders the radial area of attack indicator on the ground where the player is aiming.
        /// </summary>
        public static void RenderRadialAreaReticle(Vec3 targetCenter, float radius = 15f)
        {
            if (Mission.Current == null) return;

            try
            {
                // Center epicenter beacon
                MatrixFrame centerFrame = MatrixFrame.Identity;
                centerFrame.origin = targetCenter + new Vec3(0, 0, 0.3f);
                Mission.Current.AddParticleSystemBurstByName("psys_game_missile_flame", centerFrame, false);

                // 16-point glowing radial perimeter ring hugging the ground
                int points = 16;
                for (int i = 0; i < points; i++)
                {
                    float angle = (float)(i * (Math.PI * 2.0 / points));
                    Vec3 ringPoint = new Vec3(
                        targetCenter.x + (float)Math.Cos(angle) * radius,
                        targetCenter.y + (float)Math.Sin(angle) * radius,
                        targetCenter.z
                    );

                    try
                    {
                        float gh = Mission.Current.Scene.GetGroundHeightAtPosition(ringPoint, BodyFlags.None);
                        ringPoint.z = Math.Max(ringPoint.z, gh) + 0.25f;
                    }
                    catch { }

                    MatrixFrame ringFrame = MatrixFrame.Identity;
                    ringFrame.origin = ringPoint;
                    Mission.Current.AddParticleSystemBurstByName("psys_torch_fire_moving", ringFrame, false);
                }
            }
            catch { }
        }

        /// <summary>
        /// Launches a barrage of fire missiles towards the aimed radial area.
        /// </summary>
        public static void LaunchRhllorMissiles(Vec3? customTarget = null)
        {
            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive())
            {
                CheatNotify.Show("[RoT Cheats] R'hllor's Light can only be unleashed during battle missions.");
                return;
            }

            Agent player = Agent.Main;
            Vec3 targetCenter = customTarget.HasValue ? customTarget.Value : GetAimTargetPoint(250f);
            Vec3 eyePos = player.GetEyeGlobalPosition();
            Vec3 weaponStart = eyePos + player.LookDirection * 0.8f + new Vec3(0, 0, 0.2f);

            // Audio: Missile launch whoosh
            try
            {
                SoundEvent.PlaySound2D("event:/map/ambient/node/siege/mangonel_fire");
                SoundEvent.PlaySound2D("event:/mission/ambient/detail/fire/fire_big");
            }
            catch { }

            // Upper body attack action
            try
            {
                ActionIndexCache act = ActionIndexCache.Create("act_ready_thrust_1h");
                player.SetActionChannel(1, ref act, false, (AnimFlags)0, 0f, 1.5f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
            catch { }

            // Launch 5 fiery missiles in an arc towards points within the radial area
            float aoeRadius = 15f;
            Random rand = new Random();

            _activeMissiles.Add(new FireMissile(weaponStart, targetCenter, 0.55f, 12f));

            for (int i = 0; i < 4; i++)
            {
                double angle = rand.NextDouble() * Math.PI * 2;
                double dist = rand.NextDouble() * (aoeRadius * 0.75f);
                Vec3 subTarget = new Vec3(
                    targetCenter.x + (float)(Math.Cos(angle) * dist),
                    targetCenter.y + (float)(Math.Sin(angle) * dist),
                    targetCenter.z
                );
                try
                {
                    float gh = Mission.Current.Scene.GetGroundHeightAtPosition(subTarget, BodyFlags.None);
                    subTarget.z = Math.Max(subTarget.z, gh);
                }
                catch { }

                float flightTime = 0.55f + (i * 0.08f);
                float arcHeight = 12f + (i * 2.5f);
                _activeMissiles.Add(new FireMissile(weaponStart, subTarget, flightTime, arcHeight));
            }

            CheatNotify.Show("[R'hllor's Light] 🔥 Volley of fire missiles launched at target area!");
        }

        /// <summary>
        /// Updates all flying fire missiles in the air, rendering flame trails and detonating on impact.
        /// </summary>
        public static void UpdateFireMissiles(float dt)
        {
            if (Mission.Current == null || _activeMissiles.Count == 0) return;

            for (int i = _activeMissiles.Count - 1; i >= 0; i--)
            {
                FireMissile m = _activeMissiles[i];
                m.Elapsed += dt;
                float t = Math.Min(1f, m.Elapsed / m.Duration);

                // Parabolic ballistic trajectory
                Vec3 currentPos = Vec3.Lerp(m.StartPos, m.TargetPos, t);
                currentPos.z += (float)Math.Sin(t * Math.PI) * m.ArcHeight;

                // Fire trail particle
                try
                {
                    MatrixFrame trailFrame = MatrixFrame.Identity;
                    trailFrame.origin = currentPos;
                    Mission.Current.AddParticleSystemBurstByName("psys_game_burning_jar_trail", trailFrame, false);
                    Mission.Current.AddParticleSystemBurstByName("psys_game_missile_flame", trailFrame, false);
                }
                catch { }

                // Detonate on impact
                if (t >= 1f && !m.HasExploded)
                {
                    m.HasExploded = true;
                    ExplodeRhllorArea(m.TargetPos, 16f);
                    _activeMissiles.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Detonates a cataclysmic fire explosion across the targeted radial area.
        /// </summary>
        public static void ExplodeRhllorArea(Vec3 targetCenter, float radius)
        {
            if (Mission.Current == null || Agent.Main == null) return;

            Agent player = Agent.Main;

            // Explosion visuals
            try
            {
                MatrixFrame centerFrame = MatrixFrame.Identity;
                centerFrame.origin = targetCenter;
                Mission.Current.AddParticleSystemBurstByName("psys_game_burning_boulder_coll", centerFrame, false);
                Mission.Current.AddParticleSystemBurstByName("psys_fire_vertical", centerFrame, false);

                // Multi-burst explosion shockwave
                for (int i = 0; i < 8; i++)
                {
                    float angle = (float)(i * (Math.PI * 2.0 / 8));
                    float r = 5f + ((i % 2) * 6f);
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
                    Mission.Current.AddParticleSystemBurstByName("psys_battleground_env_fire", bFrame, false);
                }
            }
            catch { }

            // Audio: explosion
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
                    if (agent.Position.Distance(targetCenter) <= radius)
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

            if (count > 0)
            {
                CheatNotify.Show(string.Format("[R'hllor's Light] Firestorm explosion incinerated {0} enemies!", count));
            }
        }

        /// <summary>
        /// Handles tick updates for R'hllor's Light: reticle rendering and attack triggers when held.
        /// </summary>
        public static void TickRhllorsLight(float dt)
        {
            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive()) return;

            UpdateFireMissiles(dt);

            _rhllorAttackCooldown -= dt;

            bool isHolding = IsPlayerHoldingRhllorLight();
            if (!isHolding) return;

            // Render radial area reticle where the player aims
            Vec3 aimTarget = GetAimTargetPoint(250f);
            _aimReticleTimer -= dt;
            if (_aimReticleTimer <= 0f)
            {
                _aimReticleTimer = 0.08f;
                RenderRadialAreaReticle(aimTarget, 15f);
            }

            // Upon attack press (Left Click while wielding R'hllor's Light)
            if (Input.IsKeyPressed(InputKey.LeftMouseButton))
            {
                if (_rhllorAttackCooldown <= 0f)
                {
                    _rhllorAttackCooldown = 0.7f;
                    LaunchRhllorMissiles(aimTarget);
                }
            }
        }

        public static void CastRhllorLight()
        {
            LaunchRhllorMissiles();
        }
        #endregion

        #region 3. Cannibal's Wisp (Continuous Fire Spray Toggled From Hand)

        public static void CastCannibalWisp()
        {
            ToggleCannibalWisp();
        }

        public static void ToggleCannibalWisp()
        {
            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive())
            {
                CheatNotify.Show("[RoT Cheats] Cannibal's Wisp can only be toggled during battle missions.");
                return;
            }

            IsCannibalWispActive = !IsCannibalWispActive;

            if (IsCannibalWispActive)
            {
                CheatNotify.Show("[Cannibal's Wisp] 🔥 Fire spray ACTIVATED! Roaring flames stream from your hand.");
            }
            else
            {
                // Reset upper body posture
                try
                {
                    ActionIndexCache actNone = ActionIndexCache.act_none;
                    Agent.Main.SetActionChannel(1, ref actNone, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                }
                catch { }

                CheatNotify.Show("[Cannibal's Wisp] 💨 Fire spray DEACTIVATED.");
            }
        }

        /// <summary>
        /// Called every tick while Cannibal's Wisp is toggled ON.
        /// Maintains outstretched casting hand animation and sprays continuous fire from the hand bone.
        /// </summary>
        public static void TickCannibalWisp(float dt)
        {
            if (!IsCannibalWispActive) return;

            if (Mission.Current == null || Agent.Main == null || !Agent.Main.IsActive())
            {
                IsCannibalWispActive = false;
                return;
            }

            Agent player = Agent.Main;

            // 1. Maintain Outstretched Hand Animation (Channel 1 = Upper Body, allowing free running/riding)
            try
            {
                ActionIndexCache act = ActionIndexCache.Create("act_ready_thrust_1h");
                player.SetActionChannel(1, ref act, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
            catch { }

            // 2. Compute Hand Bone Global Position
            Vec3 handPos;
            try
            {
                Skeleton skel = player.AgentVisuals != null ? player.AgentVisuals.GetSkeleton() : null;
                if (skel != null)
                {
                    MatrixFrame handLocal = skel.GetBoneEntitialFrameWithName("r_hand");
                    MatrixFrame visualFrame = player.AgentVisuals.GetFrame();
                    MatrixFrame handWorld = visualFrame.TransformToParent(ref handLocal);
                    handPos = handWorld.origin;
                }
                else
                {
                    handPos = player.GetEyeGlobalPosition() + player.LookDirection * 0.45f + new Vec3(0, 0, -0.2f);
                }
            }
            catch
            {
                handPos = player.GetEyeGlobalPosition() + player.LookDirection * 0.45f + new Vec3(0, 0, -0.2f);
            }

            // Aim direction
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

            // 3. Spray Fire Particles from Hand Forward
            float sprayRange = 36f;
            try
            {
                // Hand emission burst
                MatrixFrame handMtx = MatrixFrame.Identity;
                handMtx.origin = handPos;
                Mission.Current.AddParticleSystemBurstByName("psys_torch_fire_moving", handMtx, false);

                // Continuous flame stream extending forward
                for (float d = 1.5f; d <= sprayRange; d += 2.5f)
                {
                    Vec3 firePoint = handPos + (aimDir * d);
                    try
                    {
                        float gh = Mission.Current.Scene.GetGroundHeightAtPosition(firePoint, BodyFlags.None);
                        if (firePoint.z < gh + 0.25f)
                        {
                            firePoint.z = gh + 0.25f;
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

            // 4. Roaring fire sound
            _wispSoundTimer -= dt;
            if (_wispSoundTimer <= 0f)
            {
                _wispSoundTimer = 0.8f;
                try
                {
                    SoundEvent.PlaySound2D("event:/mission/ambient/detail/fire/fire_big");
                }
                catch { }
            }

            // 5. Continuous Lethal Damage to Enemies in the Flame Cone
            List<Agent> victims = new List<Agent>();
            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent != null && agent.IsActive() && agent != player && agent.IsEnemyOf(player))
                {
                    Vec3 toTarget = agent.Position - handPos;
                    float dist = toTarget.Length;
                    if (dist <= sprayRange)
                    {
                        Vec3 toTargetNorm = toTarget;
                        toTargetNorm.Normalize();
                        if (Vec3.DotProduct(aimDir, toTargetNorm) >= 0.70f)
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
            }
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
            elements.Add(new InquiryElement("rhllor_missiles", "🔥 R'hllor's Light [Numpad 2 / I]", null, true, "Aimed radial firestorm: Launches fire missiles and catastrophic explosion at the targeted radial area."));
            elements.Add(new InquiryElement("spawn_rhllor_item", "⚔️ Spawn & Equip R'hllor's Light Weapon", null, true, "Cheats the flaming R'hllor's Light sword into your inventory and equips it."));
            elements.Add(new InquiryElement("toggle_cannibal_wisp", string.Format("🐉 Cannibal's Wisp (Toggle Spray: {0}) [Numpad 3 / O]", IsCannibalWispActive ? "ON" : "OFF"), null, true, "Toggles continuous hand flame spray with outstretched casting animation."));
            elements.Add(new InquiryElement("deathly_hallows", "💀 Deathly Hallows [Numpad 4 / P]", null, true, "Last resort: Instantly obliterates all enemies on the battlefield to win immediately."));

            MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                "⚡ Battle Skills & Magic (Realm of Thrones)",
                "Select a skill to unleash immediately (or use hotkeys in combat):",
                elements,
                true,
                1,
                1,
                "Select",
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
                        case "rhllor_missiles":
                            LaunchRhllorMissiles();
                            break;
                        case "spawn_rhllor_item":
                            ItemCheats.GiveRhllorsLight(true);
                            break;
                        case "toggle_cannibal_wisp":
                            ToggleCannibalWisp();
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
