using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.DBApi.Repository
{
    public class GenericRepository : IDisposable
    {
        private GameListEntities dbContext;

        protected GameListEntities DBContext
        {
            get
            {
                return dbContext;
            }
        }

        public GenericRepository()
        {
            dbContext = new GameListEntities();
        }

        public void Dispose()
        {
            if (dbContext != null)
            {
                dbContext.Dispose();
            }
        }
    }
}
