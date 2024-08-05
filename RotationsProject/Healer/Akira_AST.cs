namespace RotationsProject.Healer
{
    [Rotation("Akira_AST", CombatType.PvE, Description = "Akira's rotation for AST v0.4", GameVersion = "7.05")]
    [SourceCode(Path = "main/RotationsProject/Healer/Akira_AST.cs")]
    [Api(3)]
    public class Akira_AST : AstrologianRotation
    {
        #region Config Options
        [Range(0, 1, ConfigUnitType.Percent)]
        [RotationConfig(CombatType.PvE, Name = "Minimum HP threshold party member need for Aspected Benefic")]
        public float AspectedBeneHeal { get; set; } = 0.5f;

        [Range(0, 1, ConfigUnitType.Percent)]
        [RotationConfig(CombatType.PvE, Name = "HP Percent for using Essential Dignity")]
        public float EssDigHeal { get; set; } = 0.4f;
        #endregion

        #region Emergency Action
        protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
        {
            if (base.EmergencyAbility(nextGCD, out act)) return true;

            if (!InCombat) return false;

            if (PartyMembersAverHP < 0.85f)
            {
                if (nextGCD.IsTheSameTo(true, AspectedHeliosPvE, HeliosPvE))
                {
                    if (HoroscopePvE.CanUse(out act)) return true;
                    if (NeutralSectPvE.CanUse(out act)) return true;
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
            if (remainTime is < 4 and > 3 && AspectedBeneficPvE.CanUse(out act)) return act;
            if (remainTime < 30 && AstralDrawPvE.CanUse(out act)) return act;

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
            if (InCombat && ExaltationPvE.CanUse(out act)) return true;
            if (CelestialIntersectionPvE.CanUse(out act)) return true;
            return base.DefenseSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.SunSignPvE, ActionID.CollectiveUnconsciousPvE)]
        protected override bool DefenseAreaAbility(IAction nextGCD, out IAction act)
        {
            if (InCombat && SunSignPvE.CanUse(out act)) return true; // Sun Sign should be prioritized when available
            if (InCombat && CollectiveUnconsciousPvE.CanUse(out act)) return true;
            return base.DefenseAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.MacrocosmosPvE)]
        protected override bool DefenseAreaGCD(out IAction act)
        {
            act = null;
            if(Player.HasStatus(true, StatusID.WheelOfFortune))
            {
                return false;
            }
            else
            {
                if (MacrocosmosPvE.CanUse(out act)) return true;
            }
            return base.DefenseAreaGCD(out act);
        }
        #endregion

        #region Heal Logic
        [RotationDesc(ActionID.TheArrowPvE, ActionID.TheEwerPvE, ActionID.EssentialDignityPvE)]
        protected override bool HealSingleAbility(IAction nextGCD, out IAction act)
        {
            // Cards
            if (TheArrowPvE.CanUse(out act) && InCombat) return true;
            if (TheEwerPvE.CanUse(out act) && InCombat) return true;
            // oGCD
            if (EssentialDignityPvE.CanUse(out act) && EssentialDignityPvE.Target.Target.GetHealthRatio() < EssDigHeal) return true;
            return base.HealSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.MicrocosmosPvE, ActionID.LadyOfCrownsPvE, ActionID.CelestialOppositionPvE, ActionID.HoroscopePvE)]
        protected override bool HealAreaAbility(IAction nextGCD, out IAction act)
        {
            if (MicrocosmosPvE.CanUse(out act)) return true; // Want to use this first if Macrocosmos was used
            if (LadyOfCrownsPvE.CanUse(out act)) return true;
            if (CelestialOppositionPvE.CanUse(out act)) return true;
            if (HoroscopePvE.CanUse(out act)) return true;
            return base.HealAreaAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.AspectedBeneficPvE, ActionID.BeneficIiPvE, ActionID.BeneficPvE)]
        protected override bool HealSingleGCD(out IAction act)
        {
            if (AspectedBeneficPvE.CanUse(out act)
                && (IsMoving || AspectedBeneficPvE.Target.Target?.GetHealthRatio() < AspectedBeneHeal)) return true;
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
            if (AstralDrawPvE.CanUse(out act)) return true;
            if (UmbralDrawPvE.CanUse(out act)) return true;
            // Buff cards
            if (InCombat && TheBalancePvE.CanUse(out act) && !(TheBalancePvE.Target.Target.HasStatus(false, StatusID.Weakness))) return true;
            if (InCombat && TheSpearPvE.CanUse(out act) && !(TheSpearPvE.Target.Target.HasStatus(false, StatusID.Weakness))) return true;
            // Support cards used before next draw
            if (TheArrowPvE.CanUse(out act) && UmbralDrawPvE.Cooldown.WillHaveOneCharge(12)) return true;
            if (TheSpirePvE.CanUse(out act) && UmbralDrawPvE.Cooldown.WillHaveOneCharge(12)) return true;
            if (TheEwerPvE.CanUse(out act) && AstralDrawPvE.Cooldown.WillHaveOneCharge(12)) return true;
            if (TheBolePvE.CanUse(out act) && AstralDrawPvE.Cooldown.WillHaveOneCharge(12)) return true;

            return base.GeneralAbility(nextGCD, out act);
        }

        protected override bool AttackAbility(IAction nextGCD, out IAction? act)
        {
            if (IsBurst && DivinationPvE.CanUse(out act)) return true;

            if (AstralDrawPvE.CanUse(out act, usedUp: IsBurst)) return true;

            if (InCombat)
            {
                if (IsMoving && LightspeedPvE.CanUse(out act)) return true;

                if (!IsMoving && (NumberOfAllHostilesInRange < 2))
                {
                    if (!Player.HasStatus(true, StatusID.EarthlyDominance, StatusID.GiantDominance))
                    {
                        if (EarthlyStarPvE.CanUse(out act)) return true;
                    }
                }

                {
                    if (LordOfCrownsPvE.CanUse(out act)) return true;
                }
            }

            if (OraclePvE.CanUse(out act)) return true;
            return base.AttackAbility(nextGCD, out act);
        }

        #endregion

        #region Extra
        // Modify the type of Medicine, default is the most appropriate Medicine, generally do not need to modify
        public override MedicineType MedicineType => base.MedicineType;

        //This is the method to update all field you wrote, it is used first during one frame.
        protected override void UpdateInfo()
        {
            base.UpdateInfo();
        }

        //This method is used when player change the terriroty, such as go into one duty, you can use it to set the field.
        public override void OnTerritoryChanged()
        {
            base.OnTerritoryChanged();
        }

        //This method is used to debug. If you want to show some information in Debug panel, show something here.
        public override void DisplayStatus()
        {
            base.DisplayStatus();
        }

        // Modify this bool to display your DisplayStatus on the Formal Page.
        public override bool ShowStatus => base.ShowStatus;
        #endregion
    }
}
