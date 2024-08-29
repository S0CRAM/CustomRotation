using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Linq;

namespace RotationsProject.Healer
{
    [Rotation("Akira_SCH", CombatType.PvE, Description = "Akira's SCH v0.3", GameVersion = "7.05")]
    [SourceCode(Path = "main/RotationsProject/Healer/Akira_SCH.cs")]
    [Api(3)]
    public class Akira_SCH : ScholarRotation
    {
        #region Config Options

        #endregion

        #region Countdown Logic
        protected override IAction? CountDownAction(float remainTime)
        {
            if (remainTime < RuinPvE.Info.CastTime + CountDownAhead && RuinPvE.CanUse(out var act)) return act;
            foreach (var item in PartyMembers)
            {
                if (item.IsJobCategory(JobRole.Tank) || item.IsJobCategory(JobRole.Healer))
                {
                    return AdloquiumPvE;
                }
            }
            return base.CountDownAction(remainTime);
        }
        #endregion

        #region Emergency Action

        // TODO: Somehow implement Recitation - Adlo - Deployment Tactics as a form of defense
        //       However, because of how long it takes, we may use a swiftcast?
        protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
        {
            if (nextGCD.IsTheSameTo(true, AdloquiumPvE, ExcogitationPvE, IndomitabilityPvE))
            {
                if (RecitationPvE.CanUse(out act)) return true;
            }

            // Aetherpact removal
            foreach (var i in PartyMembers)
            {
                if (i.HasStatus(true, StatusID.FeyUnion_1223))
                {
                    if (i.GetHealthRatio() < 0.99) continue;
                    else
                    {
                        act = AetherpactPvE;
                        return true;
                    }
                }
            }

            return base.EmergencyAbility(nextGCD, out act);
        }
        #endregion

