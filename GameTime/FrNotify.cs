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

        public void SetLocationAndShow(Form reference, int num)
        {
            Screen scRef = LocalizeScreen(reference);
            Point position;

            if(scRef != null)
            {
                position = new Point(
                    (scRef.Bounds.X + scRef.Bounds.Width) - Size.Width,
                    (scRef.Bounds.Y + 50 + num*Size.Height));
                //(scRef.Bounds.Y + scRef.Bounds.Height) - (Size.Height + 100));
            }
            else
            {
                position = new Point(
                    reference.Location.X + (reference.Size.Width - Size.Width),
                    reference.Location.Y - Size.Height);
            }

            if(!Visible)
            {
                TopMost = true;
                TopLevel = true;
                Show();
                Location = position;
            }
        }

        private Screen LocalizeScreen(Form form)
        {
            Screen result = null;

            foreach(var s in Screen.AllScreens)
            {
                if(form.Location.X >= s.Bounds.X && form.Location.X <= (s.Bounds.X + s.Bounds.Width) &&
                   form.Location.Y >= s.Bounds.Y && form.Location.Y <= (s.Bounds.Y + s.Bounds.Height))
                {
                    result = s;
                }
            }
            return result;
        }

        private void FrNotify_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }


    }
}
