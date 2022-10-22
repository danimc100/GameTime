using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.DBApi.ExtraEntities
{
    public class TimeItemList : Time
    {
        public TimeItemList(DateTime start, DateTime end)
        {
            IdTime = 0;
            IdGame = 0;
            StartTime = start;
            EndTime = end;
            Total = end - start;
        }

        public TimeItemList(Time t)
        {
            IdGame = t.IdGame;
            IdTime = t.IdTime;
            StartTime = t.StartTime;
            EndTime = t.EndTime;
            Total = EndTime - StartTime;
        }

        public TimeSpan Total { get; set; }
    }
}
