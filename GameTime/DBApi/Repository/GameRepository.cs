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

        public void Insert(Game game)
        {
            DBContext.Game.Add(game);
            DBContext.SaveChanges();
        }

        public void Edit(Game game)
        {
            DBContext.Game.AddOrUpdate(game);
        }

        public void Delete(Game game)
        {
            DBContext.Game.Remove(game);
        }
    }
}
