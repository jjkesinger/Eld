using System;

namespace Eld.Core.Extensions
{
    public static class Extensions
    {
        public static TimeSpan HoursToTimeSpan(this double hours)
        {
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan HoursToTimeSpan(this int hours)
        {
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan DaysToTimeSpan(this int days)
        {
            return TimeSpan.FromDays(days);
        }

        public static double HoursToMinutes(this double hours)
        {
            return hours * 60;
        }
    }
}
