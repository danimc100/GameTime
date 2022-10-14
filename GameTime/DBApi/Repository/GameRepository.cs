using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Migrations;
using GameTime.DBApi;

namespace GameTime.DBApi.Repository
{
    public class GameRepository : GenericRepository
    {
        public GameRepository()
            : base()
        {

        }

        public Game Get(int idGame)
        {
            return DBContext.Game.Where(g => g.IdGame == idGame).FirstOrDefault();
        }

        public int Insert(Game game)
        {
            DBContext.Game.Add(game);
            DBContext.SaveChanges();
            return game.IdGame;
        }

        public void Edit(Game game)
        {
            DBContext.Game.AddOrUpdate(game);
            DBContext.SaveChanges();
        }

        public void Delete(Game game)
        {
            DBContext.Game.Remove(game);
            DBContext.SaveChanges();
        }
    }
}
