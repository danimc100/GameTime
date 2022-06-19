using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using SharpDX.XInput;
using GameTime.Core;
using AudioSwitcher.AudioApi.CoreAudio;

namespace GameTime
{
    public partial class FrMain : Form
    {
        private const int TIMER_INTERVAL = 5000;
        private const string AUDIODEVICECFG_NAME = "AudioDevices.cfg";
        private Controller controller;
        private GameList gameList;
        private bool lastAnyActive;
        private FrHistoric frHistoric;
        private FrTime frTime;
        private bool timeChanged;
        private CoreAudioController audio;
        private List<GameTime.Core.AudioDeviceItem> list;

        public FrMain()
        {
            InitializeComponent();

            lastAnyActive = false;
            gameList = new GameList();
            if(gameList.LoadData() != Enum.GameListResult.Ok)
            {
                MessageBox.Show("Se ha producido un error al cargar la base de datos.");
            }

            listView1.SetDoubleBuffered(true);
            timer1.Interval = TIMER_INTERVAL;
            UpdateProcess();

            controller = new Controller(UserIndex.One);

            frHistoric = null;
            frTime = null;
            timeChanged = false;

            //this.ShowInTaskbar = false;

            //int sessionId = System.Diagnostics.Process.GetCurrentProcess().SessionId;
            //string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //var list = System.Diagnostics.Process.GetProcesses().Where(p => p.SessionId == sessionId).ToList();
            //var t = new TimeSpan(7, 30, 0).Ticks;

            audio = new CoreAudioController();
            CoreAudioDevice dev = audio.GetDefaultDevice(AudioSwitcher.AudioApi.DeviceType.Playback, AudioSwitcher.AudioApi.Role.Multimedia);
            label4.Text = dev.FullName;

            //var devices = audio.GetDevices();
            //var list = new List<AudioDeviceItem>();
            //list.Add(new AudioDeviceItem { Key = "cascos", Name = "Audífono de los auriculares con micrófono (CORSAIR VOID ELITE USB Gaming Headset)", Id = "2aeffc83-75fb-4581-868b-db51a053aab1" });
            //list.Add(new AudioDeviceItem { Key = "altavoces", Name = "Altavoces (Steam Streaming Speakers)", Id = "a5a0fb4c-6d0f-4b06-abf9-dbaef3b59b1a" });
            //File.WriteAllText(AUDIODEVICECFG_NAME, JsonSerializer.Serialize(list));

            list = JsonSerializer.Deserialize<List<GameTime.Core.AudioDeviceItem>>(File.ReadAllText(AUDIODEVICECFG_NAME));
            

            //dev.Dispose();
            //dev = null;
            //audio.Dispose();
            //audio = null;
        }

        private void AddProcess()
        {
            if (comboBox1.SelectedItem != null)
            {
                ProcessInfo pInfo = comboBox1.SelectedItem as ProcessInfo;
                gameList.Add(pInfo.ProcessName);
                UpdateView();
            }
        }

