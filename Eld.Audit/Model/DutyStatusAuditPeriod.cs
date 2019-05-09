using Eld.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eld.Audit.Model
{
    internal class DutyStatusAuditPeriod
    {
        internal DutyStatusAuditPeriod(string driverId, List<DutyStatusEvent> events)
        {
            DriverId = driverId;
            DutyStatusEvents = events.OrderBy(f => f.StartDateTime).ToList();
            Violations = new List<DutyStatusViolation>();
        }

        internal string DriverId { get; private set; }
        internal List<DutyStatusEvent> DutyStatusEvents { get; private set; }
        internal DateTimeOffset? LastWeeklyReset { get; private set; }
        internal DateTimeOffset? LastDailyReset { get; private set; }
        internal DateTimeOffset? LastRestBreak { get; private set; }
        internal DutyStatusEvent FirstSleeperSplit { get; private set; }
        internal DutyStatusEvent SecondSleeperSplit { get; private set; }
        internal int? RetroactiveIndex { get; private set; } //index of event to go back and process after split sleeper completion
        internal TimeSpan DurationOnDutySinceLastRestBreak { get; private set; }
        internal TimeSpan DurationDrivingSinceLastDailyReset { get; private set; }
        internal TimeSpan DurationOnDutySinceLastDailyReset { get; private set; }
        internal TimeSpan DurationOnDutySinceLastWeeklyReset { get; private set; }
        internal TimeSpan ViolationDurationOnDutySinceLastRestBreak { get; private set; }
        internal TimeSpan ViolationDurationDrivingSinceLastDailyReset { get; private set; }
        internal TimeSpan ViolationDurationOnDutySinceLastDailyReset { get; private set; }
        internal TimeSpan ViolationDurationOnDutySinceLastWeeklyReset { get; private set; }
        internal List<DutyStatusViolation> Violations { get; private set; }
        internal bool IsAlreadyInRestBreakViolation => ViolationDurationOnDutySinceLastRestBreak > TimeSpan.FromSeconds(0);
        internal bool IsAlreadyInOnDutyViolation => ViolationDurationOnDutySinceLastDailyReset > TimeSpan.FromSeconds(0);
        internal bool IsAlreadyInWeeklyViolation => ViolationDurationOnDutySinceLastWeeklyReset > TimeSpan.FromSeconds(0);
        internal bool IsAlreadyInDrivingViolation => ViolationDurationDrivingSinceLastDailyReset > TimeSpan.FromSeconds(0);
        internal List<DutyStatusEvent> SplitDutyStatusEvents => DutyStatusEvents.Where(f => f.PossibleSplitSleeper).ToList();
        internal DateTimeOffset StartDate => DutyStatusEvents.Min(f => f.StartDateTime);
        internal DateTimeOffset PeriodEndDate => DutyStatusEvents.Max(f => f.StartDateTime);
        internal DateTimeOffset PeriodFirstOnDutyDateTime => DutyStatusEvents.Where(f => f.IsOffDutyStatus == false).Any() ?
            DutyStatusEvents.Where(f => f.IsOffDutyStatus == false).Min(f => f.StartDateTime) : StartDate;


        /// <summary>
        /// Sets current split sleeper events for duty status event being processed
        /// </summary>
        /// <param name="firstSplit">First Split Sleeper Event that occurs</param>
        /// <param name="secondSplit">Second Split Sleeper Event that occurs</param>
        /// <param name="eventStartDateTime"></param>
        /// <param name="i"></param>
        internal void SetSplitSleeper(DutyStatusEvent firstSplit, DutyStatusEvent secondSplit, DateTimeOffset eventStartDateTime, int i)
        {
            FirstSleeperSplit = firstSplit;
            SecondSleeperSplit = secondSplit;
            RetroactiveIndex = firstSplit.EndDateTime == eventStartDateTime ? i : RetroactiveIndex;
        }

        /// <summary>
        /// Add duty status violation for event period
        /// </summary>
        /// <param name="startDateTime">Startdatetime of violation</param>
        /// <param name="endDateTime">End date/time of violation</param>
        /// <param name="hoursAllowed">Timespan of hours allowed for violation</param>
        /// <param name="violationType">Enum of violation type</param>
        /// <param name="dutyStatusJursidiction">Jursidiction of duty status event where violation occured</param>
        /// <param name="onDutyDuration">Total Duration of on duty/driving time</param>
        internal void AddViolation(DateTimeOffset startDateTime, DateTimeOffset endDateTime, TimeSpan hoursAllowed, Enums.ViolationType violationType, Jurisdiction dutyStatusJursidiction, TimeSpan? onDutyDuration = null)
        {
            bool isAlreadyInViolation = false;
            switch (violationType)
            {
                case Enums.ViolationType.Driving:
                    isAlreadyInViolation = IsAlreadyInDrivingViolation;
                    onDutyDuration = onDutyDuration ?? DurationDrivingSinceLastDailyReset;
                    break;
                case Enums.ViolationType.OnDuty:
                    isAlreadyInViolation = IsAlreadyInOnDutyViolation;
                    onDutyDuration = onDutyDuration ?? DurationOnDutySinceLastDailyReset;
                    break;
                case Enums.ViolationType.Rest:
                    isAlreadyInViolation = IsAlreadyInRestBreakViolation;
                    onDutyDuration = onDutyDuration ?? DurationOnDutySinceLastRestBreak;
                    break;
                case Enums.ViolationType.Weekly:
                    isAlreadyInViolation = IsAlreadyInWeeklyViolation;
                    onDutyDuration = onDutyDuration ?? DurationOnDutySinceLastWeeklyReset;
                    break;
            }

            if (isAlreadyInViolation || endDateTime.Subtract(onDutyDuration.Value - hoursAllowed) < startDateTime)
            {
                AddViolation(startDateTime, endDateTime, violationType, dutyStatusJursidiction);
            }
            else
            {
                AddViolation(endDateTime, onDutyDuration.Value, hoursAllowed, violationType, dutyStatusJursidiction);
            }
        }

        /// <summary>
        /// Sets weekly reset based on datetime
        /// </summary>
        /// <param name="dateTime">Date/time reset occurred</param>
        internal void SetWeeklyReset(DateTimeOffset dateTime)
        {
            LastWeeklyReset = dateTime;
            DurationOnDutySinceLastWeeklyReset = TimeSpan.FromSeconds(0);
            ViolationDurationOnDutySinceLastWeeklyReset = TimeSpan.FromSeconds(0);
            SetDailyReset(dateTime); //weekly reset will reset daily
        }

        /// <summary>
        /// Gets on duty time for the last X amount of days depending on jurisdiction
        /// </summary>
        /// <param name="statusDateTime">Date/time to calculate start</param>
        /// <param name="days">Number of days to go back</param>
        /// <returns></returns>
        internal TimeSpan OnDutyLastWeeklyDays(DateTimeOffset statusDateTime, double days)
        {
            return TimeSpan.FromSeconds(DutyStatusEvents.Where(f => f.EndDateTime >= statusDateTime.Date.AddDays(-days + 1) && f.EndDateTime <= statusDateTime && f.IsOffDutyStatus == false).Sum(f => f.DayDuration.TotalSeconds));
        }

        /// <summary>
        /// Sets daily reset based on datetime
        /// </summary>
        /// <param name="dateTime">DateTime daily reset occurred</param>
        internal void SetDailyReset(DateTimeOffset dateTime)
        {
            LastDailyReset = dateTime;
            DurationOnDutySinceLastDailyReset = TimeSpan.FromSeconds(0);
            DurationDrivingSinceLastDailyReset = TimeSpan.FromSeconds(0);
            ViolationDurationOnDutySinceLastDailyReset = TimeSpan.FromSeconds(0);
            ViolationDurationDrivingSinceLastDailyReset = TimeSpan.FromSeconds(0);

            SetRestReset(dateTime); //daily reset resets rest reset (say that 10x fast)
        }

        /// <summary>
        /// Gets the index of the event immediately after the 1st split sleeper to recalculate onduty, drive, and weekly time
        /// </summary>
        /// <returns>index of status immediately after 1st (minus one to account for i++ loop)</returns>
        internal int GetIndexToBackProcessTimeBetweenSplits()
        {
            SetDailyReset(FirstSleeperSplit.EndDateTime);

            //reset 70 to exclude main sleeper berth time (and time to reprocess)
            TimeSpan reprocessedDuration = SecondSleeperSplit.StartDateTime.Subtract(FirstSleeperSplit.EndDateTime);
            TimeSpan mainSleeperDuration = FirstSleeperSplit.IsMainSleeperSplit ? FirstSleeperSplit.Duration : SecondSleeperSplit.IsMainSleeperSplit ? SecondSleeperSplit.Duration : TimeSpan.FromDays(0);
            DurationOnDutySinceLastWeeklyReset = DurationOnDutySinceLastWeeklyReset.Subtract(mainSleeperDuration.Add(reprocessedDuration));

            int i = RetroactiveIndex.Value - 1;
            ResetSplitSleeper();
            return i;
        }

        /// <summary>
        /// Sets rest reset based on datetime
        /// </summary>
        /// <param name="dateTime">DateTime rest reset occurred</param>
        internal void SetRestReset(DateTimeOffset dateTime)
        {
            LastRestBreak = dateTime;
            DurationOnDutySinceLastRestBreak = TimeSpan.FromSeconds(0);
            ViolationDurationOnDutySinceLastRestBreak = TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Adds drive time for Drive duration calculations
        /// </summary>
        /// <param name="duration">Duration of drive time</param>
        internal void AddDriveTime(TimeSpan duration)
        {
            DurationDrivingSinceLastDailyReset = DurationDrivingSinceLastDailyReset.Add(duration);
            AddOnDutyTime(duration); //driving is onduty time
        }

        /// <summary>
        /// Adds onduty time for Onduty duration calculations
        /// </summary>
        /// <param name="duration">Duration of onduty time</param>
        internal void AddOnDutyTime(TimeSpan duration, bool includeWeekly = true)
        {
            if (includeWeekly)
            {
                DurationOnDutySinceLastWeeklyReset = DurationOnDutySinceLastWeeklyReset.Add(duration);
            }

            DurationOnDutySinceLastDailyReset = DurationOnDutySinceLastDailyReset.Add(duration);
            DurationOnDutySinceLastRestBreak = DurationOnDutySinceLastRestBreak.Add(duration);
        }


        private void ResetSplitSleeper()
        {
            FirstSleeperSplit = null;
            SecondSleeperSplit = null;
            RetroactiveIndex = null;
        }

        private void AddViolation(DateTimeOffset startDateTime, DateTimeOffset endDateTime, Enums.ViolationType violationType, Jurisdiction jurisdiction)
        {
            switch (violationType)
            {
                case Enums.ViolationType.Driving:
                    ViolationDurationDrivingSinceLastDailyReset = ViolationDurationDrivingSinceLastDailyReset.Add(endDateTime.Subtract(startDateTime));
                    break;
                case Enums.ViolationType.OnDuty:
                    ViolationDurationOnDutySinceLastDailyReset = ViolationDurationOnDutySinceLastDailyReset.Add(endDateTime.Subtract(startDateTime));
                    break;
                case Enums.ViolationType.Rest:
                    ViolationDurationOnDutySinceLastRestBreak = ViolationDurationOnDutySinceLastRestBreak.Add(endDateTime.Subtract(startDateTime));
                    break;
                case Enums.ViolationType.Weekly:
                    ViolationDurationOnDutySinceLastWeeklyReset = ViolationDurationOnDutySinceLastWeeklyReset.Add(endDateTime.Subtract(startDateTime));
                    break;
            }
            Violations.Add(new DutyStatusViolation(startDateTime, endDateTime, violationType, jurisdiction.JurisdictionId));
        }

        private void AddViolation(DateTimeOffset endDateTime, TimeSpan durationSinceLastDailyReset, TimeSpan hoursallowed, Enums.ViolationType violationType, Jurisdiction jurisdiction)
        {
            var violationOffset = TimeSpan.FromSeconds(0);
            switch (violationType)
            {
                case Enums.ViolationType.Driving:
                    violationOffset = ViolationDurationDrivingSinceLastDailyReset;
                    break;
                case Enums.ViolationType.OnDuty:
                    violationOffset = ViolationDurationOnDutySinceLastDailyReset;
                    break;
                case Enums.ViolationType.Rest:
                    violationOffset = ViolationDurationOnDutySinceLastRestBreak;
                    break;
                case Enums.ViolationType.Weekly:
                    violationOffset = ViolationDurationOnDutySinceLastWeeklyReset;
                    break;
            }

            var violationStartDateTime = endDateTime.Subtract(durationSinceLastDailyReset - hoursallowed).Add(violationOffset);
            AddViolation(violationStartDateTime, endDateTime, violationType, jurisdiction);
        }
    }
}
