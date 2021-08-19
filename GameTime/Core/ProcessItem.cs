using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.Core
{
    public class ProcessItem
    {
        public ProcessItem(string name)
        {
            Name = name;
            Inserted = DateTime.Now;
        }

        public string Name { get; set; }
        public DateTime Inserted { get; set; }
    }
}
