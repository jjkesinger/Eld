using Eld.Core.Extensions;
using Eld.Core.Model;
using Itenso.TimePeriod;
using System;
using System.Linq;

namespace Eld.Audit.Model
{
    public class DutyStatusEvent
    {
        public DutyStatusEvent(string driverId, DateTimeOffset startDateTime, DateTimeOffset endDateTime, Enums.DutyStatus status, Enums.Jurisdiction jurisdiction)
        {
            DriverId = driverId;
            DutyStatusJursidiction = GetJurisdiction(jurisdiction);
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            DutyStatus = status;
        }

        public string DriverId { get; private set; }
        public DateTimeOffset StartDateTime { get; private set; }
        public DateTimeOffset EndDateTime { get; private set; }
        public Enums.DutyStatus DutyStatus { get; private set; }
        internal Jurisdiction DutyStatusJursidiction { get; private set; }

        public ITimePeriod TimeRange => new TimeRange(StartDateTime.DateTime, EndDateTime.DateTime);

        public TimeSpan Duration => EndDateTime.Subtract(StartDateTime);
        public TimeSpan DayDuration => EndDateTime.Date == StartDateTime.Date ? Duration :
            EndDateTime.Date > StartDateTime.Date ? StartDateTime.Date.AddDays(1).Subtract(StartDateTime.DateTime) :
            EndDateTime.Subtract(EndDateTime.DateTime); //Duration of status on same day of StartDateTime
        public bool IsQualifyingDailyReset => Duration >= DutyStatusJursidiction.DailyResetHours.HoursToTimeSpan() && (DutyStatus == Enums.DutyStatus.OffDuty || DutyStatus == Enums.DutyStatus.Sleeper);
        public bool IsQualifyingWeeklyReset => Duration >= DutyStatusJursidiction.WeeklyResetHours.HoursToTimeSpan() && (DutyStatus == Enums.DutyStatus.OffDuty || DutyStatus == Enums.DutyStatus.Sleeper);
        public bool IsQualifyingRestPeriod => (Duration >= DutyStatusJursidiction.MinimumRestLengthHours.HoursToTimeSpan() && (DutyStatus == Enums.DutyStatus.OffDuty || DutyStatus == Enums.DutyStatus.Sleeper));
        public bool IsMainSleeperSplit => (Duration >= DutyStatusJursidiction.MainSplitMinimumHours.HoursToTimeSpan() && Duration < DutyStatusJursidiction.DailyResetHours.HoursToTimeSpan()) &&
                ((DutyStatusJursidiction.SplitStatuses.Contains(DutyStatus) && !DutyStatusJursidiction.IsMainSplitRequiredInSleeperOnly) ||
                (DutyStatus == Enums.DutyStatus.Sleeper && DutyStatusJursidiction.IsMainSplitRequiredInSleeperOnly));

        public TimeSpan MaxShiftHoursBeforeRestRequiredTimespan => DutyStatusJursidiction.MaximumShiftHoursBeforeRestBreakRequired.HoursToTimeSpan();
        public TimeSpan MaxShiftHoursForRestBreakExemption => DutyStatusJursidiction.MaximumShiftHoursForRestExemption.HoursToTimeSpan();
        public TimeSpan MaxOnDutyHoursAllowed => DutyStatusJursidiction.OnDutyHoursAllowed.HoursToTimeSpan();
        public TimeSpan MaxDrivingHoursAllowed => DutyStatusJursidiction.DrivingHoursAllowed.HoursToTimeSpan();
        public TimeSpan MaxWeeklyHoursAllowedTimeSpan => DutyStatusJursidiction.WeeklyOnDutyHoursAllowed.HoursToTimeSpan();
        public TimeSpan WeeklyAuditDaysTimeSpan => DutyStatusJursidiction.WeeklyAuditTimeRangeDays.DaysToTimeSpan();

        public bool IsRestBreakExempt => DutyStatusJursidiction.IsRestBreakExempt;

        public bool PossibleSplitSleeper => Duration >= DutyStatusJursidiction.SplitMinimumHours.HoursToTimeSpan() && Duration < DutyStatusJursidiction.DailyResetHours.HoursToTimeSpan() &&
            ((DutyStatusJursidiction.SplitStatuses.Contains(DutyStatus) && !DutyStatusJursidiction.IsMainSplitRequiredInSleeperOnly) ||
                (DutyStatus == Enums.DutyStatus.Sleeper && DutyStatusJursidiction.IsMainSplitRequiredInSleeperOnly));
        public bool IsOffDutyStatus => new[] { Enums.DutyStatus.OffDuty, Enums.DutyStatus.Sleeper }.Contains(DutyStatus);
        public bool IsTexasSleeperExempt => DutyStatusJursidiction.JurisdictionId == Enums.Jurisdiction.TxIntrastate;

        /// <summary>
        /// Combines events based on if their the same Jurisdiction and (Same duty status or both are considered off duty status)
        /// </summary>
        /// <param name="statusEvent">Status event to combine</param>
        /// <returns>True if combined, False if not</returns>
        public bool CombineDutyStatusEvents(DutyStatusEvent statusEvent)
        {
            if (statusEvent.DutyStatusJursidiction.JurisdictionId == DutyStatusJursidiction.JurisdictionId && (statusEvent.DutyStatus == DutyStatus || statusEvent.IsOffDutyStatus && IsOffDutyStatus))
            {
                if (!TimeRange.OverlapsWith(statusEvent.TimeRange) && TimeRange.IntersectsWith(statusEvent.TimeRange))
                {
                    if (StartDateTime > statusEvent.StartDateTime)
                    {
                        StartDateTime = statusEvent.StartDateTime;
                        return true;
                    }
                    else
                    {
                        EndDateTime = statusEvent.EndDateTime;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a Jurisdiciton based on Enum value
        /// </summary>
        /// <param name="jurisdiction">Enum of Jurisdiction</param>
        /// <returns>Jurisdiction Object populated with Correct Values</returns>
        private Jurisdiction GetJurisdiction(Enums.Jurisdiction jurisdiction)
        {
            return new Jurisdiction(jurisdiction);
        }
    }
}
