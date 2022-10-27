using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTime.DBApi.ExtraEntities;
using GameTime.DBApi.Repository;
using GameTime.Core.Extensions;

namespace GameTime.DBApi.Logic
{
    public class ReportsLogic
    {
        private ReportsRepository reportsRep;
        private TimeRepository timeRep;

        public ReportsLogic()
        {
            reportsRep = new ReportsRepository();
            timeRep = new TimeRepository();
        }

        public List<GLGeneralList> GeneralReports(bool? historic = null)
        {
            List<GLGeneralList> lst = new List<GLGeneralList>();
            var baseLst = reportsRep.GeneralReports(historic);

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

        public List<TimeItemList> GetTimesIndividually(int? idGame = null)
        {
            List<TimeItemList> genList = new List<TimeItemList>();

            var lst = timeRep.ListTimes(idGame);
            lst.ForEach(t =>
            {
                genList.Add(new TimeItemList(t));
            });
            return genList;
        }

        public List<TimeItemList> GetTimesPerDay(int? idGame)
        {
            List<TimeItemList> genList = new List<TimeItemList>();
            var lst = timeRep.ListTimes(idGame);

            if (lst == null || !lst.Any())
            {
                return null;
            }

            lst.ForEach(t =>
            {
                if (t.StartTime.Day == t.EndTime.Day)
                {
                    UpdateTimesList(genList, t.StartTime, t.EndTime);
                }
                else
                {
                    UpdateTimesList(genList, t.StartTime, t.StartTime.GetEndOfDay());
                    UpdateTimesList(genList, t.EndTime.GetBeginOfDay(), t.EndTime);
                }
            });

            return genList;
        }

        private void UpdateTimesList(List<TimeItemList> genList, DateTime startTime, DateTime endTime)
        {
            var item = genList.Where(t => t.StartTime.Year == startTime.Year && t.StartTime.Month == startTime.Month && t.StartTime.Day == startTime.Day)
                     .FirstOrDefault();

            if (item == null)
            {
                item = new TimeItemList(startTime, endTime);
                genList.Add(item);
            }
            else
            {
                if (startTime < item.StartTime)
                {
                    item.StartTime = startTime;
                }

                if (endTime > item.EndTime)
                {
                    item.EndTime = endTime;
                }

                item.Total = item.Total.Add(endTime - startTime);
            }
        }
    }
}