        #region Defense Logic
        [RotationDesc(ActionID.SacredSoilPvE, ActionID.FeyIlluminationPvE, ActionID.ExpedientPvE)]
        protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
        {
            // Fae moves check for Dissipation 
            if (!Player.HasStatus(true, StatusID.Dissipation))
            {
                if (SeraphismPvE.CanUse(out act)) return true;
                if (SummonSeraphPvE.CanUse(out act)) return true;
                if (FeyIlluminationPvE_16538.CanUse(out act)) return true;
            }
            if (DeploymentTacticsPvE.CanUse(out act)) return true;
            if (SacredSoilPvE.CanUse(out act)) return true;


            return base.DefenseAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.SuccorPvE)]
        protected override bool DefenseAreaGCD(out IAction act)
        {
            if (SuccorPvE.CanUse(out act)) return true;
            return base.DefenseAreaGCD(out act);
        }
        [RotationDesc(ActionID.ProtractionPvE, ActionID.ExcogitationPvE)]
        protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
        {
            // idk, SCH doesnt really have a defense single ability
            if (ProtractionPvE.CanUse(out act)) return true;
            if (ExcogitationPvE.CanUse(out act)) return true;

            return base.DefenseSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.AdloquiumPvE)]
        protected override bool DefenseSingleGCD(out IAction act)
        {
            if (AdloquiumPvE.CanUse(out act)) return true;

            return base.DefenseSingleGCD(out act);
        }
        #endregion

        #region Heal Logic
        [RotationDesc(ActionID.ConsolationPvE, ActionID.FeyBlessingPvE, ActionID.IndomitabilityPvE)]
        protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
        {
            if (ConsolationPvE.CanUse(out act)) return true; // Want to use when available
            if (FeyBlessingPvE.CanUse(out act) && !Player.HasStatus(true, StatusID.Dissipation)) return true;
            if (IndomitabilityPvE.CanUse(out act)) return true;

            return base.HealAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.SuccorPvE)]
        protected override bool HealAreaGCD(out IAction act)
        {
            // if (AccessionPvE.CanUse(out act)) return true;
            if (SuccorPvE.CanUse(out _))
            {
                if (EmergencyTacticsPvE.CanUse(out act)) return true;
                act = SuccorPvE;
                return true;
            }
            if (SuccorPvE.CanUse(out act)) return true;
            return base.HealAreaGCD(out act);
        }
        [RotationDesc(ActionID.AetherpactPvE, ActionID.LustratePvE, ActionID.ExcogitationPvE)]
        protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
        {
            var linkedPlayer = PartyMembers.Any(p => p.HasStatus(true, StatusID.FeyUnion_1223));

            if (AetherpactPvE.CanUse(out act) && FairyGauge >= 70 && !linkedPlayer && !Player.HasStatus(true, StatusID.Dissipation)) return true;
            if (LustratePvE.CanUse(out act)) return true;
            if (ExcogitationPvE.CanUse(out act) && ExcogitationPvE.Target.Target?.GetHealthRatio() < 0.5) return true; // Yummy 800 pot heal
            if (AetherpactPvE.CanUse(out act) && !linkedPlayer && !Player.HasStatus(true, StatusID.Dissipation) &&
                AetherpactPvE.Target.Target?.GetHealthRatio() <= 0.9) return true;

            return base.HealSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.AdloquiumPvE, ActionID.PhysickPvE)]
        protected override bool HealSingleGCD(out IAction act)
        {
            //if (ManifestationPvE.CanUse(out act)) return true;
            if (AdloquiumPvE.CanUse(out _))
            {
                if (EmergencyTacticsPvE.CanUse(out act)) return true;
                act = AdloquiumPvE;
                return true;
            }
            if (AdloquiumPvE.CanUse(out act) && !AdloquiumPvE.Target.Target.HasStatus(true, StatusID.Galvanize))
            if (PhysickPvE.CanUse(out act)) return true;

            return base.HealSingleGCD(out act);
        }
        #endregion

        #region GCD Logic
        protected override bool GeneralGCD(out IAction act)
        {
            if (SummonEosPvE.CanUse(out act)) return true;

            // DoT
            if (BioPvE.CanUse(out act)) return true;

            // AoE
            if (ArtOfWarPvE.CanUse(out act) && ArtOfWarPvE.Target.AffectedTargets.Count() > 2) return true;

            // Spellcast ST
            if (RuinPvE.CanUse(out act)) return true;

            // Instant ST
            if (RuinIiPvE.CanUse(out act)) return true;

            return base.GeneralGCD(out act);
        }

        // TODO: Allow option for EnergyDrain to only be used in burst
        protected override bool AttackAbility(IAction nextGCD, out IAction? act)
        {
            if (IsBurst && HostileTarget.IsBossFromIcon())
            {
                if (ChainStratagemPvE.CanUse(out act)) return true;
                if (BanefulImpactionPvE.CanUse(out act,skipAoeCheck: true)) return true;
            }

            if (AetherflowPvE.Cooldown.WillHaveOneChargeGCD(3))
            {
                if (EnergyDrainPvE.CanUse(out act)) return true;
            }

            // In here because of how low the potency is
            if (InCombat && WhisperingDawnPvE_16537.CanUse(out act) &&
                PartyMembersAverHP <= 0.95 && !Player.HasStatus(true, StatusID.Dissipation)) return true;

            // Aetherflow management
            if (AetherflowPvE.CanUse(out act) && !HasAetherflow) return true;
            if (DissipationPvE.CanUse(out act) && !HasAetherflow && AetherflowPvE.IsInCooldown) return true;
            return base.AttackAbility(nextGCD, out act);
        }
        #endregion

        #region oGCD Logic
        [RotationDesc(ActionID.ExpedientPvE)]
        protected override bool SpeedAbility(IAction nextGCD, out IAction? act)
        {
            if (ExpedientPvE.CanUse(out act)) return true;

            return base.SpeedAbility(nextGCD, out act);
        }
        #endregion

        #region Extra
        // Modify the type of Medicine, default is the most appropriate Medicine, generally do not need to modify
        public override MedicineType MedicineType => base.MedicineType;
        #endregion
    }
}
