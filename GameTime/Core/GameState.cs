using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace GameTime.Core
{
    public class GameState
    {
        public GameState()
        {
        }

        public GameState(string name)
        {
            Name = name;
            Title = string.Empty;
            PartialTime = new TimeSpan(0);
            TotalTime = new TimeSpan(0);
            Active = false;
            Created = DateTime.Now;
        }

        public string Name { get; set; }

        public string Title { get; set; }

        [JsonIgnore]
        public TimeSpan PartialTime { get; set; }

        [JsonIgnore]
        public TimeSpan TotalTime { get; set; }
        public long TotalTimeTicks
        {
            get
            {
                return TotalTime.Ticks;
            }
            set
            {
                TotalTime = new TimeSpan(value);
            }
        }

        public bool Active { get; set; }

        public DateTime Created { get; set; }
    }
}
