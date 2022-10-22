using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameTime.Core;
using GameTime.DBApi;
using GameTime.DBApi.ExtraEntities;
using GameTime.DBApi.Repository;


namespace GameTime
{
    public partial class FrPartials : Form
    {
        private int idGame;
        private GameRepository gameRep;
        private TimeRepository timeRep;
        private List<Time> timesLst;
        private List<TimeItemList> timesList;
        
        public int IdGame 
        { 
            set
            {
                var aux = GetTimesPerDay(23);

                idGame = value;
                UpdateView();
            }
        }

        public FrPartials()
        {
            InitializeComponent();
            gameRep = new GameRepository();
            timeRep = new TimeRepository();
        }

        private void GetTimesIndividually()
        {
            var lst = timeRep.ListTimes(idGame);
            timesList = new List<TimeItemList>();

            lst.ForEach(t =>
            {
                timesList.Add(new TimeItemList(t));
            });
        }

        private List<TimeItemList> GetTimesPerDay(int idGame)
        {
            List<TimeItemList> genList = new List<TimeItemList>();
            var lst = timeRep.ListTimes(idGame);

            if(lst == null || !lst.Any())
            {
                return null;
            }

            lst.ForEach(t => 
            { 
                if(t.StartTime.Day == t.EndTime.Day)
                {
                    UpdateTimesList(genList, t.StartTime, t.EndTime);
                }
                else
                {
                    UpdateTimesList(genList, t.StartTime, GetEndOfDay(t.StartTime));
                    UpdateTimesList(genList, GetBeginOfDay(t.EndTime), t.EndTime);
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

        private DateTime GetEndOfDay(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
        }

        private DateTime GetBeginOfDay(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        }

        private void UpdateView()
        {
            Game game = gameRep.Get(idGame);

            if(game == null)
            {
                return;
            }

            label1.Text = string.Format("{0} / {1}", game.Name, game.Title);
            timesLst = timeRep.ListTimes(idGame);
            listView1.Items.Clear();

            timesLst.ForEach(t => 
            {
                TimeSpan elapse = t.EndTime - t.StartTime;
                ListViewItem item = listView1.Items.Add(t.StartTime.ToString());
                item.Tag = t;
                item.SubItems.Add(t.EndTime.ToString());
                item.SubItems.Add(Utils.TimeFormat(elapse));
            });

            label3.Text = "-";
            label5.Text = Utils.TimeFormat(TotalToday());
            label7.Text = Utils.TimeFormat(Total());
        }

        private TimeSpan Total()
        {
            TimeSpan total = new TimeSpan();
            
            timesLst.ForEach(t => 
            {
                total = total.Add(t.EndTime - t.StartTime);
            });
            return total;
        }
        
        private TimeSpan TotalToday()
        {
            TimeSpan total = new TimeSpan();
            DateTime endTime;

            var lst = timesLst.Where(t => IsToday(t.StartTime)).ToList();
            lst.ForEach(t =>
            {
                if (IsToday(t.EndTime))
                {
                    endTime = t.EndTime;
                }
                else
                {
                    endTime = new DateTime(t.StartTime.Year, t.StartTime.Month, t.StartTime.Day, 23, 59, 59);
                }
                total = total.Add(endTime - t.StartTime);
            });
            return total;
        }

        private bool IsToday(DateTime t)
        {
            DateTime now = DateTime.Now;

            return t.Year == now.Year && t.Month == now.Month && t.Day == now.Day;
        }

        private void FrPartials_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            TimeSpan total = new TimeSpan();

            System.Console.WriteLine("SelectionChanged");
            foreach(ListViewItem item in listView1.SelectedItems)
            {
                Time t = (Time)item.Tag;
                total = total.Add(t.EndTime - t.StartTime);
            }

            label3.Text = Utils.TimeFormat(total);
        }
    }
}
