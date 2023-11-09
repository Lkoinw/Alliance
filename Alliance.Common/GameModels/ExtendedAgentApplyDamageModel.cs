﻿using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace Alliance.Common.GameModels
{
    public class ExtendedAgentApplyDamageModel : AgentApplyDamageModel
    {
        public ExtendedAgentApplyDamageModel()
        {
        }

        public override float CalculateDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
        {
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            AttackCollisionData attackCollisionData = collisionData;
            if (currentUsageItem != null && currentUsageItem.IsMeleeWeapon && currentUsageItem.ItemUsage.Contains("customweapon"))
            {
                if (attackCollisionData.StrikeType == 1 && !attackCollisionData.IsAlternativeAttack)
                {
                    float num = currentUsageItem.ThrustDamage;
                    baseDamage += num;
                }
                if (attackCollisionData.StrikeType == 0 && !attackCollisionData.IsAlternativeAttack)
                {
                    float num2 = currentUsageItem.SwingDamage;
                    baseDamage += num2;
                }
            }
            return baseDamage;

            //if (weapon.IsEmpty && (attackInformation.AttackerAgentCharacter?.Name.ToString().ToLower().Contains("troll") ?? false))
            //{
            //    return baseDamage * 100;
            //}
            //return baseDamage;
        }

        public override void DecideMissileWeaponFlags(Agent attackerAgent, MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
        {
        }

        public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsage)
        {
            if (attackerAgent.Name.ToLower().Contains("troll")) return true;

            EquipmentIndex equipmentIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (equipmentIndex == EquipmentIndex.None)
            {
                equipmentIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            }
            WeaponComponentData weaponComponentData = equipmentIndex != EquipmentIndex.None ? attackerAgent.Equipment[equipmentIndex].CurrentUsageItem : null;
            if (weaponComponentData == null || isPassiveUsage || !weaponComponentData.WeaponFlags.HasAnyFlag(WeaponFlags.CanCrushThrough) || strikeType != StrikeType.Swing || attackDirection != Agent.UsageDirection.AttackUp)
            {
                return false;
            }
            float num = 58f;
            if (defendItem != null && defendItem.IsShield)
            {
                num *= 1.2f;
            }
            return totalAttackEnergy > num;
        }

        public override bool CanWeaponDismount(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            if (attackerAgent.Name.ToLower().Contains("troll")) return true;

            return MBMath.IsBetween((int)blow.VictimBodyPart, 0, 6) && (!attackerAgent.HasMount && blow.StrikeType == StrikeType.Swing && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanHook) || blow.StrikeType == StrikeType.Thrust && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanDismount));
        }

        public override void CalculateCollisionStunMultipliers(Agent attackerAgent, Agent defenderAgent, bool isAlternativeAttack, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, out float attackerStunMultiplier, out float defenderStunMultiplier)
        {
            if (defenderAgent.Name.ToLower().Contains("troll"))
            {
                attackerStunMultiplier = 1f;
                defenderStunMultiplier = 0f;
            }
            else if (attackerAgent.Name.ToLower().Contains("troll"))
            {
                attackerStunMultiplier = 0f;
                defenderStunMultiplier = 1f;
            }
            else
            {
                attackerStunMultiplier = 1f;
                defenderStunMultiplier = 1f;
            }
        }

        public override bool CanWeaponKnockback(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            if (attackerAgent.Name.ToLower().Contains("troll")) return true;

            AttackCollisionData attackCollisionData = collisionData;
            return MBMath.IsBetween((int)attackCollisionData.VictimHitBodyPart, 0, 6) && !attackerWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown) && (attackerWeapon.IsConsumable || (blow.BlowFlag & BlowFlags.CrushThrough) != BlowFlags.None || blow.StrikeType == StrikeType.Thrust && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.WideGrip));
        }

        public override bool CanWeaponKnockDown(Agent attackerAgent, Agent victimAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            if (attackerAgent.Name.ToLower().Contains("troll")) return true;

            if (attackerWeapon.WeaponClass == WeaponClass.Boulder)
            {
                return true;
            }
            AttackCollisionData attackCollisionData = collisionData;
            BoneBodyPartType victimHitBodyPart = attackCollisionData.VictimHitBodyPart;
            bool flag = MBMath.IsBetween((int)victimHitBodyPart, 0, 6);
            if (!victimAgent.HasMount && victimHitBodyPart == BoneBodyPartType.Legs)
            {
                flag = true;
            }
            return flag && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown) && (attackerWeapon.IsPolearm && blow.StrikeType == StrikeType.Thrust || attackerWeapon.IsMeleeWeapon && blow.StrikeType == StrikeType.Swing && MissionCombatMechanicsHelper.DecideSweetSpotCollision(collisionData));
        }

        public override float GetDismountPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            float num = 0f;
            if (blow.StrikeType == StrikeType.Swing && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanHook))
            {
                num += 0.25f;
            }
            return num;
        }

        public override float GetKnockBackPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            return 0f;
        }

        public override float GetKnockDownPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            float num = 0f;
            if (attackerWeapon.WeaponClass == WeaponClass.Boulder)
            {
                num += 0.25f;
            }
            else if (attackerWeapon.IsMeleeWeapon)
            {
                AttackCollisionData attackCollisionData2 = attackCollisionData;
                if (attackCollisionData2.VictimHitBodyPart == BoneBodyPartType.Legs && blow.StrikeType == StrikeType.Swing)
                {
                    num += 0.1f;
                }
                else
                {
                    attackCollisionData2 = attackCollisionData;
                    if (attackCollisionData2.VictimHitBodyPart == BoneBodyPartType.Head)
                    {
                        num += 0.15f;
                    }
                }
            }
            return num;
        }

        public override float GetHorseChargePenetration()
        {
            return 0.37f;
        }

        public override float CalculateStaggerThresholdMultiplier(Agent defenderAgent)
        {
            return 1f;
        }

        public override float CalculatePassiveAttackDamage(BasicCharacterObject attackerCharacter, in AttackCollisionData collisionData, float baseDamage)
        {
            return baseDamage;
        }

        public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit)
        {
            return MeleeCollisionReaction.Bounced;
        }

        public override float CalculateShieldDamage(in AttackInformation attackInformation, float baseDamage)
        {
            baseDamage *= 1.25f;
            MissionMultiplayerFlagDomination missionBehavior = Mission.Current.GetMissionBehavior<MissionMultiplayerFlagDomination>();
            if (missionBehavior != null && missionBehavior.GetMissionType() == MissionLobbyComponent.MultiplayerGameType.Captain)
            {
                return baseDamage * 0.5f;
            }
            return baseDamage;
        }

        public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman)
        {
            float num = 1f;
            switch (bodyPart)
            {
                case BoneBodyPartType.None:
                    num = 1f;
                    break;
                case BoneBodyPartType.Head:
                    switch (type)
                    {
                        case DamageTypes.Invalid:
                            num = 2f;
                            break;
                        case DamageTypes.Cut:
                            num = 1.2f;
                            break;
                        case DamageTypes.Pierce:
                            if (isHuman)
                            {
                                num = 2f;
                            }
                            else
                            {
                                num = 1.2f;
                            }
                            break;
                        case DamageTypes.Blunt:
                            num = 1.2f;
                            break;
                    }
                    break;
                case BoneBodyPartType.Neck:
                    switch (type)
                    {
                        case DamageTypes.Invalid:
                            num = 2f;
                            break;
                        case DamageTypes.Cut:
                            num = 1.2f;
                            break;
                        case DamageTypes.Pierce:
                            if (isHuman)
                            {
                                num = 2f;
                            }
                            else
                            {
                                num = 1.2f;
                            }
                            break;
                        case DamageTypes.Blunt:
                            num = 1.2f;
                            break;
                    }
                    break;
                case BoneBodyPartType.Chest:
                case BoneBodyPartType.Abdomen:
                case BoneBodyPartType.ShoulderLeft:
                case BoneBodyPartType.ShoulderRight:
                case BoneBodyPartType.ArmLeft:
                case BoneBodyPartType.ArmRight:
                    if (isHuman)
                    {
                        num = 1f;
                    }
                    else
                    {
                        num = 0.8f;
                    }
                    break;
                case BoneBodyPartType.Legs:
                    num = 0.8f;
                    break;
            }
            return num;
        }

        public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon)
        {
            return weapon != null && weapon.IsConsumable && weapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanPenetrateShield) && weapon.WeaponFlags.HasAnyFlag(WeaponFlags.MultiplePenetration);
        }

        public override bool DecideAgentShrugOffBlow(Agent victimAgent, AttackCollisionData collisionData, in Blow blow)
        {
            if (victimAgent.Name.ToLower().Contains("troll"))
            {
                return true;
            }

            return MissionCombatMechanicsHelper.DecideAgentShrugOffBlow(victimAgent, collisionData, blow);
        }

        public override bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            if (attackerAgent.Name.ToLower().Contains("troll"))
            {
                return true;
            }

            return MissionCombatMechanicsHelper.DecideAgentDismountedByBlow(attackerAgent, victimAgent, collisionData, attackerWeapon, blow);
        }

        public override bool DecideAgentKnockedBackByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            if (victimAgent.Name.ToLower().Contains("troll"))
            {
                return false;
            }

            return MissionCombatMechanicsHelper.DecideAgentKnockedBackByBlow(attackerAgent, victimAgent, collisionData, attackerWeapon, blow);
        }

        public override bool DecideAgentKnockedDownByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            if (victimAgent.Name.ToLower().Contains("troll"))
            {
                return false;
            }
            else if (attackerAgent.Name.ToLower().Contains("troll"))
            {
                return true;
            }

            return MissionCombatMechanicsHelper.DecideAgentKnockedDownByBlow(attackerAgent, victimAgent, collisionData, attackerWeapon, blow);
        }

        public override bool DecideMountRearedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            if (attackerAgent.Name.ToLower().Contains("troll"))
            {
                return true;
            }

            return MissionCombatMechanicsHelper.DecideMountRearedByBlow(attackerAgent, victimAgent, collisionData, attackerWeapon, blow);
        }
    }
}