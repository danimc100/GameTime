using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.Core.Extensions
{
    public static class DateTimeExtension
    {
        public static bool IsToday(this DateTime t)
        {
            DateTime now = DateTime.Now;

            return t.Year == now.Year && t.Month == now.Month && t.Day == now.Day;
        }
        public static DateTime GetEndOfDay(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
        }
        public static DateTime GetBeginOfDay(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        }
    }
}
