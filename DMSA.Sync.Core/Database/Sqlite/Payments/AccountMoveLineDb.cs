using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountMoveLineDb : SqliteDbBase<account_move_line>
    {
        public AccountMoveLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            Task.Run(async () =>
            {
                await InitializeAsync();
            });
        }

        public async Task InitializeAsync()
        {
            await Init();
                        
            await Database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__move_id ON account_move_line(_move_id)"
            );

            await Database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__product_id ON account_move_line(_product_id)"
            );

            await Database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__account_id ON account_move_line(_account_id)"
            );
                        
            await Database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__create_date ON account_move_line(create_date)"
            );

            await Database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__write_date ON account_move_line(write_date)"
            );
                        
            await Database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__display_type ON account_move_line(display_type)"
            );

            await Database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__move_id__display_type ON account_move_line(_move_id, display_type)"
            );
        }

        public async Task<List<account_move_line>> GetItemsAsync(
            string name, 
            string display_type, 
            account_move[] account_Move_Parents, 
            int limit)
        {
            await Init();
            var resultTmp = await Database.Table<account_move_line>().Where(i=>i.display_type == display_type &&
            i.name.ToLower().Contains(name.ToLower())
            ).ToListAsync();

            return resultTmp.Where(i=>
            account_Move_Parents.Any(p => p.id == i._move_id)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move_line>> GetItemsAsync(
            int productId,
            account_move[] account_Move_Parents,
            int limit)
        {
            await Init();
            var resultTmp = await Database.Table<account_move_line>().
                Where(i => i._product_id == productId).ToListAsync();

            return resultTmp.Where(i =>
            account_Move_Parents.Any(p => p.id == i._move_id)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move_line>> GetItemsByParentAsync(int move_id)
        {
            await Init();
            return await Database.Table<account_move_line>().Where(x=>x._move_id == move_id).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<account_move_line> GetItem(int id)
        {
            await Init();
            return await Database.Table<account_move_line>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertBatchAsync(account_move_line[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }
    }
}
