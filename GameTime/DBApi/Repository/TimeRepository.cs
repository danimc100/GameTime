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

        public void DeleteAllTime(int idGame)
        {
            var lst = (from t in DBContext.Time
                       where t.IdGame == idGame
                       select t).ToList();

            lst.ForEach(t =>
            {
                DBContext.Time.Remove(t);
                DBContext.SaveChanges();
            });
        }
    }
}
