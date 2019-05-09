using Eld.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eld.Audit.Model
{
    internal class Jurisdiction
    {
        internal Jurisdiction(Enums.Jurisdiction jursidiction)
        {
            JurisdictionId = jursidiction;
            switch (jursidiction)
            {
                case Enums.Jurisdiction.Us70:
                    OnDutyHoursAllowed = 14;
                    DrivingHoursAllowed = 11;
                    WeeklyOnDutyHoursAllowed = 70;
                    DailyResetHours = 10;
                    WeeklyResetHours = 34;
                    WeeklyAuditTimeRangeDays = 8;
                    MinimumRestLengthHours = 0.5;
                    MaximumShiftHoursForRestExemption = 12;
                    MaximumShiftHoursBeforeRestBreakRequired = 8;
                    IsRestBreakExempt = false;
                    //SplitStatuses = use default
                    SplitMinimumHours = 2;
                    MainSplitMinimumHours = 8;
                    SplitTotalForReset = 10;
                    IsMainSplitRequiredInSleeperOnly = true;
                    NumberOfSplits = 2;
                    break;

                case Enums.Jurisdiction.Us60:
                    OnDutyHoursAllowed = 14;
                    DrivingHoursAllowed = 11;
                    WeeklyOnDutyHoursAllowed = 60;
                    DailyResetHours = 10;
                    WeeklyResetHours = 34;
                    WeeklyAuditTimeRangeDays = 7;
                    MinimumRestLengthHours = 0.5;
                    MaximumShiftHoursForRestExemption = 12;
                    MaximumShiftHoursBeforeRestBreakRequired = 8;
                    IsRestBreakExempt = false;
                    //SplitStatuses = use default
                    SplitMinimumHours = 2;
                    MainSplitMinimumHours = 8;
                    SplitTotalForReset = 10;
                    IsMainSplitRequiredInSleeperOnly = true;
                    NumberOfSplits = 2;
                    break;

                case Enums.Jurisdiction.TxIntrastate:
                    OnDutyHoursAllowed = 15;
                    DrivingHoursAllowed = 12;
                    WeeklyOnDutyHoursAllowed = 70;
                    DailyResetHours = 8;
                    WeeklyResetHours = 34;
                    WeeklyAuditTimeRangeDays = 8;
                    MaximumShiftHoursForRestExemption = 12;
                    MaximumShiftHoursBeforeRestBreakRequired = 8;
                    IsRestBreakExempt = true;
                    //SplitStatuses = use default
                    SplitMinimumHours = 2;
                    MainSplitMinimumHours = 6;
                    SplitTotalForReset = 10;
                    IsMainSplitRequiredInSleeperOnly = true;
                    NumberOfSplits = 2;
                    break;

                case Enums.Jurisdiction.Passenger70:
                    OnDutyHoursAllowed = 15;
                    DrivingHoursAllowed = 12;
                    WeeklyOnDutyHoursAllowed = 70;
                    DailyResetHours = 8;
                    WeeklyResetHours = 34;
                    WeeklyAuditTimeRangeDays = 8;
                    MaximumShiftHoursForRestExemption = 12;
                    MaximumShiftHoursBeforeRestBreakRequired = 8;
                    IsRestBreakExempt = true;

                    SplitStatuses = new[] { Enums.DutyStatus.Sleeper }.ToList();
                    SplitMinimumHours = 2;
                    MainSplitMinimumHours = 6;
                    SplitTotalForReset = 10;
                    IsMainSplitRequiredInSleeperOnly = true;
                    NumberOfSplits = 2;
                    break;

                case Enums.Jurisdiction.Passenger60:
                    OnDutyHoursAllowed = 15;
                    DrivingHoursAllowed = 12;
                    WeeklyOnDutyHoursAllowed = 60;
                    DailyResetHours = 8;
                    WeeklyResetHours = 34;
                    WeeklyAuditTimeRangeDays = 7;
                    MaximumShiftHoursForRestExemption = 12;
                    MaximumShiftHoursBeforeRestBreakRequired = 8;
                    IsRestBreakExempt = true;

                    SplitStatuses = new[] { Enums.DutyStatus.Sleeper }.ToList();
                    SplitMinimumHours = 2;
                    MainSplitMinimumHours = 6;
                    SplitTotalForReset = 10;
                    IsMainSplitRequiredInSleeperOnly = true;
                    NumberOfSplits = 2;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        internal protected Enums.Jurisdiction JurisdictionId { get; private set; }
        internal protected int DrivingHoursAllowed { get; private set; }
        internal protected int OnDutyHoursAllowed { get; private set; }
        internal protected int WeeklyOnDutyHoursAllowed { get; private set; }
        internal protected int DailyResetHours { get; private set; }
        internal protected int WeeklyResetHours { get; private set; }
        internal protected int WeeklyAuditTimeRangeDays { get; private set; }
        internal protected double MinimumRestLengthHours { get; private set; }
        internal protected int MaximumShiftHoursForRestExemption { get; private set; } //Max amount of hours you can work with an exemption from rest break
        internal protected int MaximumShiftHoursBeforeRestBreakRequired { get; private set; }
        internal protected bool IsRestBreakExempt { get; private set; }
        internal protected List<Enums.DutyStatus> SplitStatuses { get; private set; } = new[] { Enums.DutyStatus.OffDuty, Enums.DutyStatus.Sleeper }.ToList();
        internal protected int SplitMinimumHours { get; private set; }
        internal protected int MainSplitMinimumHours { get; private set; }
        internal protected int SplitTotalForReset { get; private set; }
        internal protected bool IsMainSplitRequiredInSleeperOnly { get; private set; }
        internal protected int NumberOfSplits { get; private set; }
    }
}
