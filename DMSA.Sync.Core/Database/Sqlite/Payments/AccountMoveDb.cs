using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountMoveDb : SqliteDbBase<account_move>
    {         
        public AccountMoveDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        //public async Task<DateTime> GetLastDate()
        //{
        //    await Init();
        //    string Sql = "select MAX(invoice_date) invoice_date from account_move"; //" order by FECHAREGISTRO DESC";

        //    DateTime fd = await Database.ExecuteScalarAsync<DateTime>(Sql);

        //    return fd;
        //}


        //empresa.id, res_Partner.id, txtBusqueda.Text.ToUpper(), 25
        public async Task<List<account_move>> GetItemsAsync(int company_id, int res_partner_id, string Search, int limit)
        {
            await Init();

            if (!Search.Equals(string.Empty) && Search != "")
            {
                return await Database.Table<account_move>().Where(x =>
                    x._company_id == company_id &&
                    x._partner_id == res_partner_id &&
                    (x.move_type == "in_invoice" || x.move_type == "out_invoice") &&
                    x.name.Contains(Search)
                ).
                OrderByDescending(o => o.invoice_date).
                Take(limit).ToListAsync();
            }
            else
            {
                return await Database.Table<account_move>().Where(x =>
                    x._company_id == company_id &&
                    x._partner_id == res_partner_id &&
                    (x.move_type == "in_invoice" || x.move_type == "out_invoice")
                ).
                OrderByDescending(o => o.invoice_date).
                Take(limit).ToListAsync();
            }
        }

        public async Task<List<account_move>> GetItemsAsync(account_move_line[] lines, int company_id, int partner_id, int limit)
        {
            await Init();

            var previuResult = await Database.Table<account_move>().Where(i => i._company_id == company_id &&
            i._partner_id == partner_id).Take(limit).ToListAsync();

            return previuResult.Where(i =>
            lines.Any(p => p._move_id == i.id)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move>> GetItemsAsync(int company_id, int partner_id, int limit)
        {
            await Init();
            return await Database.Table<account_move>().Where(i => i._company_id == company_id &&
            i._partner_id == partner_id).Take(limit).ToListAsync();
        }

        public async Task<List<account_move>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<account_move>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move>> GetItemsByPartnerForPaymentAsync(res_partner res_Partner)
        {
            await Init();
            return await Database.Table<account_move>().Where(x=>
            x._partner_id == res_Partner.id &&
            x._company_id == res_Partner._company_id &&
            x.move_type == "out_invoice" && 
            x.amount_residual > 0).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move>> GetItemsByPartnerAndCompany(res_partner res_Partner, res_company res_Company)
        {
            await Init();
            return await Database.Table<account_move>().Where(x =>
            x._partner_id == res_Partner.id &&
            x._company_id == res_Company.id &&
            x.move_type == "out_invoice" &&
            x.amount_residual > 0).ToListAsync();            
        }

        //public async Task<List<account_move>> GetItemsByPartnerAsync(res_partner res_Partner)
        //{
        //    await Init();
        //    return await Database.Table<account_move>().Where(x =>
        //    x._partner_id == res_Partner.id &&
        //    x.move_type == "out_invoice" &&
        //    x.amount_residual > 0).ToListAsync();
        //    //return Database.Table<account_journal>().ToList();
        //}


        public async Task<account_move> GetByNameItem(string name_doc)
        {
            await Init();
            return await Database.Table<account_move>().Where(i => i.name == name_doc).FirstOrDefaultAsync();
        }    
    }
}
