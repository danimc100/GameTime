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
            //var lst = (from g in DBContext.Game
            //           join t in DBContext.Time on g.IdGame equals t.IdGame
            //           group new { g, t } by new { g.IdGame, g.Name, g.Title } into gg
            //           select new GLGeneralList
            //           {
            //               IdGame = gg.FirstOrDefault().g.IdGame,
            //               Name = gg.FirstOrDefault().g.Name,
            //               Title = gg.FirstOrDefault().g.Title,
            //               TotalTicks = gg.Sum(f => f.t.EndTime.Ticks - f.t.StartTime.Ticks),
            //               Historic = gg.FirstOrDefault().g.Historic
            //           });

            var lst = (from g in DBContext.Game
                       join t in DBContext.Time on g.IdGame equals t.IdGame
                       select new GLBaseGeneralList
                       {
                           IdGame = g.IdGame,
                           Name = g.Name,
                           Title = g.Title,
                           StartTime = t.StartTime,
                           EndTime = t.EndTime,
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
