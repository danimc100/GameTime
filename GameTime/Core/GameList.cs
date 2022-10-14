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
using GameTime.DBApi;
using GameTime.DBApi.Repository;
using GameTime.DBApi.Logic;

namespace GameTime.Core
{
    public class GameList
    {
        private const string FILE_NAME = "GameList.data";
        private const string FILE_NAME_HISTORIC = "GameListHistoric.data";
        private const string FILE_NAME_BKP = "GameList-{0}.data";
        private const string FILE_NAME_HISTORIC_BKP = "GameListHistoric-{0}.data";
        private const string BKP_DATETIME_FORMAT = "yyyyMMddhhmmss";
        private const string BACKUP_DIR = "backup";
        private const double DAYS_MANTAIN_BACKUP = -10;

        private List<GameState> _list;
        private List<GameState> _historic;
        private int sessionId;
        private List<ProcessItem> currentProcessLst;
        private bool anyActive;

        private GameRepository gameRep;
        private TimeRepository timeRep;
        private ReportsLogic reportsLg;

        public GameList()
        {
            _list = new List<GameState>();
            _historic = new List<GameState>();
            currentProcessLst = new List<ProcessItem>();
            sessionId = System.Diagnostics.Process.GetCurrentProcess().SessionId;
            anyActive = false;

            gameRep = new GameRepository();
            timeRep = new TimeRepository();
            reportsLg = new ReportsLogic();
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

        public bool AnyActive
        {
            get
            {
                return anyActive;
            }
        }

        public void CheckGames(TimeSpan elapsed)
        {
            var pList = GetProcessList();
            anyActive = false;

            foreach (var g in List)
            {
                if (pList.Contains(g.Name))
                {
                    if (!g.Active)
                    {
                        g.StartedProcess = DateTime.Now;
                        g.PartialTime = new TimeSpan();
                    }

                    g.Active = true;
                    g.Modified = true;
                    g.PartialTime = g.PartialTime.Add(elapsed);
                    g.TotalTime = g.TotalTime.Add(elapsed);
                    anyActive = true;
                }
                else
                {
                    if (g.Modified)
                    {
                        if (g.IdGame == 0)
                        {
                            // Guardamos el proceso en la base de datos
                            StoreGame(g, false);
                        }

                        // Guardamos registro en la base de datos.
                        timeRep.InsertTime(new Time 
                        {
                            IdGame = g.IdGame,
                            StartTime = g.StartedProcess,
                            EndTime = g.StartedProcess.AddTicks(g.PartialTime.Ticks)
                        });

                        g.Modified = false;
                    }
                    g.Active = false;
                }
            }
        }

        public List<string> GetProcessList()
        {
            return System.Diagnostics.Process.GetProcesses()
                .Where(p => p.SessionId == sessionId)
                .Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName != "explorer")
                .Select(f => f.ProcessName)
                .OrderBy(f => f)
                .Distinct()
                .ToList();
        }

        public List<ProcessInfo> GetProcessInfoList()
        {
            return System.Diagnostics.Process.GetProcesses()
                .Where(p => p.SessionId == sessionId)
                .Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName != "explorer")
                .Select(f => new ProcessInfo { ProcessName = f.ProcessName, ProcessTitle = f.MainWindowTitle })
                .OrderBy(f => f.ProcessName)
                .Distinct()
                .ToList();
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
                item.Historic = true;
                item.Active = false;
                UpdateGame(item);
                Delete(name, false);
            }
        }

        public void UpdateGame(GameState gameState)
        {
            if(gameState.IdGame != 0)
            {
                gameRep.Edit(GenGameEntity(gameState));
            }
        }

        public void Delete(string name, bool includeDadabase = true)
        {
            var item = Get(name);
            if (item != null)
            {
                List.Remove(item);
                if(item.IdGame != 0 && includeDadabase)
                {
                    timeRep.DeleteAllTime(item.IdGame);
                    gameRep.Delete(gameRep.Get(item.IdGame));
                }
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
            //SaveDataToFile();
        }

        public GameListResult LoadData()
        {
            //return LoadDataFromFile();

            LoadDataFromDB();
            return GameListResult.Ok;
        }

        private Game GenGameEntity(GameState gameState)
        {
            return new Game
            {
                IdGame = gameState.IdGame,
                Name = gameState.Name,
                Title = gameState.Title,
                Historic = gameState.Historic
            };
        }

        private void SaveDataToFile()
        {
            DateTime now = DateTime.Now;
            string bkpDatetimeFormat = now.ToString(BKP_DATETIME_FORMAT);

            if (List.Count > 0)
            {
                if (File.Exists(FILE_NAME))
                {
                    string dest = Path.Combine(BACKUP_DIR, string.Format(FILE_NAME_BKP, bkpDatetimeFormat));
                    File.Move(FILE_NAME, dest);
                }

                var stream = File.CreateText(FILE_NAME);
                string data = System.Text.Json.JsonSerializer.Serialize(List);
                stream.WriteLine(data);
                stream.Close();
            }

            if (Historic.Count > 0)
            {
                if (File.Exists(FILE_NAME_HISTORIC))
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

        private GameListResult LoadDataFromFile()
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
                    catch (Exception)
                    {
                        resultData = GameListResult.ErrorReadData;
                    }
                }
            }

            if (File.Exists(FILE_NAME_HISTORIC))
            {
                var data = File.ReadAllText(FILE_NAME_HISTORIC);
                if (!string.IsNullOrWhiteSpace(data))
                {
                    try
                    {
                        _historic = JsonSerializer.Deserialize<List<GameState>>(data);
                    }
                    catch (Exception)
                    {
                        resultDataHistoric = GameListResult.ErrorReadData;
                    }
                }
            }

            if (resultData == GameListResult.ErrorReadData || resultDataHistoric == GameListResult.ErrorReadData)
            {
                return GameListResult.ErrorReadData;
            }
            else
            {
                return GameListResult.Ok;
            }
        }

        private void LoadDataFromDB()
        {
            _list.Clear();
            var lst = reportsLg.GeneralReports(false);

            lst.ForEach(g =>
            {
                _list.Add(new GameState(g));
            });
        }

        private void DeleteOldBackupFiles()
        {
            string[] files = Directory.GetFiles(BACKUP_DIR);
            DateTime dateRef = DateTime.Now.AddDays(DAYS_MANTAIN_BACKUP);

            foreach (string file in files)
            {
                FileInfo fInfo = new FileInfo(file);
                if (fInfo.LastWriteTime < dateRef)
                {
                    fInfo.Delete();
                }
            }
        }

        private void StoreGame(GameState gameState, bool createInitialTime)
        {
            gameState.IdGame = gameRep.Insert(new Game
            {
                Name = gameState.Name,
                Title = gameState.Title,
                Historic = false
            });

            if (createInitialTime)
            {
                DateTime now = DateTime.Now;

                Time t = new Time
                {
                    IdGame = gameState.IdGame,
                    StartTime = now.AddTicks(gameState.TotalTime.Ticks * -1),
                    EndTime = now
                };

                timeRep.InsertTime(t);
            }
        }
    }
}
