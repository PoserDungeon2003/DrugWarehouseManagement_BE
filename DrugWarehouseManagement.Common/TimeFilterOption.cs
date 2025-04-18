using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Common
{
    public enum TimeFilterOption
    {
        Day,
        Week,
        Month,
        Year
    }

    public static class DateTimeHelper
    {
        public static (Instant start, Instant end) GetInstantRange(TimeFilterOption option)
        {
            // Lấy thời điểm hiện tại và timezone hệ thống
            var now = SystemClock.Instance.GetCurrentInstant();
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var today = now.InZone(zone).Date;   // LocalDate ngày hôm nay

            switch (option)
            {
                case TimeFilterOption.Day:
                    {
                        var start = today.AtStartOfDayInZone(zone).ToInstant();
                        var end = start.Plus(Duration.FromDays(1)).Minus(Duration.FromTicks(1));
                        return (start, end);
                    }
                case TimeFilterOption.Week:
                    {
                        int diff = ((int)today.DayOfWeek + 6) % 7;  // thứ Hai = 0
                        var startOfWeek = today.PlusDays(-diff);
                        var start = startOfWeek.AtStartOfDayInZone(zone).ToInstant();
                        var end = start.Plus(Duration.FromDays(7)).Minus(Duration.FromTicks(1));
                        return (start, end);
                    }
                case TimeFilterOption.Month:
                    {
                        var firstOfMonth = new LocalDate(today.Year, today.Month, 1);
                        // Lấy số ngày trong tháng từ Calendar
                        int daysInMonth = today.Calendar.GetDaysInMonth(today.Year, today.Month);
                        var start = firstOfMonth.AtStartOfDayInZone(zone).ToInstant();
                        var end = start.Plus(Duration.FromDays(daysInMonth)).Minus(Duration.FromTicks(1));
                        return (start, end);
                    }
                case TimeFilterOption.Year:
                    {
                        var firstOfYear = new LocalDate(today.Year, 1, 1);
                        var start = firstOfYear.AtStartOfDayInZone(zone).ToInstant();
                        // Lấy ngày của năm hiện tại
                        int daysInYear = today.Calendar.GetDaysInYear(today.Year);
                        var end = start.Plus(Duration.FromDays(daysInYear)).Minus(Duration.FromTicks(1));
                        return (start, end);
                    }
                default:
                    {
                        // Mặc định trả về cả ngày hôm nay
                        var start = today.AtStartOfDayInZone(zone).ToInstant();
                        var end = start.Plus(Duration.FromDays(1)).Minus(Duration.FromTicks(1));
                        return (start, end);
                    }
            }
        }
    }
}
