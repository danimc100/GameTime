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
using AudioSwitcher.AudioApi.CoreAudio;
using GameTime.Core;
using GameTime.DBApi;
using GameTime.DBApi.ExtraEntities;
using GameTime.DBApi.Repository;
using GameTime.DBApi.Logic;

namespace GameTime
{
    public partial class FrMain : Form
    {
        private const int TIMER_INTERVAL = 1000;
        private const string AUDIODEVICECFG_NAME = "AudioDevices.cfg";
        private const int NO_CONTROLLERS = 4;

        private Controller[] controller;
        private int conSeleced;
        private Button[] btnCon;
        private GameList gameList;
        private bool lastAnyActive;
        private FrHistoric frHistoric;
        private FrPartials frPartials;
        private FrTime frTime;
        private CoreAudioController audio;
        private IEnumerable<CoreAudioDevice> audioDeviceList;

        public FrMain()
        {
            InitializeComponent();

            lastAnyActive = false;
            gameList = new GameList();
            if (gameList.LoadData() != Enum.GameListResult.Ok)
            {
                MessageBox.Show("Se ha producido un error al cargar la base de datos.");
            }

            listView1.SetDoubleBuffered(true);
            timer1.Interval = TIMER_INTERVAL;
            UpdateProcess();

            conSeleced = -1;
            int i = 0;
            controller = new Controller[NO_CONTROLLERS];
            foreach(UserIndex index in (UserIndex[]) System.Enum.GetValues(typeof(UserIndex)))
            {
                if(index != UserIndex.Any)
                {
                    controller[i] = new Controller(index);
                    i++;
                }
            }

            label4.Text = "-";

            btnCon = new Button[NO_CONTROLLERS];
            btnCon[0] = button8;
            btnCon[1] = button9;
            btnCon[2] = button10;
            btnCon[3] = button11;

            frHistoric = null;
            frPartials = null;
            frTime = null;

            audio = null;
            audioDeviceList = null;
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
                        ShowNotify(game);
                    }
                    else
                    {
                        items[0].SubItems[4].Text = string.Empty;
                        CloseNotify(game);
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

        private void ShowNotify(GameState game)
        {
            if(game.NotifyForm == null)
            {
                game.NotifyForm = new FrNotify();
            }
            game.NotifyForm.Title = $"{game.Title} / {game.Name}";
            game.NotifyForm.Time = game.PartialTime.ToString();
            game.NotifyForm.SetLocationAndShow(this, game.ActiveNum);
        }

        private void CloseNotify(GameState game)
        {
            if(game.NotifyForm != null)
            {
                game.NotifyForm.Close();
                game.NotifyForm.Dispose();
                game.NotifyForm = null;
            }
        }

        private void UpdateProcess()
        {
            var pList = gameList.GetProcessInfoList();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(pList.ToArray());
        }

        #region Controller Funcitions

        private void UpdateController()
        {
            Controller con;

            for (int i=0; i<NO_CONTROLLERS; i++)
            {
                btnCon[i].Enabled = controller[i].IsConnected;
                if(controller[i].IsConnected)
                {
                    btnCon[i].Text = string.Format("M{0} - {1}", i + 1, controller[i].GetBatteryInformation(BatteryDeviceType.Gamepad).BatteryType.ToString());
                }
                else
                {
                    btnCon[i].Text = "N/A";
                }
            }

            if(conSeleced == -1)
            {
                con = controller
                    .Where(c => c.IsConnected)
                    .Where(c => c.GetBatteryInformation(BatteryDeviceType.Gamepad).BatteryType == BatteryType.Alkaline).FirstOrDefault();

                if (con == null)
                {
                    con = controller
                        .Where(c => c.IsConnected)
                        .Where(c => c.GetBatteryInformation(BatteryDeviceType.Gamepad).BatteryType == BatteryType.Nimh).FirstOrDefault();
                }

                if (con == null)
                {
                    con = controller
                        .Where(c => c.IsConnected).FirstOrDefault();
                }
            }
            else
            {
                con = controller[conSeleced];
            }

            if (con != null && con.IsConnected)
            {
                label2.Text = "Conectado";
                BatteryInformation bi = con.GetBatteryInformation(BatteryDeviceType.Gamepad);
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

        #endregion

        private void ShowHistoric()
        {
            if (frHistoric == null)
            {
                frHistoric = new FrHistoric();
            }

            frHistoric.Show();
        }

        private TimeSpan? ShowFrTime(TimeSpan time)
        {
            if (frTime == null)
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
            if (WindowState == FormWindowState.Normal)
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
            SaveWindowState();
        }

        #region Audio Devices Functions

        private IEnumerable<CoreAudioDevice> GetAudioDevices()
        {
            if (audio == null)
            {
                audio = new CoreAudioController();
                audioDeviceList = audio.GetDevices();
            }
            return audioDeviceList;
        }

        private CoreAudioDevice GetAudioDevice(string id)
        {
            if(audio != null)
            {
                Guid guidDev = new Guid(id);
                return audio.GetDevice(guidDev);
            }
            else
            {
                return null;
            }
        }

        private void SetAudioDevice(string id)
        {
            CoreAudioDevice dev = GetAudioDevice(id);
            if (dev != null)
            {
                audio.SetDefaultDevice(dev);
                label4.Text = dev.FullName;
            }
            else
            {
                MessageBox.Show("No se ha encontrado el dispositivo de audio.");
            }
        }

        #endregion

        #region Events funtions

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

            if (anyActive != lastAnyActive && !anyActive)
            {
                SaveGameListAndWindowStatus();
            }
            lastAnyActive = anyActive;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateProcess();
        }

        private void editarTítuloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
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
                            gameList.UpdateGame(item);
                        }
                    });
                }
            }
        }

        private void eliminarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
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
                }
            }
        }

        private void guardarEnHistóricoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string name = listView1.SelectedItems[0].Name;
                gameList.Historify(name);
                UpdateView(true);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowHistoric();
        }

        private void FrMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
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

        private void button5_Click(object sender, EventArgs e)
        {
            // Cascos
            // Audífono de los auriculares con micrófono (CORSAIR VOID ELITE USB Gaming Headset)
            //SetAudioDevice(list.Find(d => d.Key == "cascos").Id); //  "2aeffc83-75fb-4581-868b-db51a053aab1");

            // {{ Name = Audífono de los auriculares con micrófono, InterfaceName = CORSAIR VOID ELITE USB Gaming Headset }}
            // {{ Name = Headset Earphone, InterfaceName = Poly Blackwire 3320 Series }}
            // {{ Name = Headset Microphone, InterfaceName = Poly Blackwire 3320 Series }}

            var lst = GetAudioDevices();
            var device = lst
                //.Where(d => d.Name.Contains("Audífono de los auriculares con micrófono") && d.InterfaceName.Contains("CORSAIR VOID ELITE USB Gaming Headset"))
                .Where(d => d.Name.Contains("Headset Earphone") && d.InterfaceName.Contains("Poly Blackwire 3320 Series"))
                .FirstOrDefault();

            if (device != null)
            {
                SetAudioDevice(device.Id.ToString());
            }
            else
            {
                MessageBox.Show("Dispositivo no encontrado.");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Altavoces
            // Altavoces (Realtek(R) Audio)
            //SetAudioDevice(list.Find(d => d.Key == "altavoces").Id); //"a5a0fb4c-6d0f-4b06-abf9-dbaef3b59b1a");

            // {{ Name = Altavoces, InterfaceName = Realtek(R) Audio }}
            var lst = GetAudioDevices();
            var device = lst
                .Where(d => d.Name.Contains("Altavoces") && d.InterfaceName.Contains("Realtek(R) Audio"))
                .FirstOrDefault();

            if (device != null)
            {
                SetAudioDevice(device.Id.ToString());
            }
            else
            {
                MessageBox.Show("Dispositivo no encontrado.");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Cascos
            // Audífono de los auriculares con micrófono (CORSAIR VOID ELITE USB Gaming Headset)
            //SetAudioDevice(list.Find(d => d.Key == "cascos").Id); //  "2aeffc83-75fb-4581-868b-db51a053aab1");

            // {{ Name = Audífono de los auriculares con micrófono, InterfaceName = CORSAIR VOID ELITE USB Gaming Headset }}
            // {{ Name = Headset Earphone, InterfaceName = Poly Blackwire 3320 Series }}
            // {{ Name = Headset Microphone, InterfaceName = Poly Blackwire 3320 Series }}

            var lst = GetAudioDevices();
            var device = lst
                .Where(d => d.Name.Contains("Audífono de los auriculares con micrófono") && d.InterfaceName.Contains("CORSAIR VOID ELITE USB Gaming Headset"))
                .FirstOrDefault();

            if (device != null)
            {
                SetAudioDevice(device.Id.ToString());
            }
            else
            {
                MessageBox.Show("Dispositivo no encontrado.");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            conSeleced = 0;
            UpdateController();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            conSeleced = 1;
            UpdateController();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            conSeleced = 2;
            UpdateController();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            conSeleced = 3;
            UpdateController();
        }

        private void verTiemposRegistradosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(frPartials == null)
            {
                frPartials = new FrPartials();
            }
            
            string name = listView1.SelectedItems[0].Name;
            GameState item = gameList.Get(name);

            frPartials.IdGame = item.IdGame;
            frPartials.Show();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if(gameList.AnyActive)
            {
                MessageBox.Show("Hay procesos de la lista activos, no se puede actualizar.", "! Antención !");
            }
            else
            {
                gameList.LoadData();
                UpdateView(true);
            }
        }

        #endregion
    }
}
