using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.DBApi.ExtraEntities
{
    public class GLBaseGeneralList
    {
        public int IdGame { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Historic { get; set; }
    }
}
