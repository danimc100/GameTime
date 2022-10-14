using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.DBApi.ExtraEntities
{
    public class GLGeneralList
    {
        public GLGeneralList()
        {

        }

        public int IdGame { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public virtual TimeSpan TotalTime { get; set; }
        public bool Historic { get; set; }
    }
}