        private void UpdateView(bool refreshAll = false)
        {
            if (refreshAll)
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
                        items[0].SubItems[3].Text = Utils.TimeFormat(game.TotalTime);
                        items[0].SubItems[4].Text = "Si";
                        TimeChanged(true);
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
                    item.SubItems.Add(Utils.TimeFormat(game.TotalTime));
                    item.SubItems.Add(string.Empty);
                }
            }
        }

        private void UpdateProcess()
        {
            var pList = gameList.GetProcessInfoList(); //gameList.GetProcessList(true);
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(pList.ToArray());
        }

        private void UpdateController()
        {
            if(controller.IsConnected)
            {
                label2.Text = "Conectado";
                BatteryInformation bi = controller.GetBatteryInformation(BatteryDeviceType.Gamepad);
                progressBar1.Value = (int)bi.BatteryLevel;
                label3.Text = string.Format("{0} - {1}",
                        System.Enum.GetName(typeof(BatteryLevel), bi.BatteryLevel),
                        System.Enum.GetName(typeof(BatteryType), bi.BatteryType)
                        );
            }
            else
            {
                label2.Text = "Desconectado";
                label3.Text = "Desconocido";
            }

        }

        private void ShowHistoric()
        {
            if(frHistoric == null)
            {
                frHistoric = new FrHistoric();
                frHistoric.GameList = gameList;
            }

            frHistoric.Show();
        }

        private TimeSpan? ShowFrTime(TimeSpan time)
        {
            if(frTime == null)
            {
                frTime = new FrTime();
            }

            frTime.Time = time;
            DialogResult result = frTime.ShowDialog();
            TimeSpan timeSpan = frTime.Time;
            frTime.Dispose();
            frTime = null;

            if (result == DialogResult.OK)
            {
                return timeSpan;
            }
            else
            {
                return null;
            }
        }

        private void TimeChanged(bool state)
        {
            timeChanged = state;
            if(state)
            {
                button3.BackColor = Color.Orange;
            }
            else
            {
                button3.BackColor = SystemColors.Control;
            }
        }

        private void SetMinimizedWindow()
        {
            WindowState = FormWindowState.Minimized;
            Hide();
            ShowInTaskbar = false;
        }

        private void SetNormalWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
        }

        private void SaveWindowState()
        {
            if(WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Save();
            }
        }

        private void SetWindowState()
        {
            SetNormalWindow();
            Location = Properties.Settings.Default.Location;
        }

        private void SaveGameListAndWindowStatus()
        {
            gameList.SaveDate();
            TimeChanged(false);
            SaveWindowState();
        }

        private CoreAudioDevice GetAudioDevice(string id)
        {
            Guid guidDev = new Guid(id);
            return audio.GetDevice(guidDev);
        }

        private void SetAudioDevice(string id)
        {
            CoreAudioDevice dev = GetAudioDevice(id);
            if(dev != null)
            {
                audio.SetDefaultDevice(dev);
                label4.Text = dev.FullName;
            }
            else
            {
                MessageBox.Show("No se ha encontrado el dispositivo de audio.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddProcess();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var elapsed = new TimeSpan(0, 0, 0, 0, timer1.Interval);
            gameList.CheckGames(elapsed);
            bool anyActive = gameList.AnyActive;
            UpdateView();
            UpdateController();

            if(anyActive != lastAnyActive && !anyActive)
            {
                SaveGameListAndWindowStatus();
            }
            lastAnyActive = anyActive;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            gameList.SaveDate();
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
                            TimeChanged(true);
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
                    TimeChanged(true);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox1.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveGameListAndWindowStatus();
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
                    UpdateView(true);
                    TimeChanged(true);
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
                TimeChanged(true);
            }
        }

        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            ShowHistoric();
        }

        private void cambiarTiempoTotalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                int index = listView1.SelectedItems[0].Index;
                TimeSpan time = gameList.List[index].TotalTime;
                TimeSpan? resultTime = ShowFrTime(time);
                if(resultTime.HasValue)
                {
                    gameList.List[index].TotalTime = resultTime.Value;
                    UpdateView(true);
                    TimeChanged(true);
                }
            }
        }

        private void FrMain_Resize(object sender, EventArgs e)
        {
            if( WindowState == FormWindowState.Minimized )
            {
                SetMinimizedWindow();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SetNormalWindow();
        }

        private void FrMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowState();
        }

        private void FrMain_Load(object sender, EventArgs e)
        {
            SetWindowState();
        }

        private void borrarTiempoParcialYBorrarDelTotalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                var name = listView1.SelectedItems[0].Name;
                var gameItem = gameList.Get(name);
                if(gameItem != null)
                {
                    gameItem.TotalTime = gameItem.TotalTime.Subtract(gameItem.PartialTime);
                    gameItem.PartialTime = new TimeSpan(0);
                    UpdateView(true);
                    TimeChanged(true);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Cascos
            // Audífono de los auriculares con micrófono (CORSAIR VOID ELITE USB Gaming Headset)
            SetAudioDevice(list.Find(d => d.Key == "cascos").Id); //  "2aeffc83-75fb-4581-868b-db51a053aab1");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Altavoces
            // Altavoces (Realtek(R) Audio)
            SetAudioDevice(list.Find(d => d.Key == "altavoces").Id); //"a5a0fb4c-6d0f-4b06-abf9-dbaef3b59b1a");
        }
    }
}
