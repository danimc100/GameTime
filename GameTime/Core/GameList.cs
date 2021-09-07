using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using GameTime.Enum;

namespace GameTime.Core
{
    public class GameList
    {
        private const string FILE_NAME              = "GameList.data";
        private const string FILE_NAME_HISTORIC     = "GameListHistoric.data";
        private const string FILE_NAME_BKP          = "GameList-{0}.data";
        private const string FILE_NAME_HISTORIC_BKP = "GameListHistoric-{0}.data";
        private const string BKP_DATETIME_FORMAT    = "yyyyMMddhhmmss";
        private const string BACKUP_DIR             = "backup";
        private const double DAYS_MANTAIN_BACKUP    = -10;

        private List<GameState> _list;
        private List<GameState> _historic;
        private int sessionId;
        private List<ProcessItem> currentProcessLst;

        public GameList()
        {
            _list = new List<GameState>();
            _historic = new List<GameState>();
            currentProcessLst = new List<ProcessItem>();
            sessionId = System.Diagnostics.Process.GetCurrentProcess().SessionId;
        }

        public List<GameState> List
        {
            get
            {
                return _list;
            }
        }

        public List<GameState> Historic
        {
            get
            {
                return _historic;
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

        public List<string> GetProcessList(bool newFirst = false)
        {
            if(newFirst)
            {
                var pList = Process.GetProcesses().ToList();

                foreach (Process p in pList)
                {
                    if (!currentProcessLst.Exists(cpl => cpl.Name == p.ProcessName))
                    {
                        currentProcessLst.Add(new ProcessItem(p.ProcessName));
                    }
                }

                currentProcessLst = (from cuLst in currentProcessLst
                                     join p in pList on cuLst.Name equals p.ProcessName
                                     select cuLst).ToList();

                return currentProcessLst
                    .OrderByDescending(c => c.Inserted)
                    .ThenBy(c => c.Name)
                    .Select(c => c.Name)
                    .Distinct()
                    .ToList<string>();
            }
            else
            {
                return System.Diagnostics.Process.GetProcesses()
                    .Where(p => p.SessionId == sessionId)
                    .Select(f => f.ProcessName)
                    .OrderBy(f => f)
                    .Distinct()
                    .ToList();
            }
        }

        public bool Exists(string name)
        {
            return List.Where(i => i.Name == name).Any();
        }

        public void Add(string name)
        {
            List.Add(new GameState(name));
        }

        public GameState Get(string name)
        {
            return List.Where(i => i.Name == name).FirstOrDefault();
        }

        public GameState GetHistoric(string name)
        {
            return Historic.Where(i => i.Name == name).FirstOrDefault();
        }

        public void Historify(string name)
        {
            var item = Get(name);
            if (item != null)
            {
                Delete(name);
                item.Active = false;
                _historic.Add(item);
            }
        }

        public void Delete(string name)
        {
            var item = Get(name);
            if(item != null)
            {
                List.Remove(item);
            }
        }

        public void DeleteHistoric(string name)
        {
            var item = GetHistoric(name);
            if (item != null)
            {
                Historic.Remove(item);
            }
        }

        public void SaveDate()
        {
            DateTime now = DateTime.Now;
            string bkpDatetimeFormat = now.ToString(BKP_DATETIME_FORMAT);

            if (List.Count > 0)
            {
                if(File.Exists(FILE_NAME))
                {
                    string dest = Path.Combine(BACKUP_DIR, string.Format(FILE_NAME_BKP, bkpDatetimeFormat));
                    File.Move(FILE_NAME, dest);
                }
                
                var stream = File.CreateText(FILE_NAME);
                string data = System.Text.Json.JsonSerializer.Serialize(List);
                stream.WriteLine(data);
                stream.Close();
            }

            if(Historic.Count > 0)
            {
                if(File.Exists(FILE_NAME_HISTORIC))
                {
                    string dest = Path.Combine(BACKUP_DIR, string.Format(FILE_NAME_HISTORIC_BKP, bkpDatetimeFormat));
                    File.Move(FILE_NAME_HISTORIC, dest);
                }

                var stream = File.CreateText(FILE_NAME_HISTORIC);
                string data = System.Text.Json.JsonSerializer.Serialize(Historic);
                stream.WriteLine(data);
                stream.Close();
            }

            DeleteOldBackupFiles();
        }

        public GameListResult LoadData()
        {
            GameListResult resultData = GameListResult.Ok;
            GameListResult resultDataHistoric = GameListResult.Ok;

            if (File.Exists(FILE_NAME))
            {
                var data = File.ReadAllText(FILE_NAME);
                if (!string.IsNullOrWhiteSpace(data))
                {
                    try
                    {
                        _list = JsonSerializer.Deserialize<List<GameState>>(data);
                    }
                    catch(Exception)
                    {
                        resultData = GameListResult.ErrorReadData;
                    }
                }
            }

            if(File.Exists(FILE_NAME_HISTORIC))
            {
                var data = File.ReadAllText(FILE_NAME_HISTORIC);
                if(!string.IsNullOrWhiteSpace(data))
                {
                    try
                    {
                        _historic = JsonSerializer.Deserialize<List<GameState>>(data);
                    }
                    catch(Exception)
                    {
                        resultDataHistoric = GameListResult.ErrorReadData;
                    }
                }
            }

            if(resultData == GameListResult.ErrorReadData || resultDataHistoric == GameListResult.ErrorReadData)
            {
                return GameListResult.ErrorReadData;
            }
            else
            {
                return GameListResult.Ok;
            }
        }

        private void DeleteOldBackupFiles()
        {
            string[] files = Directory.GetFiles(BACKUP_DIR);
            DateTime dateRef = DateTime.Now.AddDays(DAYS_MANTAIN_BACKUP);
            
            foreach(string file in files)
            {
                FileInfo fInfo = new FileInfo(file);
                if(fInfo.LastWriteTime < dateRef)
                {
                    fInfo.Delete();
                }
            }
        }
    }
}
