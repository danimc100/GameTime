using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTime.DBApi.ExtraEntities;
using GameTime.DBApi.Repository;

namespace GameTime.DBApi.Logic
{
    public class ReportsLogic
    {
        private ReportsRepository reportsRepository;

        public ReportsLogic()
        {
            reportsRepository = new ReportsRepository();
        }

        public List<GLGeneralList> GeneralReports(bool? historic = null)
        {
            List<GLGeneralList> lst = new List<GLGeneralList>();
            var baseLst = reportsRepository.GeneralReports(historic);

            var games = baseLst.Select(f => f.IdGame).Distinct().ToList();
            games.ForEach(idGame =>
            {
                var times = baseLst.Where(f => f.IdGame == idGame).ToList();
                long ticks = 0;
                foreach(var time in times)
                {
                    if(time.StartTime.HasValue)
                    {
                        ticks += time.EndTime.Value.Ticks - time.StartTime.Value.Ticks;
                    }
                }

                GLGeneralList item = new GLGeneralList();
                item.IdGame = times.First().IdGame;
                item.Name = times.First().Name;
                item.Title = times.First().Title;
                item.TotalTime = new TimeSpan(ticks);
                lst.Add(item);
            });

            if(lst.Any())
            {
                return lst;
            }
            else
            {
                return null;
            }
        }
    }
}
