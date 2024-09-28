using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System.Linq;

namespace RotationsProject.Healer
{
    [Rotation("Akira_AST", CombatType.PvE, Description = "Akira's rotation for AST v0.11b", GameVersion = "7.05")]
    [SourceCode(Path = "main/RotationsProject/Healer/Akira_AST.cs")]
    [Api(4)]
    public class Akira_AST : AstrologianRotation
    {
        #region Config Options
        [Range(0, 1, ConfigUnitType.Percent)]
        [RotationConfig(CombatType.PvE, Name = "Minimum HP threshold party member need for Aspected Benefic")]
        public float AspectedBeneHeal { get; set; } = 0.5f;

        [Range(0, 1, ConfigUnitType.Percent)]
        [RotationConfig(CombatType.PvE, Name = "HP Percent for using Essential Dignity")]
        public float EssDigHeal { get; set; } = 0.44f;

        [Range(0, 1, ConfigUnitType.Percent)]
        [RotationConfig(CombatType.PvE, Name = "Average Party HP for using Horoscope Helios")]
        public float HoroHelios { get; set; } = 0.85f;

        [Range(0, 1, ConfigUnitType.Percent)]
        [RotationConfig(CombatType.PvE, Name = "Target's HP Percent for Arrow")]
        public float ArrowUse { get; set; } = 0.8f;

        [Range(0, 1, ConfigUnitType.Percent)]
        [RotationConfig(CombatType.PvE, Name = "Target's HP Percent for Ewer")]
        public float EwerUse { get; set; } = 0.8f;
        #endregion

        #region Emergency Action
        protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
        {
            if (base.EmergencyAbility(nextGCD, out act)) return true;
            if (!InCombat) return false;
            // Weave lucid dreaming during combat
            if (CurrentMp <= 6000 && InCombat && LucidDreamingPvE.CanUse(out act)) return true;
            // Buff cards
            if (InCombat && (!DivinationPvE.Cooldown.WillHaveOneCharge(70) ||
                              DivinationPvE.Cooldown.HasOneCharge ||
                              DivinationPvE.Cooldown.WillHaveOneChargeGCD(gcdCount: 1)))
            {
                if (InCombat && TheBalancePvE.CanUse(out act) && !(TheBalancePvE.Target.Target.HasStatus(false, StatusID.Weakness))) return true;
                if (InCombat && TheSpearPvE.CanUse(out act) && !(TheSpearPvE.Target.Target.HasStatus(false, StatusID.Weakness))) return true;
            }
            // Healing
            if (PartyMembersAverHP < 0.85f)
            {
                if (MicrocosmosPvE.CanUse(out act)) return true;
                else
                {
                    if (LightspeedPvE.CanUse(out act) && !Player.HasStatus(true, StatusID.Lightspeed)) return true;
                    //if (nextGCD.IsTheSameTo(true, AspectedHeliosPvE, HeliosPvE))
                    //{

                    //    if (StellarDetonationPvE.CanUse(out act)) return true;
                    //    else if (CelestialOppositionPvE.CanUse(out act)) return true;
                    //    else if (HoroscopePvE.CanUse(out act)) return true;
                    //    else if (NeutralSectPvE.CanUse(out act)) return true;
                    //}
                }
            }
            if (nextGCD.IsTheSameTo(true, BeneficPvE, BeneficIiPvE, AspectedBeneficPvE))
            {
                if (SynastryPvE.CanUse(out act)) return true;
            }

            return base.EmergencyAbility(nextGCD, out act);
        }
        #endregion

        #region Countdown Logic
        protected override IAction? CountDownAction(float remainTime)
        {
            if (remainTime < MaleficPvE.Info.CastTime + CountDownAhead
                && MaleficPvE.CanUse(out var act)) return act;
            if (remainTime < 3 && UseBurstMedicine(out act)) return act;
            return base.CountDownAction(remainTime);
        }
        #endregion

