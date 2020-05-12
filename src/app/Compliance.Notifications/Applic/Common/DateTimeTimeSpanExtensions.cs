using System;
using Compliance.Notifications.Resources;

namespace Compliance.Notifications.Applic.Common
{
    public static class DateTimeTimeSpanExtensions
    {
        public static string InPeriodFromNowPure(this DateTime dateTime, Func<DateTime> getNow)
        {
            if (getNow == null) throw new ArgumentNullException(nameof(getNow));
            var now = getNow();
            var timeSpan = dateTime - now;
            return timeSpan.TimeSpanToString();
        }

        public static string InPeriodFromNow(this DateTime dateTime)
        {
            return InPeriodFromNowPure(dateTime, () => DateTime.Now);
        }

        public static string TimeSpanToString(this TimeSpan timeSpan)
        {
            var totalDaysRounded = Convert.ToInt32(Math.Round(timeSpan.TotalDays));
            if (timeSpan.TotalDays < 1)
            {
                var totalHoursRounded = Convert.ToInt32(Math.Round(timeSpan.TotalHours));
                if (totalHoursRounded == 1)
                    return $"{timeSpan.Hours} {strings.Hour}";
                return $"{timeSpan.Hours} {strings.Hours}";
            }
            if (totalDaysRounded == 1)
                return $"{totalDaysRounded} {strings.Day}";
            return $"{totalDaysRounded} {strings.Days}";
        }
    }
}
