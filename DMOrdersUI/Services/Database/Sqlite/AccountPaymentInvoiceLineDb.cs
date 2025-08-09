using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Database.Sqlite
{
    public class AccountPaymentInvoiceLineDb
    {
        SQLiteAsyncConnection Database;

        public AccountPaymentInvoiceLineDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<AccountPaymentInvoiceLine>().ToListAsync()).Count;
        }

        public async Task<List<AccountPaymentInvoiceLine>> GetItemsAsync()
        {
            await Init();
            return (await Database.Table<AccountPaymentInvoiceLine>().ToListAsync());
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<AccountPaymentInvoiceLine>> GetItemsAsync(AccountPayment parent)
        {
            await Init();
            //return Database.GetAllWithChildren<AccountPaymentInvoiceLine>();
            return (await Database.Table<AccountPaymentInvoiceLine>().ToListAsync()).Where(pl=>pl.parent_payment_id == parent.id).ToList();
        }

        //public async Task<account_journal> GetItemAsync(int id)
        //{
        //    await Init();
        //    return await Database.Table<account_journal>().Where(i => i.id == id).FirstOrDefaultAsync();
        //}

        public async Task<int> DeleteItemOfParent(AccountPayment parent)
        {
            await Init();
            int count = 0;
            //await Init();
            var resultItems = (await Database.Table<AccountPaymentInvoiceLine>().ToListAsync()).Where(i => i.parent_payment_id == parent.id);
            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }

        public async Task<int> InsertAsync(AccountPaymentInvoiceLine item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(AccountPaymentInvoiceLine[] items)
        {
            await Init();
            await Database.InsertAllAsync(items);            
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<AccountPaymentInvoiceLine>();            
        }
    }
}
