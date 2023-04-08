using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
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

        public Time FindTime(int idTime)
        {
            return DBContext.Time.Find(idTime);
        }

        public void UpdateTime(Time time) 
        { 
            DBContext.Entry(time).State = System.Data.Entity.EntityState.Modified;
            DBContext.SaveChanges();
        }

        public void DeleteAllTime(int idGame)
        {
            var lst = ListTimes(idGame);

            lst.ForEach(t =>
            {
                DBContext.Time.Remove(t);
                DBContext.SaveChanges();
            });
        }

        public List<Time> ListTimes(int? idGame = null)
        {
            var lst = (from t in DBContext.Time select t);

            if(idGame != null)
            {
                lst = lst.Where(t => t.IdGame == idGame.Value);
            }

            lst = lst.OrderBy(t => t.StartTime);

            return lst.ToList();
        }
    }
}
