﻿using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.Agent;

namespace Alliance.Common.Core.Utils
{
	static class CoreUtils
	{
		public static void TakeDamage(Agent victim, int damage, float magnitude = 50f)
		{
			TakeDamage(victim, victim, damage, magnitude);
		}

		public static void TakeDamage(Agent victim, Agent attacker, int damage, float magnitude = 50f)
		{
			if (victim == null || attacker == null)
			{
				Utilities.Logger.Log("Victim and/or attacker is null. Damage skipped", Utilities.Logger.LogLevel.Warning);
				return;
			};

			if (victim.Health <= 0) return;

			Blow blow = new Blow(attacker.Index);
			blow.DamageType = DamageTypes.Pierce;
			blow.BoneIndex = victim.Monster.HeadLookDirectionBoneIndex;
			blow.GlobalPosition = victim.Position;
			blow.GlobalPosition.z = blow.GlobalPosition.z + victim.GetEyeGlobalHeight();
			blow.BaseMagnitude = magnitude;
			blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
			blow.InflictedDamage = damage;
			blow.SwingDirection = victim.LookDirection;
			MatrixFrame frame = victim.Frame;
			blow.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
			blow.SwingDirection.Normalize();
			blow.Direction = blow.SwingDirection;
			blow.DamageCalculated = true;
			sbyte mainHandItemBoneIndex = attacker.Monster.MainHandItemBoneIndex;
			AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(
				false,
				false,
				false,
				true,
				false,
				false,
				false,
				false,
				false,
				false,
				false,
				false,
				CombatCollisionResult.StrikeAgent,
				-1,
				0,
				2,
				blow.BoneIndex,
				BoneBodyPartType.Head,
				mainHandItemBoneIndex,
				UsageDirection.AttackLeft,
				-1,
				CombatHitResultFlags.NormalHit,
				0.5f,
				1f,
				0f,
				0f,
				0f,
				0f,
				0f,
				0f,
				Vec3.Up,
				blow.Direction,
				blow.GlobalPosition,
				Vec3.Zero,
				Vec3.Zero,
				victim.Velocity,
				Vec3.Up
			);
			victim.RegisterBlow(blow, attackCollisionDataForDebugPurpose);
		}

		/// <summary>
		/// Return the list of all agents alives that are near the target.
		/// IT WILL NOT INCLUDE THE MOUNT OF THE TARGET IF THE TARGET IS MOUNTED
		/// </summary>
		/// <param name="range"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static List<Agent> GetNearAliveAgentsInRange(float range, Agent target)
		{
			//Contain all agent (Players/Bots/Ridings)
			List<Agent> allAgents = Mission.Current.AllAgents;
			List<Agent> agentsInRange = new List<Agent>();

			foreach (Agent agent in allAgents)
			{
				if (!agent.IsActive()) continue;

				// Do not include mount of player and player.
				if (agent == target.MountAgent || agent == target) continue;

				float distance = agent.Position.Distance(target.Position);

				//Add offset in case of mount since mount are large
				if (agent.IsMount)
				{
					distance -= 0.5f;
				}

				if (distance < range)
				{
					agentsInRange.Add(agent);
				}
			}

			return agentsInRange;
		}

		/// <summary>
		/// Return the number of seconds since the mission started.
		/// </summary>
		public static float GetMissionTimeInSeconds(this Mission mission)
		{
			return mission.MissionTimeTracker.NumberOfTicks / 10000000f;
		}
	}
}
