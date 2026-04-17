using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class BankDb : SqliteDbBase<ResBank>
    {
        public BankDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<ResBank>> GetItemsBySearchAsync(int company_id, string TextSearch, int limit)
        {
            await Init();
            int findCode = 0;

            int.TryParse(TextSearch, out findCode);

            if (findCode > 0)
            {
                return await Database.Table<ResBank>().Where(y =>(
                y.id == findCode)
                ).Take(limit).ToListAsync();
            }
            else
            {
                if (TextSearch.Length < 3)
                {
                    //Sin texto para buscar
                    //await Database.Table<res_partner>().Where(x => x.id == -1).ToListAsync();
                    await Database.Table<ResBank>().Take(0).ToListAsync();
                }

                return await Database.Table<ResBank>().Where(y =>(
                y.id == findCode ||
                y.name.ToUpper().Contains(TextSearch))                
                ).Take(limit).ToListAsync();
            }
            //return Database.Table<account_journal>().ToList();
        }

        //public async Task<List<ResBank>> GetItemsAsync()
        //{
        //    await Init();
        //    return await Database.Table<ResBank>().ToListAsync();
        //}
    }
}
