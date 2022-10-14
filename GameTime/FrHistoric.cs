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
using GameTime.DBApi.Logic;

namespace GameTime
{
    public partial class FrHistoric : Form
    {
        private ReportsLogic reportsLg;

        public FrHistoric()
        {
            InitializeComponent();
            reportsLg = new ReportsLogic();
        }

        private void UpdateView()
        {
            var lst = reportsLg.GeneralReports(true);

            if(lst != null)
            {
                listView1.Items.Clear();
                foreach (var game in lst)
                {
                    var item = listView1.Items.Add(game.Name);
                    item.Name = game.Name;
                    item.SubItems.Add(game.Title);
                    item.SubItems.Add(Utils.TimeFormat(game.TotalTime));
                    //item.SubItems.Add(game.Created.ToShortDateString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void FrHistoric_Activated(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void borrarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                string name = item.Name;
                string title = item.SubItems[1].Text;

                DialogResult dialogResult = MessageBox.Show("¿Está seguro de eliminar?", title, MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    listView1.Items.Remove(item);
                    //GameList.DeleteHistoric(name);
                }
                UpdateView();
            }
        }

        private void FrHistoric_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}