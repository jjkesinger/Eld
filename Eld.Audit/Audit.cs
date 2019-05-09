using Eld.Audit.Model;
using Eld.Core.Extensions;
using Eld.Core.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Eld.Test")]
namespace Eld.Audit
{
    public class LogAuditor
    {
        public ConcurrentDictionary<string, List<DutyStatusViolation>> Audit(List<DutyStatusEvent> logs)
        {
            return Audit(GetDutyStatusByDriverAndPeriods(logs));
        }

        /// <summary>
        /// Takes a list of driver duty status evetns and audits them asyncronously and returns a dictionary of violations
        /// </summary>
        /// <param name="logs"></param>
        /// <returns>Threadsave Dictionary of Violations by DriverId key</returns>
        private ConcurrentDictionary<string, List<DutyStatusViolation>> Audit(ConcurrentDictionary<string, 
            List<DutyStatusAuditPeriod>> groupedPeriods)
        {
            ConcurrentDictionary<string, List<DutyStatusViolation>> violations = new ConcurrentDictionary<string, List<DutyStatusViolation>>();

            Parallel.ForEach(groupedPeriods, (period) =>
            {
                List<DutyStatusViolation> currentViolations = AuditGroupedPeriods(period.Value);
                if (currentViolations.Any())
                {
                    violations.AddOrUpdate(period.Key, currentViolations, (key, value) => { value.AddRange(currentViolations); return value; });
                }
            });

            return violations;
        }

        /// <summary>
        /// Takes a list of periods between weekly resets and audits them syncronously for violations (must run syncronously)
        /// </summary>
        /// <param name="periods">Log periods between weekly resets divided by driver</param>
        /// <returns>List of Violations for periods</returns>
        private List<DutyStatusViolation> AuditGroupedPeriods(List<DutyStatusAuditPeriod> periods)
        {
            List<DutyStatusViolation> periodViolations = new List<DutyStatusViolation>();

            foreach (var period in periods)
            {
                period.SetWeeklyReset(period.PeriodFirstOnDutyDateTime);

                for (var i = 0; i < period.DutyStatusEvents.Count; i++)
                {
                    var statusEvent = period.DutyStatusEvents[i];
                    switch (statusEvent.DutyStatus)
                    {
                        case Enums.DutyStatus.OffDuty:
                        case Enums.DutyStatus.Sleeper:
                            if (statusEvent.IsQualifyingWeeklyReset)
                            {
                                period.SetWeeklyReset(statusEvent.EndDateTime);
                                break;
                            }

                            if (statusEvent.IsQualifyingDailyReset)
                            {
                                period.SetDailyReset(statusEvent.EndDateTime);
                                break;
                            }

                            //this is the second part of split sleeper need to retroactively process time since reset applied
                            //so break to back process events
                            if (period.SecondSleeperSplit?.EndDateTime == statusEvent.EndDateTime)
                            {
                                i = period.GetIndexToBackProcessTimeBetweenSplits();
                                break;
                            }

                            if (statusEvent.IsTexasSleeperExempt == false)
                            {
                                //if the first is off/sleep we can't assume what previous was so don't know to add onduty time.
                                if (i > 0)
                                {
                                    period.AddOnDutyTime(statusEvent.Duration);
                                }
                            }

                            if (statusEvent.IsQualifyingRestPeriod)
                            {
                                period.SetRestReset(statusEvent.EndDateTime);
                            }
                            break;

                        case Enums.DutyStatus.Driving:
                            period.AddDriveTime(statusEvent.Duration);

                            #region CheckOnDuty
                            if (period.DurationOnDutySinceLastDailyReset > statusEvent.MaxOnDutyHoursAllowed)
                            {
                                if (SetSplitSleeper(period, statusEvent, i) == false)
                                {
                                    period.AddViolation(statusEvent.StartDateTime, statusEvent.EndDateTime, statusEvent.MaxOnDutyHoursAllowed, Enums.ViolationType.OnDuty, statusEvent.DutyStatusJursidiction);
                                }
                            }
                            #endregion

                            #region CheckRestBreak
                            if (period.DurationOnDutySinceLastRestBreak > statusEvent.MaxShiftHoursBeforeRestRequiredTimespan)
                            {
                                if (statusEvent.IsRestBreakExempt)
                                {
                                    if (period.DurationOnDutySinceLastDailyReset > statusEvent.MaxShiftHoursForRestBreakExemption)
                                    {
                                        period.AddViolation(statusEvent.StartDateTime, statusEvent.EndDateTime, statusEvent.MaxShiftHoursBeforeRestRequiredTimespan, Enums.ViolationType.Rest, statusEvent.DutyStatusJursidiction);
                                    }
                                }
                                else
                                {
                                    period.AddViolation(statusEvent.StartDateTime, statusEvent.EndDateTime, statusEvent.MaxShiftHoursBeforeRestRequiredTimespan, Enums.ViolationType.Rest, statusEvent.DutyStatusJursidiction);
                                }
                            }
                            #endregion

                            #region CheckDriving
                            if (period.DurationDrivingSinceLastDailyReset > statusEvent.DutyStatusJursidiction.DrivingHoursAllowed.HoursToTimeSpan())
                            {
                                period.AddViolation(statusEvent.StartDateTime, statusEvent.EndDateTime, statusEvent.MaxDrivingHoursAllowed, Enums.ViolationType.Driving, statusEvent.DutyStatusJursidiction);
                            }
                            #endregion

                            #region CheckWeekly
                            if (period.DurationOnDutySinceLastWeeklyReset > statusEvent.MaxWeeklyHoursAllowedTimeSpan)
                            {
                                if (period.LastWeeklyReset.HasValue && (statusEvent.EndDateTime.Subtract(period.LastWeeklyReset.Value) <= statusEvent.WeeklyAuditDaysTimeSpan || period.LastWeeklyReset == statusEvent.StartDateTime))
                                {
                                    period.AddViolation(statusEvent.StartDateTime, statusEvent.EndDateTime, statusEvent.MaxWeeklyHoursAllowedTimeSpan, Enums.ViolationType.Weekly, statusEvent.DutyStatusJursidiction);
                                }
                                else
                                {
                                    TimeSpan onDutyLastWeek = period.OnDutyLastWeeklyDays(statusEvent.EndDateTime, statusEvent.DutyStatusJursidiction.WeeklyAuditTimeRangeDays);
                                    if (onDutyLastWeek > statusEvent.MaxWeeklyHoursAllowedTimeSpan)
                                    {
                                        period.AddViolation(statusEvent.StartDateTime, statusEvent.EndDateTime, statusEvent.MaxWeeklyHoursAllowedTimeSpan, Enums.ViolationType.Weekly, statusEvent.DutyStatusJursidiction, onDutyLastWeek);
                                    }
                                }
                            }
                            #endregion
                            break;

                        case Enums.DutyStatus.OnDutyNotDriving:
                            period.AddOnDutyTime(statusEvent.Duration);
                            break;
                    }
                }

                periodViolations.AddRange(period.Violations);
            }

            return periodViolations;
        }

