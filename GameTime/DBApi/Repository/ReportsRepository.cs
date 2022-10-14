using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTime.DBApi.ExtraEntities;

namespace GameTime.DBApi.Repository
{
    public class ReportsRepository : GenericRepository
    {
        public ReportsRepository()
            : base()
        {
        }

        public List<GLBaseGeneralList> GeneralReports(bool? historic = null)
        {
            var lst = (from g in DBContext.Game
                       join t in DBContext.Time on g.IdGame equals t.IdGame into rep from lstRep in rep.DefaultIfEmpty()
                       select new GLBaseGeneralList
                       {
                           IdGame = g.IdGame,
                           Name = g.Name,
                           Title = g.Title,
                           StartTime = lstRep.StartTime,
                           EndTime = lstRep.EndTime,
                           Historic = g.Historic
                       });

            if (historic != null)
            {
                lst = lst.Where(f => f.Historic == historic);
            }

            return lst.ToList();
        }
    }
}
