using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameTime
{
    public partial class FrTime : Form
    {
        public FrTime()
        {
            InitializeComponent();
        }

        public TimeSpan Time
        {
            get
            {
                return GetTime();
            }
            set
            {
                maskedTextBox1.Text = string.Format("{0:D2} / {1:D2}:{2:D2}:{3:D2}", value.Days, value.Hours, value.Minutes, value.Seconds);
            }
        }

        private TimeSpan GetTime()
        {
            string[] l1 = maskedTextBox1.Text.Split('/');
            string[] l2 = l1[1].Split(':');
            return new TimeSpan(
                Convert.ToInt32(l1[0]),
                Convert.ToInt32(l2[0]),
                Convert.ToInt32(l2[1]),
                Convert.ToInt32(l2[2])
                );
        }

        private void Accept()
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void CloseForm()
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void FrTime_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
