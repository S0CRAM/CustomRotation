using System.Linq;

namespace RotationsProject.Healer
{
    [Rotation("Akira_SCH", CombatType.PvE, Description = "Akira's SCH v0.1", GameVersion = "7.05")]
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
        protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
        {
            if (nextGCD.IsTheSameTo(true, SuccorPvE, AdloquiumPvE))
            {
                if (RecitationPvE.CanUse(out act)) return true;
            }
            return base.EmergencyAbility(nextGCD, out act);
        }
        #endregion

        #region Defense Logic
        protected override bool DefenseSingleAbility(IAction nextGCD, out IAction act)
        {
            if (ExcogitationPvE.CanUse(out act)) return true;
            return base.DefenseSingleAbility(nextGCD, out act);
        }
        [RotationDesc(ActionID.AdloquiumPvE)]
        protected override bool DefenseSingleGCD(out IAction act)
        {
            if (AdloquiumPvE.CanUse(out act)) return true;
            return base.DefenseSingleGCD(out act);
        }
        protected override bool DefenseAreaAbility(IAction nextGCD, out IAction act)
        {
            if (FeyIlluminationPvE_16538.CanUse(out act)) return true;
            if (SacredSoilPvE.CanUse(out act)) return true;

            return base.DefenseAreaAbility(nextGCD, out act);
        }
        protected override bool DefenseAreaGCD(out IAction act)
        {
            if (SuccorPvE.CanUse(out act)) return true;
            return base.DefenseAreaGCD(out act);
        }
        #endregion

        #region Heal Logic
        protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
        {
            var haveLink = PartyMembers.Any(p => p.HasStatus(true, StatusID.FeyUnion_1223));

            if (AetherpactPvE.CanUse(out act) && FairyGauge >= 70 && !haveLink) return true;
            if (ProtractionPvE.CanUse(out act)) return true;
            if (LustratePvE.CanUse(out act)) return true;
            if (AetherpactPvE.CanUse(out act) && !haveLink) return true;

            return base.HealSingleAbility(nextGCD, out act);
        }
        protected override bool HealSingleGCD(out IAction act)
        {
            /*if (AdloquiumPvE.Target.Target.HasStatus(true, StatusID.Galvanize))
            {
                if (AdloquiumPvE.CanUse(out act)) return true;
            }*/
            if (PhysickPvE.CanUse(out act)) return true;
            return base.HealSingleGCD(out act);
        }
        #endregion

        #region GCD Logic

        #endregion

        #region oGCD Logic

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
