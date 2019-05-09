using Eld.Core.Extensions;
using Eld.Core.Model;
using System;

namespace Eld.Audit.Model
{
    public class DutyStatusViolation
    {
        internal DutyStatusViolation(DateTimeOffset startDateTime, DateTimeOffset endDateTime, Enums.ViolationType violationType, Enums.Jurisdiction jurisdictionId)
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            ViolationType = violationType;
            Jurisdiction jurisdiction = new Jurisdiction(jurisdictionId);

            switch (violationType)
            {
                case Enums.ViolationType.Driving:
                    ViolationText = $"Driving more than {jurisdiction.DrivingHoursAllowed} hours.";
                    break;
                case Enums.ViolationType.OnDuty:
                    ViolationText = $"Driving after the {jurisdiction.OnDutyHoursAllowed}th hour on duty.";
                    break;
                case Enums.ViolationType.Rest:
                    ViolationText = $"No {jurisdiction.MinimumRestLengthHours.HoursToMinutes()}-minute rest break in the fist {jurisdiction.MaximumShiftHoursBeforeRestBreakRequired} hours";
                    break;
                case Enums.ViolationType.Weekly:
                    ViolationText = $"Driving after being on duty more than {jurisdiction.WeeklyOnDutyHoursAllowed} hours in the last {jurisdiction.WeeklyAuditTimeRangeDays} days.";
                    break;
            }
        }
        public override bool Equals(object obj)
        {

            if (!(obj is DutyStatusViolation item))
            {
                return false;
            }
            return item.StartDateTime == StartDateTime && item.EndDateTime == EndDateTime && item.ViolationType == ViolationType;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public DateTimeOffset StartDateTime { get; private set; }
        public DateTimeOffset EndDateTime { get; private set; }
        public string ViolationText { get; private set; }
        public Enums.ViolationType ViolationType { get; private set; }
    }
}
