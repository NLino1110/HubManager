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
    public class PartnerBankDb
    {
        SQLiteAsyncConnection Database;

        public PartnerBankDb()
        {

        }

        public async Task<int> GetCount()
        {
            await Init();
            return (await Database.Table<res_partner_bank>().ToListAsync()).Count;
        }

        public async Task<int> getNewId()
        {
            await Init();
            
            int resultInt = 0;

            var res_partner_banks = await Database.Table<res_partner_bank>().ToListAsync();
            if(res_partner_banks!= null && res_partner_banks.Any())
            {
                resultInt = res_partner_banks.Min(x => x.id) - 1;
            }

            if (resultInt >= 0)
                resultInt = -1;

            return resultInt;
        }

        public async Task<List<res_partner_bank>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_partner_bank>().ToListAsync();            
        }

        public async Task<List<res_partner_bank>> GetItemsAsync(int company_id, int partner_id)
        {
            //await Init();
            //return (await Database.Table<res_partner_bank>().Where(i=>i.PartnerId == partner_id).ToListAsync());

            await Init();
            //var result = await Database.Table<res_partner_bank>().Where(i => i.PartnerId == partner_id).ToListAsync();
            var result = await Database.Table<res_partner_bank>().Where(i => i.PartnerId == partner_id).ToListAsync();
            var resultbank = await Database.Table<Bank_Id>().ToListAsync();

            foreach (var item in result)
            {
                item.bank_name = "";
                var BankItem = resultbank.Where(b => b.id == item.BankId).FirstOrDefault();
                if (BankItem != null)
                {
                    item.bank_name = BankItem?.name;
                }
            }

            return result;
        }

        public async Task<List<res_partner_bank>> GetItemsAsync(
            int company_id, 
            int partner_id, 
            string Search)
        {
            await Init();
            //var result = await Database.Table<res_partner_bank>().Where(i => i.PartnerId == partner_id).ToListAsync();
            var result = await Database.Table<res_partner_bank>().ToListAsync();

            var resultbank = await Database.Table<Bank_Id>().ToListAsync();

            foreach (var item in result) 
            {
                item.bank_name = "";
                var BankItem = resultbank.Where( b=>b.id == item.BankId) .FirstOrDefault();
                if (BankItem != null)
                {
                    item.bank_name = BankItem?.name;
                }
            }

            return result.Where (i=>
            
            i.acc_holder_name.ToLower().Contains(Search.ToLower()) ||
            i.acc_number.ToLower().Contains(Search.ToLower()) ||
            i.bank_name.ToLower().Contains(Search.ToLower())
            ).ToList();
        }

        public async Task<res_partner_bank> GetItem(int id)
        {
            await Init();
            return (await Database.Table<res_partner_bank>().ToListAsync()).Where(x=>x.id == id).FirstOrDefault();
        }

        public async Task<res_partner_bank> GetMatch (res_partner_bank objectMatch)
        {
            await Init();

            return (await Database.Table<res_partner_bank>().ToListAsync()).Where(x => 
                x.PartnerId == objectMatch.PartnerId &&
                x.BankId == objectMatch.BankId &&
                x.acc_number == objectMatch.acc_number &&
                // x.acc_holder_name == objectMatch.acc_holder_name &&
                x.type_account == objectMatch.type_account &&
                x.use_bank_type == objectMatch.use_bank_type
                //&& x.allow_out_payment == objectMatch.allow_out_payment
                ).
                FirstOrDefault();
            //return Database.Find<res_partner_bank>(objectMatch);
            //return Database.Get<res_partner_bank>(objectMatch);
        }

        public async Task<int> InsertAsync(res_partner_bank item)
        {
            await Init();
            await Database.InsertAsync(item);
            //Database.Insert(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(res_partner_bank[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        public async Task<int> UpdateAsync(res_partner_bank item, int OldId)
        {
            await Init();
            await Database.InsertAsync(item);
            
            item.id = OldId;
            return await Database.DeleteAsync(item);
            //return await Database.UpdateAsync(item);
        }
        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<res_partner_bank>();            
        }

    }
}
