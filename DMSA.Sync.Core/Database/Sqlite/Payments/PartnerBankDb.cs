using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class PartnerBankDb : SqliteDbBase<res_partner_bank>
    {
        public PartnerBankDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

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

        public async Task<List<res_partner_bank>> GetItemsAsync(int company_id, int partner_id)
        {
            //await Init();
            //return (await Database.Table<res_partner_bank>().Where(i=>i.PartnerId == partner_id).ToListAsync());

            await Init();
            //var result = await Database.Table<res_partner_bank>().Where(i => i.PartnerId == partner_id).ToListAsync();
            var result = await Database.Table<res_partner_bank>().Where(i => i._partner_id == partner_id).ToListAsync();
            var resultbank = await Database.Table<ResBank>().ToListAsync();

            foreach (var item in result)
            {
                item.bank_name = "";
                var BankItem = resultbank.Where(b => b.id == item._bank_id).FirstOrDefault();
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

            var resultbank = await Database.Table<ResBank>().ToListAsync();

            foreach (var item in result) 
            {
                item.bank_name = "";
                var BankItem = resultbank.Where( b=>b.id == item._bank_id) .FirstOrDefault();
                if (BankItem != null)
                {
                    item.bank_name = BankItem?.name;
                }
            }

            return (result.Where (i=>
            (
            i.acc_holder_name.ToLower().Contains(Search.ToLower()) ||
            i.acc_number.ToLower().Contains(Search.ToLower()) ||
            i.bank_name.ToLower().Contains(Search.ToLower())
            )).ToList());
        }

        public async Task<res_partner_bank> GetMatch (res_partner_bank objectMatch)
        {
            await Init();

            return (await Database.Table<res_partner_bank>().ToListAsync()).Where(x => 
                x._partner_id == objectMatch._partner_id &&
                x._bank_id == objectMatch._bank_id &&
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


        public async Task<int> UpdateAsync(res_partner_bank item, int OldId)
        {
            await Init();
            await Database.InsertAsync(item);
            
            item.id = OldId;
            return await Database.DeleteAsync(item);
            //return await Database.UpdateAsync(item);
        }
    }
}
