using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTime.DBApi;

namespace GameTime.DBApi.Repository
{
    public class TimeRepository : GenericRepository
    {
        public TimeRepository()
            : base()
        {

        }

        public void InsertTime(Time time)
        {
            DBContext.Time.Add(time);
            DBContext.SaveChanges();
        }
    }
}
