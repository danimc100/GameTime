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
        private List<TimeItemList> timesList;
        private FormSaveState formSaveState;
        
        public int IdGame 
        { 
            set
            {
                using(var reportsLogic = new ReportsLogic())
                {
                    idGame = value;
                    timesList = reportsLogic.GetTimesPerDay(idGame);
                    UpdateView();
                }
            }
        }

        public FrPartials()
        {
            InitializeComponent();
            formSaveState = new FormSaveState(this);
        }

        private void UpdateView()
        {
            using (var gameRep = new GameRepository())
            {
                Game game = gameRep.Get(idGame);

                if (game != null)
                {
                    label1.Text = string.Format("{0} / {1}", game.Name, game.Title);
                }
                else
                {
                    label1.Text = "Todos";
                }

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
            //e.Cancel = true;
            //Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
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
            using(var reportsLogic = new ReportsLogic())
            {
                timesList = reportsLogic.GetTimesPerDay(idGame);
                UpdateView();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (var reportsLogic = new ReportsLogic())
            {
                timesList = reportsLogic.GetTimesIndividually(idGame);
                UpdateView();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(idGame != 0)
            {
                FrEditTime editTime = new FrEditTime();
                editTime.Inicio = DateTime.Now;
                editTime.Fin = DateTime.Now;

                DialogResult result = editTime.ShowDialog();
                if (result == DialogResult.OK)
                {
                    using (var timeRep = new TimeRepository())
                    {
                        using (var reportsLogic = new ReportsLogic())
                        {
                            Time time = new Time();
                            time.StartTime = editTime.Inicio;
                            time.EndTime = editTime.Fin;
                            time.IdGame = idGame;
                            timeRep.InsertTime(time);
                            timesList = reportsLogic.GetTimesPerDay(idGame);
                            UpdateView();
                        }
                    }
                }
                editTime.Dispose();
            }
            else
            {
                MessageBox.Show("No hay un nuego seleccionado.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count == 0)
            {
                return;
            }

            Time time = listView1.SelectedItems[0].Tag as Time;

            if(time.IdTime == 0)
            {
                MessageBox.Show("No se puede editar cuando la lista de tiempos está agrupada.");
                return;
            }   

            FrEditTime editTime = new FrEditTime();
            editTime.Inicio = time.StartTime;
            editTime.Fin = time.EndTime;
            
            DialogResult result = editTime.ShowDialog();
            if(result == DialogResult.OK) 
            {
                using (var timeRep = new TimeRepository())
                {
                    using (var reportsLogic = new ReportsLogic())
                    {
                        var timeMod = timeRep.FindTime(time.IdTime);
                        timeMod.StartTime = editTime.Inicio;
                        timeMod.EndTime = editTime.Fin;
                        timeRep.UpdateTime(timeMod);
                        timesList = reportsLogic.GetTimesPerDay(idGame);
                        UpdateView();
                    }
                }
            }
        }
    }
}
