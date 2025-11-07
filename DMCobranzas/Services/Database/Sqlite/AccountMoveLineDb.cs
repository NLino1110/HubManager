using DMCobranzas.Models;
using DMCobranzas.Settings;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Services.Database.Sqlite
{
    public class AccountMoveLineDb
    {
        SQLiteAsyncConnection Database;

        public AccountMoveLineDb()
        {

        }

        public async Task<int> GetCount()
        {
            await Init();
            return await Database.Table<account_move_line>().CountAsync();
        }

        public async Task<int> Truncate()
        {
            await Init();
            
            return await Database.DeleteAllAsync<account_move_line>();
        }

        public async Task<account_move_line> GetItemByIdAsync(int id)
        {
            await Init();
            return await Database.Table<account_move_line>().Where(i=>i.id == id).FirstOrDefaultAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move_line>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<account_move_line>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
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
            account_Move_Parents.Any(p => p.id == i.moveId)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move_line>> GetItemsAsync(
            int productId,
            account_move[] account_Move_Parents,
            int limit)
        {
            await Init();
            var resultTmp = await Database.Table<account_move_line>().
                Where(i => i.productId == productId).ToListAsync();

            return resultTmp.Where(i =>
            account_Move_Parents.Any(p => p.id == i.moveId)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move_line>> GetItemsByParentAsync(int move_id)
        {
            await Init();
            return await Database.Table<account_move_line>().Where(x=>x.moveId == move_id).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<account_move_line> GetItem(int id)
        {
            await Init();
            return await Database.Table<account_move_line>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(account_move_line item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(account_move_line[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<account_move_line>();
        }
    }
}
