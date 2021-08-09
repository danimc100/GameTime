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
                "Días {0} - {1}:{2}:{3}",
                t.Days,
                t.Hours,
                t.Minutes,
                t.Seconds);
        }
    }
}
