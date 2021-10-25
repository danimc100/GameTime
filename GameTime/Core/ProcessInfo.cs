using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.Core
{
    public class ProcessInfo : Object
    {
        public string ProcessName { get; set; }
        public string ProcessTitle { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", ProcessTitle, ProcessName);
        }
    }
}
