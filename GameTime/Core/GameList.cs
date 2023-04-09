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
        private List<GameState> _list;
        private int sessionId;
        private List<ProcessItem> currentProcessLst;

        public GameList()
        {
            _list = new List<GameState>();
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

        public bool AnyActive
        {
            get
            {
                return List.Where(g => g.Active).Any();
            }
        }

        public void CheckGames(TimeSpan elapsed)
        {
            var pList = GetProcessList();
            int activeNum = 1;

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
                    g.ActiveNum = activeNum;
                    activeNum++;
                }
                else
                {
                    if (g.Modified)
                    {
                        if (g.IdGame == 0)
                        {
                            // Guardamos el proceso en la base de datos
                            UpdateGame(g);
                        }

                        // Guardamos registro en la base de datos.
                        using(var timeRep = new TimeRepository())
                        {
                            timeRep.InsertTime(new Time
                            {
                                IdGame = g.IdGame,
                                StartTime = g.StartedProcess,
                                EndTime = g.StartedProcess.AddTicks(g.PartialTime.Ticks)
                            });
                        }

                        g.Modified = false;
                    }
                    g.Active = false;
                    g.ActiveNum = 0;
                }
            }
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
            using(var gameRep = new GameRepository())
            {
                if (gameState.IdGame == 0)
                {
                    gameState.IdGame = gameRep.Insert(new Game
                    {
                        IdGame = 0,
                        Name = gameState.Name,
                        Title = gameState.Title,
                        Historic = false,
                    });
                }
                else
                {
                    gameRep.Edit(GenGameEntity(gameState));
                }
            }
        }

        public void Delete(string name, bool includeDadabase = true)
        {
            using(var gameRep = new GameRepository())
            {
                using(var timeRep = new TimeRepository())
                {
                    var item = Get(name);
                    if (item != null)
                    {
                        List.Remove(item);
                        if (item.IdGame != 0 && includeDadabase)
                        {
                            timeRep.DeleteAllTime(item.IdGame);
                            gameRep.Delete(gameRep.Get(item.IdGame));
                        }
                    }
                }
            }
        }

        public GameListResult LoadData()
        {
            LoadDataFromDB();
            return GameListResult.Ok;
        }

        private List<string> GetProcessList()
        {
            return System.Diagnostics.Process.GetProcesses()
                .Where(p => p.SessionId == sessionId)
                .Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName != "explorer")
                .Select(f => f.ProcessName)
                .OrderBy(f => f)
                .Distinct()
                .ToList();
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

        private void LoadDataFromDB()
        {
            _list.Clear();

            using(var reportsLg = new ReportsLogic())
            {
                var lst = reportsLg.GeneralReports(false);

                lst.ForEach(g =>
                {
                    _list.Add(new GameState(g));
                });
            }
        }
    }
}
