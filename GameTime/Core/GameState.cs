using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameTime.DBApi.Logic;
using GameTime.DBApi.ExtraEntities;
using System.Text.Json.Serialization;

namespace GameTime.Core
{
    public class GameState : GLGeneralList
    {
        public GameState()
        {
            IdGame = 0;
            Name = string.Empty;
            Title = string.Empty;
            PartialTime = new TimeSpan(0);
            TotalTime = new TimeSpan(0);
            Active = false;
            Modified = false;
            Created = DateTime.Now;
            NotifyForm = null;
            ActiveNum = 0;
        }
        public GameState(string name)
        {
            IdGame = 0;
            Name = name;
            Title = string.Empty;
            PartialTime = new TimeSpan(0);
            TotalTime = new TimeSpan(0);
            Active = false;
            Modified = false;
            Created = DateTime.Now;
            NotifyForm = null;
            ActiveNum = 0;
        }
        public GameState(GLGeneralList generalList)
        {
            IdGame = generalList.IdGame;
            Name = generalList.Name;
            Title = generalList.Title;
            PartialTime = new TimeSpan(0);
            TotalTime = generalList.TotalTime;
            Active = false;
            Modified = false;
            Created = DateTime.Now;
            NotifyForm = null;
            ActiveNum = 0;
        }

        [JsonIgnore]
        public DateTime StartedProcess { get; set; }
        
        [JsonIgnore]
        public TimeSpan PartialTime { get; set; }
        
        [JsonIgnore]
        public override TimeSpan TotalTime { get; set; }
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
        [JsonIgnore]
        public bool Active { get; set; }
        [JsonIgnore]
        public bool Modified { get; set; }
        public DateTime Created { get; set; }
        public FrNotify NotifyForm { get; set; }
        public int ActiveNum { get; set; }
    }
}
