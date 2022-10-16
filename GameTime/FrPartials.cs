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
using GameTime.DBApi.Repository;


namespace GameTime
{
    public partial class FrPartials : Form
    {
        private GameRepository gameRep;
        private TimeRepository timeRep;
        private List<Time> timesLst;
        
        public int IdGame 
        { 
            set
            {
                UpdateView(value);
            }
        }

        public FrPartials()
        {
            InitializeComponent();
            gameRep = new GameRepository();
            timeRep = new TimeRepository();
        }

        private void UpdateView(int idGame)
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
                ListViewItem item = listView1.Items.Add(Utils.TimeFormat(elapse));
                item.Tag = t.IdTime;
            });
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
    }
}
