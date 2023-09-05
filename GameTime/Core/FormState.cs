using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameTime.Core
{
    public class FormState
    {
        public Point Location { get; set; }

        public FormState()
        { }
        
        public FormState(Form form) 
        {
            GetState(form);
        }  

        public void SetState(Form form)
        {
            form.Location = Location;
        }

        public void GetState(Form form)
        {
            Location = form.Location;   
        }
    }
}
