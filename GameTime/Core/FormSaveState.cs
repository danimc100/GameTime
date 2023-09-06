using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace GameTime.Core
{
    public class FormSaveState
    {
        private Form _form;
        private FormState state = null;

        private string GetFilename
        {
            get
            {
                string path = Path.GetDirectoryName(Application.ExecutablePath);
                string fileName = Path.Combine(path, $"form_{_form.Name}_state.json");
                return fileName;
            }
        }

        public FormSaveState(Form form) 
        {
            _form = form;
            if(_form != null) 
            {
                _form.Load += FrmSet_Load;
                _form.FormClosing += FrmSet_FormClosing;    
            }
        }

        private void FrmSet_Load(object sender, EventArgs e) 
        {
            LoadSettings();
            if(state != null)
            {
                state.SetState(_form);
            }
        }

        private void FrmSet_FormClosing(object sender, FormClosingEventArgs e) 
        {
            state = new FormState(_form);
            SaveSettings();
        }

        private void LoadSettings()
        {
            if(File.Exists(GetFilename)) 
            {
                state = JsonSerializer.Deserialize<FormState>(File.ReadAllText(GetFilename));
            }
            else
            {
                state = null;
            }
        }

        private void SaveSettings()
        {
            if(state != null) 
            {
                File.WriteAllText(GetFilename, JsonSerializer.Serialize(state));
            }
        }
    }
}
