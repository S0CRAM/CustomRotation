using System.Linq;
namespace RotationsProject.Healer
{
    [Rotation("Akira_SGE", CombatType.PvE, Description = "Akira's rotation for SGE v0.18", GameVersion = "7.05")]
    [SourceCode(Path = "main/RotationsProject/Healer/Akira_SGE.cs")]
    [Api(3)]
    public class Akira_SGE : SageRotation
    {
        #region Config options

        #endregion

        #region Emergency
        protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
        {
            if (base.EmergencyAbility(nextGCD, out act)) return true;

            if (nextGCD.IsTheSameTo(false, PneumaPvE, EukrasianDiagnosisPvE,
                EukrasianPrognosisPvE, EukrasianPrognosisIiPvE, DiagnosisPvE, PrognosisPvE) && (PartyMembersAverHP < 0.7f || PartyMembersMinHP < 0.6f))
            {
                if (ZoePvE.CanUse(out act)) return true;
            }

            if (nextGCD.IsTheSameTo(false, PneumaPvE, EukrasianDiagnosisPvE, DiagnosisPvE))
            {
                if (KrasisPvE.CanUse(out act)) return true;
            }

            if (nextGCD.IsTheSameTo(false, PneumaPvE, EukrasianDiagnosisPvE,
                 EukrasianPrognosisPvE, EukrasianPrognosisIiPvE, DiagnosisPvE, PrognosisPvE))
            {
                if (PhilosophiaPvE.CanUse(out act)) return true;
            }

            return base.EmergencyAbility(nextGCD, out act);
        }
        #endregion

        #region oGCD Defense + Healing
        [RotationDesc(ActionID.PanhaimaPvE, ActionID.KeracholePvE)]
        protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
        {
            if (Addersgall <= 1)
            {
                if (PanhaimaPvE.CanUse(out act)) return true; // Use when Addersgall is low
            }
            if (KeracholePvE.CanUse(out act)) return true;
            return base.DefenseAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.HaimaPvE)]
        protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
        {
            if (HaimaPvE.CanUse(out act)) return true;
            return base.DefenseSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.PhysisPvE, ActionID.IxocholePvE)]
        protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
        {
            if (IxocholePvE.CanUse(out act)) return true;
            if (HolosPvE.CanUse(out act)) return true;
            if (PhysisPvE.CanUse(out act)) return true; // HoT AoE skill & heal potency+
            return base.HealAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.TaurocholePvE, ActionID.DruocholePvE)]
        protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
        {
            if (TaurocholePvE.CanUse(out act) && TaurocholePvE.Target.Target?.GetHealthRatio() < 0.7f) return true;
            if (DruocholePvE.CanUse(out act) && DruocholePvE.Target.Target?.GetHealthRatio() < 0.85f) return true;
            return base.HealSingleAbility(nextGCD, out act);
        }
        #endregion
        #region GCD Defense + Heal
        [RotationDesc(ActionID.EukrasianPrognosisPvE)]
        protected override bool DefenseAreaGCD(out IAction? act)
        {
            if (EukrasianPrognosisPvE.CanUse(out act))
            {
                if (EukrasiaPvE.CanUse(out act)) return true;
                act = EukrasianPrognosisPvE;
                return true;
            }
            return base.DefenseAreaGCD(out act);
        }

        [RotationDesc(ActionID.EukrasianDiagnosisPvE)]
        protected override bool DefenseSingleGCD(out IAction? act)
        {
            if (EukrasianDiagnosisPvE.CanUse(out act))
            {
                if (EukrasiaPvE.CanUse(out act)) return true;
                act = EukrasianDiagnosisPvE;
                return true;
            }
            return base.DefenseSingleGCD(out act);
        }
        [RotationDesc(ActionID.PneumaPvE, ActionID.PrognosisPvE)]
        protected override bool HealAreaGCD(out IAction? act)
        {
            if (PneumaPvE.CanUse(out act) && PartyMembersAverHP < 0.8) return true;
            if (PrognosisPvE.CanUse(out act)) return true;
            return base.HealAreaGCD(out act);
        }
        [RotationDesc(ActionID.DiagnosisPvE)]
        protected override bool HealSingleGCD(out IAction? act)
        {
            if (DiagnosisPvE.CanUse(out act)) return true;
            return base.HealSingleGCD(out act);
        }
        #endregion
        #region oGCD General Logic
        protected override bool AttackAbility(IAction nextGCD, out IAction? act)
        {
            if (PsychePvE.CanUse(out act)) return true; // lol, lmao
            return base.AttackAbility(nextGCD, out act);
        }
        protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
        {
            if (KardiaPvE.CanUse(out act)) return true;
            if (Addersgall.Equals(0) && RhizomataPvE.CanUse(out act)) return true;
            if (SoteriaPvE.CanUse(out act) && PartyMembers.Any(p => p.HasStatus(true, StatusID.Kardion) && p.GetHealthRatio() < 0.9f)) return true;
            if (PepsisPvE.CanUse(out act) && PartyMembers.All(p => !p.IsDead && p.GetHealthRatio() < 0.95f && p.HasStatus(true,
                StatusID.EukrasianDiagnosis,
                StatusID.EukrasianDiagnosis_2865,
                StatusID.EukrasianDiagnosis_3109,
                StatusID.EukrasianPrognosis,
                StatusID.EukrasianPrognosis_2866))) return true;
            return base.GeneralAbility(nextGCD, out act);
        }
        #endregion
        #region GCD Logic
        protected override bool GeneralGCD(out IAction? act)
        {
            // Keep Eukrasian Diagnosis on tank
            if (EukrasianDiagnosisPvE.CanUse(out _) && (EukrasianDiagnosisPvE.Target.Target?.IsJobCategory(JobRole.Tank) ?? false))
            {
                if (EukrasiaPvE.CanUse(out act)) return true;
                act = EukrasianDiagnosisPvE;
                return true;
            }
            // AoE DoT Attack
            if (EukrasianDyskrasiaPvE.EnoughLevel && (DyskrasiaPvE.Target.AffectedTargets?.Length > 2) && !(HostileTarget?.HasStatus(true,
                StatusID.EukrasianDosis,
                StatusID.EukrasianDosisIi,
                StatusID.EukrasianDosisIii,
                StatusID.EukrasianDyskrasia
                ) ?? true))
            {
                if (!HasEukrasia)
                {
                    if (EukrasiaPvE.CanUse(out act, skipCastingCheck: true)) return true;
                    act = DyskrasiaPvE;
                    return true;
                }
            }
            // Single DoT Attack
            if (EukrasianDosisPvE.EnoughLevel && !(HostileTarget?.HasStatus(true,
                StatusID.EukrasianDosis,
                StatusID.EukrasianDosisIi,
                StatusID.EukrasianDosisIii,
                StatusID.EukrasianDyskrasia
                ) ?? true))
            {
                if (!HasEukrasia)
                {
                    if (EukrasiaPvE.CanUse(out act, skipCastingCheck: true)) return true;
                    act = DosisPvE;
                    return true;
                }
            }
            // Normal Attacks
            if (PhlegmaPvE.CanUse(out act, usedUp: IsMoving)) return true;
            if (ToxikonPvE.CanUse(out act, usedUp: IsMoving)) return true;
            if (DyskrasiaPvE.CanUse(out act) && DyskrasiaPvE.Target.AffectedTargets?.Length > 2) return true;
            if (DosisPvE.CanUse(out act)) return true;

            return base.GeneralGCD(out act);
        }
        #endregion
    }
}
