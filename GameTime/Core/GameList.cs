﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;

namespace GameTime.Core
{
    public class GameList
    {
        private const string FILE_NAME = "GameList.data";

        private List<GameState> _list;

        public GameList()
        {
            _list = new List<GameState>();
            LoadData();
        }

        public List<GameState> List
        {
            get
            {
                return _list;
            }
        }

        public void CheckGames(TimeSpan elapsed)
        {
            var pList = GetProcessList();

            foreach (var g in List)
            {
                if (pList.Contains(g.Name))
                {
                    g.Active = true;
                    g.PartialTime = g.PartialTime.Add(elapsed);
                    g.TotalTime = g.TotalTime.Add(elapsed);
                }
                else
                {
                    g.Active = false;
                }
            }
        }

        public List<string> GetProcessList()
        {
            return System.Diagnostics.Process.GetProcesses()
                .Select(f => f.ProcessName)
                .OrderBy(f => f)
                .Distinct()
                .ToList();
        }

        public void SaveDate()
        {
            if (List.Count > 0)
            {
                var stream = File.CreateText(FILE_NAME);
                string data = System.Text.Json.JsonSerializer.Serialize(List);
                stream.WriteLine(data);
                stream.Close();
            }
        }

        public void LoadData()
        {
            if (System.IO.File.Exists(FILE_NAME))
            {
                var data = File.ReadAllText(FILE_NAME);
                if (!string.IsNullOrWhiteSpace(data))
                {
                    _list = JsonSerializer.Deserialize<List<GameState>>(data);
                }
            }
        }
    }
}
