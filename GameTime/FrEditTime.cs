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
    public partial class FrEditTime : Form
    {
        public DateTime Inicio { get { return dateTimePicker1.Value; } set { dateTimePicker1.Value = value; } }
        public DateTime Fin { get { return dateTimePicker2.Value; } set { dateTimePicker2.Value = value; } }

        public FrEditTime()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
