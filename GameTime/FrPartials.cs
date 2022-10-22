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
using GameTime.Core.Extensions;
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
        private List<TimeItemList> timesList;
        
        public int IdGame 
        { 
            set
            {
                idGame = value;
                timesList = GetTimesPerDay();
                UpdateView();
            }
        }

        public FrPartials()
        {
            InitializeComponent();
            gameRep = new GameRepository();
            timeRep = new TimeRepository();
        }

        private List<TimeItemList> GetTimesIndividually()
        {
            List<TimeItemList> genList = new List<TimeItemList>();

            var lst = timeRep.ListTimes(idGame);
            lst.ForEach(t =>
            {
                genList.Add(new TimeItemList(t));
            });
            return genList;
        }

        private List<TimeItemList> GetTimesPerDay()
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

        private void UpdateView()
        {
            Game game = gameRep.Get(idGame);

            if(game == null)
            {
                return;
            }

            label1.Text = string.Format("{0} / {1}", game.Name, game.Title);
            listView1.Items.Clear();

            timesList.ForEach(t =>
            {
                ListViewItem item = listView1.Items.Add(t.StartTime.ToString());
                item.Tag = t;
                item.SubItems.Add(t.EndTime.ToString());
                item.SubItems.Add(Utils.TimeFormat(t.Total));
            });

            label3.Text = "-";
            label5.Text = Utils.TimeFormat(TotalToday());
            label7.Text = Utils.TimeFormat(Total());
        }

        private TimeSpan Total()
        {
            TimeSpan total = new TimeSpan();
            
            timesList.ForEach(t => 
            {
                total = total.Add(t.Total);
            });
            return total;
        }
        
        private TimeSpan TotalToday()
        {
            TimeSpan total = new TimeSpan();

            var lst = timesList.Where(t => t.StartTime.IsToday()).ToList();
            lst.ForEach(t =>
            {
                if (t.EndTime.IsToday())
                {
                    total = total.Add(t.Total);
                }
                else
                {
                    DateTime endTime = t.StartTime.GetEndOfDay();
                    total = total.Add(endTime - t.StartTime);
                }
            });
            return total;
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
                TimeItemList t = (TimeItemList)item.Tag;
                total = total.Add(t.Total);
            }

            label3.Text = Utils.TimeFormat(total);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timesList = GetTimesPerDay();
            UpdateView();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timesList = GetTimesIndividually();
            UpdateView();
        }
    }
}