        /// <summary>
        /// Checks if status event is between 2 qualifying split sleepers
        /// </summary>
        /// <param name="period">Period of status event</param>
        /// <param name="statusEvent">Current status event</param>
        /// <param name="i">Index of current status event to go back and reprocess if qualifying split sleeper</param>
        /// <returns>True if split sleeper occurs, False if not</returns>
        private bool SetSplitSleeper(DutyStatusAuditPeriod period, DutyStatusEvent statusEvent, int i)
        {
            bool hasSplitSleeper = false;
            var firstSplit = period.SplitDutyStatusEvents.Where(f => f.StartDateTime > period.LastDailyReset && f.StartDateTime < statusEvent.StartDateTime).OrderByDescending(g => g.StartDateTime).FirstOrDefault();
            var secondSplit = period.SplitDutyStatusEvents.Where(f => f.StartDateTime >= statusEvent.EndDateTime).OrderBy(g => g.StartDateTime).FirstOrDefault();

            if (firstSplit != null && secondSplit != null && (firstSplit.IsMainSleeperSplit || secondSplit.IsMainSleeperSplit || statusEvent.DutyStatusJursidiction.IsMainSplitRequiredInSleeperOnly == false) &&
                secondSplit.Duration.Add(firstSplit.Duration) >= statusEvent.DutyStatusJursidiction.SplitTotalForReset.HoursToTimeSpan())
            {
                if (period.DurationOnDutySinceLastDailyReset.Subtract(firstSplit.Duration) <= statusEvent.DutyStatusJursidiction.OnDutyHoursAllowed.HoursToTimeSpan())
                {
                    hasSplitSleeper = true;
                    period.SetSplitSleeper(firstSplit, secondSplit, statusEvent.StartDateTime, i);
                }
            }

            return hasSplitSleeper;
        }

        /// <summary>
        /// Groups a list of DutyStatusAuditPeriods by Driver and Periods between weekly resets and returns a dictionary with DriverId key
        /// </summary>
        /// <param name="logs"></param>
        /// <returns>Dictionary of DutyStatusPeriods</returns>
        private ConcurrentDictionary<string, List<DutyStatusAuditPeriod>> GetDutyStatusByDriverAndPeriods(List<DutyStatusEvent> logs)
        {
            var d = new ConcurrentDictionary<string, List<DutyStatusAuditPeriod>>();

            var groups = logs.GroupBy(f => f.DriverId);
            Parallel.ForEach(groups, (group) =>
            {
                List<DutyStatusEvent> auditPeriodLogs = new List<DutyStatusEvent>();
                bool skipNext = false;

                for (var i = 0; i < group.Count(); i++)
                {
                    if (i < group.Count() - 1)
                    {
                        skipNext = group.ElementAt(i).CombineDutyStatusEvents(group.ElementAt(i + 1));
                    }

                    auditPeriodLogs.Add(group.ElementAt(i));

                    if (group.ElementAt(i).IsQualifyingWeeklyReset || (i == group.Count() - 1 && auditPeriodLogs.Any()) || (skipNext && i + 1 == group.Count() - 1 && auditPeriodLogs.Any()))
                    {
                        d.AddOrUpdate(group.Key, new List<DutyStatusAuditPeriod>() { new DutyStatusAuditPeriod(group.Key, auditPeriodLogs) }, (key, value) => { value.Add(new DutyStatusAuditPeriod(group.Key, auditPeriodLogs)); return value; });
                        auditPeriodLogs = new List<DutyStatusEvent>();
                    }

                    if (skipNext) //combine if next status off duty or same duty status
                    {
                        i++; //skip status that was combined
                    }
                }
            });

            return d;
        }
    }
}
