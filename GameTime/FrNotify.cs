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
    public partial class FrNotify : Form
    {
        public string Title
        {
            set
            {
                label1.Text = value;
            }
        }

        public string Time
        {
            set
            {
                label2.Text = value;    
            }
        }

        public FrNotify()
        {
            InitializeComponent();
        }

        public void SetLocationAndShow(Form reference)
        {
            Point position = new Point(
                reference.Location.X + (reference.Size.Width - Size.Width), 
                reference.Location.Y - Size.Height);
            Location = position;
            TopMost= true;
            Show();
        }

        private void FrNotify_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