        #region Defense Logic
        [RotationDesc(ActionID.TheSpirePvE, ActionID.TheBolePvE, ActionID.ExaltationPvE, ActionID.CelestialIntersectionPvE)]
        protected override bool DefenseSingleAbility(IAction nextGCD, out IAction act)
        {
            // Cards
            if (TheSpirePvE.CanUse(out act) && InCombat) return true;
            if (TheBolePvE.CanUse(out act) && InCombat) return true;
            // oGCD
            if (ExaltationPvE.CanUse(out act)) return true;
            if (CelestialIntersectionPvE.CanUse(out act)) return true;
            return base.DefenseSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.SunSignPvE, ActionID.NeutralSectPvE, ActionID.CollectiveUnconsciousPvE)]
        protected override bool DefenseAreaAbility(IAction nextGCD, out IAction act)
        {
            if (SunSignPvE.CanUse(out act)) return true; // Sun Sign should be prioritized when available
            if (NeutralSectPvE.CanUse(out act)) return true; // Big cooldown so wanna use this more often
            if (CollectiveUnconsciousPvE.CanUse(out act)) return true;
            return base.DefenseAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.MacrocosmosPvE)]
        protected override bool DefenseAreaGCD(out IAction act)
        {
            act = null;
            if(CollectiveUnconsciousPvE.CanUse(out _))
            {
                return false;
            }
            else
            {
                if (MacrocosmosPvE.CanUse(out act) && !Player.HasStatus(true, StatusID.Horoscope, StatusID.HoroscopeHelios)) return true;
            }
            return base.DefenseAreaGCD(out act);
        }
        #endregion

        #region Heal Logic
        [RotationDesc(ActionID.TheArrowPvE, ActionID.TheEwerPvE, ActionID.CelestialIntersectionPvE, ActionID.EssentialDignityPvE)]
        protected override bool HealSingleAbility(IAction nextGCD, out IAction act)
        {
            // Cards
            if (TheArrowPvE.CanUse(out act) && TheArrowPvE.Target.Target?.GetHealthRatio() <= ArrowUse) return true;
            if (TheEwerPvE.CanUse(out act) && TheEwerPvE.Target.Target?.GetHealthRatio() <= EwerUse) return true;
            // oGCD
            if (EssentialDignityPvE.CanUse(out act) && EssentialDignityPvE.Target.Target?.GetHealthRatio() <= EssDigHeal) return true;
            if (CelestialIntersectionPvE.CanUse(out act)) return true;
            return base.HealSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.MicrocosmosPvE, ActionID.LadyOfCrownsPvE, ActionID.StellarDetonationPvE, ActionID.CelestialOppositionPvE, ActionID.HoroscopePvE)]
        protected override bool HealAreaAbility(IAction nextGCD, out IAction act)
        {
            // Priority
            if (MicrocosmosPvE.CanUse(out act)) return true; // Want to use this first if Macrocosmos was used
            if (LadyOfCrownsPvE.CanUse(out act)) return true;
            // Lv. (Desc)
            if (HoroscopePvE.CanUse(out act)) return true;
            if (StellarDetonationPvE.CanUse(out act)) return true;
            if (CelestialOppositionPvE.CanUse(out act)) return true;
            return base.HealAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.AspectedBeneficPvE, ActionID.BeneficIiPvE, ActionID.BeneficPvE)]
        protected override bool HealSingleGCD(out IAction act)
        {
            if (AspectedBeneficPvE.CanUse(out act)
                && (IsMoving || AspectedBeneficPvE.Target.Target?.GetHealthRatio() <= AspectedBeneHeal)) return true;
            if (BeneficIiPvE.CanUse(out act)) return true;
            if (BeneficPvE.CanUse(out act)) return true;
            return base.HealSingleGCD(out act);
        }
        [RotationDesc(ActionID.AspectedHeliosPvE, ActionID.HeliosPvE)]
        protected override bool HealAreaGCD(out IAction act)
        {
            if (AspectedHeliosPvE.CanUse(out act)) return true;
            if (HeliosPvE.CanUse(out act)) return true;
            return base.HealAreaGCD(out act);
        }
        #endregion

        #region GCD Logic
        protected override bool GeneralGCD(out IAction act)
        {
            if (IsLastAction(true, NeutralSectPvE, HoroscopePvE))
            {
                AspectedHeliosPvE.CanUse(out act);
                return true;
            }

            if (GravityPvE.CanUse(out act)) return true;

            if (CombustPvE.CanUse(out act)) return true;
            if (MaleficPvE.CanUse(out act)) return true;
            if (CombustPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

            return base.GeneralGCD(out act);
        }


        #endregion

        #region oGCD Logic
        protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
        {
            // Horoscope Helios Usage (Requires Horoscope to be used already)
            if (PartyMembersAverHP <= HoroHelios)
            {
                if (HoroscopePvE_16558.CanUse(out act)) return true;
            }
            // Use SunSign if losing effect soon
            if (Player.HasStatus(true, StatusID.Suntouched) && Player.WillStatusEndGCD(gcdCount: 3))
            {
                SunSignPvE.CanUse(out act);
                return true;
            }
            // Draw cards
            if (AstralDrawPvE.CanUse(out act) && !TheBalancePvE.CanUse(out _)) return true;
            if (UmbralDrawPvE.CanUse(out act) && !TheSpearPvE.CanUse(out _)) return true;
            // Support cards used before next draw
            if (TheArrowPvE.CanUse(out act) && UmbralDrawPvE.Cooldown.WillHaveOneChargeGCD(gcdCount: 3)) return true;
            if (TheSpirePvE.CanUse(out act) && UmbralDrawPvE.Cooldown.WillHaveOneChargeGCD(gcdCount: 3)) return true;
            if (TheEwerPvE.CanUse(out act) && AstralDrawPvE.Cooldown.WillHaveOneChargeGCD(gcdCount: 3)) return true;
            if (TheBolePvE.CanUse(out act) && AstralDrawPvE.Cooldown.WillHaveOneChargeGCD(gcdCount: 3)) return true;

            return base.GeneralAbility(nextGCD, out act);
        }

        protected override bool AttackAbility(IAction nextGCD, out IAction? act)
        {
            if (IsBurst && DivinationPvE.CanUse(out act)) return true;
            if (OraclePvE.CanUse(out act)) return true;

            if (IsMoving && LightspeedPvE.CanUse(out act)) return true;

            if (!IsMoving && (NumberOfAllHostilesInRange < 2))
            {
                if (!Player.HasStatus(true, StatusID.EarthlyDominance, StatusID.GiantDominance))
                {
                    if (EarthlyStarPvE.CanUse(out act)) return true;
                }
            }

            if (HostileTarget.IsBossFromIcon())
            {
                if (LordOfCrownsPvE.CanUse(out act)) return true;
            }

            if (LordOfCrownsPvE.CanUse(out act) && LordOfCrownsPvE.Target.AffectedTargets?.Length > 1) return true;

            return base.AttackAbility(nextGCD, out act);
        }

        #endregion

        #region Extra
        // Modify the type of Medicine, default is the most appropriate Medicine, generally do not need to modify
        public override MedicineType MedicineType => base.MedicineType;
        #endregion
    }
}
