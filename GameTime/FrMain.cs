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

namespace GameTime
{
    public partial class FrMain : Form
    {
        private GameList gameList;

        public FrMain()
        {
            InitializeComponent();
            gameList = new GameList();
            listView1.SetDoubleBuffered(true);
            UpdateProcess();

            //int sessionId = System.Diagnostics.Process.GetCurrentProcess().SessionId;
            //string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //var list = System.Diagnostics.Process.GetProcesses().Where(p => p.SessionId == sessionId).ToList();

            //var t = new TimeSpan(7, 30, 0).Ticks;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void AddProcess()
        {
            if (comboBox1.SelectedItem != null)
            {
                gameList.Add(comboBox1.SelectedItem.ToString());
                UpdateView();
            }
        }

        private void UpdateView(bool refreshAllA = false)
        {
            if (refreshAllA)
            {
                listView1.Items.Clear();
            }

            foreach (var game in gameList.List)
            {
                var items = listView1.Items.Find(game.Name, false);
                if (items.Length > 0)
                {
                    listView1.BeginUpdate();
                    if (game.Active)
                    {
                        items[0].SubItems[2].Text = game.PartialTime.ToString();
                        items[0].SubItems[3].Text = TimeFormat(game.TotalTime);
                        items[0].SubItems[4].Text = "Si";
                    }
                    else
                    {
                        items[0].SubItems[4].Text = string.Empty;
                    }
                    listView1.EndUpdate();
                }
                else
                {
                    var item = listView1.Items.Add(game.Name);
                    item.Name = game.Name;
                    item.SubItems.Add(game.Title);
                    item.SubItems.Add(game.PartialTime.ToString());
                    item.SubItems.Add(TimeFormat(game.TotalTime));
                    item.SubItems.Add(string.Empty);
                }
            }
        }

        private void UpdateProcess()
        {
            var pList = gameList.GetProcessList();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(pList.ToArray());
        }
        private void button1_Click(object sender, EventArgs e)
        {
            AddProcess();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var elapsed = new TimeSpan(0, 0, 0, 0, timer1.Interval);
            gameList.CheckGames(elapsed);
            UpdateView();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            gameList.SaveDate();
        }

        private string TimeFormat(TimeSpan t)
        {
            return string.Format(
                "Días {0} - {1}:{2}:{3}",
                t.Days,
                t.Hours,
                t.Minutes,
                t.Seconds);
        }

        #region Events

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateProcess();
        }

        private void editarTítuloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                string name = listView1.SelectedItems[0].Name;
                string title = listView1.SelectedItems[0].SubItems[1].Text;

                var result = Dialogs.InputBox("Editar título", "Nuevo título", ref title);
                if (result == DialogResult.OK)
                {
                    listView1.SelectedItems[0].SubItems[1].Text = title;
                    gameList.List.ForEach(item =>
                    {
                        if (item.Name == name)
                        {
                            item.Title = title;
                        }
                    });
                }
            }
        }

        private void eliminarToolStripMenuItem_Click(object sender, EventArgs e)
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
                    gameList.Delete(name);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox1.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            gameList.SaveDate();
        }

        private void borrarTiemposToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var name = listView1.SelectedItems[0].Name;
                var gameItem = gameList.Get(name);
                if (gameItem != null)
                {
                    gameItem.PartialTime = new TimeSpan(0);
                    gameItem.TotalTime = new TimeSpan(0);
                    UpdateView(true);
                }
            }
        }

        private void guardarEnHistóricoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                string name = listView1.SelectedItems[0].Name;
                gameList.Historify(name);
                UpdateView(true);
            }
        }

        #endregion
    }
}
