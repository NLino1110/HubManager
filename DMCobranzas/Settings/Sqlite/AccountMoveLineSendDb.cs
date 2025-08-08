using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Settings.Sqlite
{
    public class AccountMoveLineSendDb
    {
        SQLiteAsyncConnection Database;

        public AccountMoveLineSendDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<account_move_line_send>().CountAsync();
        }

        public async Task<List<account_move_line_send>> GetItemsAsync(account_move_send parent)
        {
            await Init();            
            return (await Database.Table<account_move_line_send>().ToListAsync()).Where(pl => pl.parent_move_id == parent.id).ToList();
        }

        public async Task<int> Truncate()
        {
            await Init();
            
            return await Database.DeleteAllAsync<account_move_line_send>();
        }

        public async Task<int> DeleteItemOfParent(account_move_send parent)
        {
            await Init();
            int count = 0;
            //await Init();
            var resultItems = (await Database.Table<account_move_line_send>().ToListAsync()).Where(i => i.parent_move_id == parent.id);
            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }

        public async Task<List<account_move_line_send>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<account_move_line_send>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<int> DeleteByParentAsync(int parent_id)
        {
            await Init();
            return await Database.Table<account_move_line_send>().DeleteAsync(x => x.parent_move_id == parent_id);
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move_line_send>> GetItemsByParentAsync(int parent_id)
        {
            await Init();
            return await Database.Table<account_move_line_send>().Where(x=>x.parent_move_id == parent_id).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<account_move_line_send> GetItem(int id)
        {
            await Init();
            return await Database.Table<account_move_line_send>().Where(i => i.line_id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(account_move_line_send item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(account_move_line_send[] items)
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
            var result = await Database.CreateTableAsync<account_move_line_send>();
        }
    }
}
