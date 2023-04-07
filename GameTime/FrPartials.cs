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
using GameTime.DBApi.Logic;


namespace GameTime
{
    public partial class FrPartials : Form
    {
        private int idGame;
        private GameRepository gameRep;
        private TimeRepository timeRep;
        private ReportsLogic reportsLogic;
        private List<TimeItemList> timesList;
        
        public int IdGame 
        { 
            set
            {
                idGame = value;
                timesList = reportsLogic.GetTimesPerDay(idGame);
                UpdateView();
            }
        }

        public FrPartials()
        {
            InitializeComponent();
            gameRep = new GameRepository();
            timeRep = new TimeRepository();
            reportsLogic = new ReportsLogic();
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
            timesList = reportsLogic.GetTimesPerDay(idGame);
            UpdateView();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timesList = reportsLogic.GetTimesIndividually(idGame);
            UpdateView();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FrEditTime editTime = new FrEditTime();
            editTime.Inicio = DateTime.Now;
            editTime.Fin = DateTime.Now;

            DialogResult result = editTime.ShowDialog();
            if(result == DialogResult.OK)
            {
                Time time = new Time();
                time.StartTime = editTime.Inicio;
                time.EndTime = editTime.Fin;
                time.IdGame = idGame;
                timeRep.InsertTime(time);
                timesList = reportsLogic.GetTimesPerDay(idGame);
                UpdateView();
            }
            editTime.Dispose();
        }
    }
}
