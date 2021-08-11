using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.Core
{
    public class Utils
    {
        public static string TimeFormat(TimeSpan t)
        {
            return string.Format(
                "Días {0} - {1:D2}:{2:D2}:{3:D2}",
                t.Days,
                t.Hours,
                t.Minutes,
                t.Seconds);
        }
    }
}
