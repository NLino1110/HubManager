using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMOrders.Services.Database.Sqlite
{
    public class PromotionBenefitDb: SqliteDbBase<PromotionBenefit>
    {
        public PromotionBenefitDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionBenefit>> GetItemsAsync(string state)
        {
            await Init();
            return await Database.Table<PromotionBenefit>().Where(x=> x.state == state).ToListAsync();
        }

        public async Task<PromotionBenefit> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionBenefit>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        internal async Task<IEnumerable<PromotionBenefit>> SearchAll(int companyId, DateTime nowUtc)
        {
            await Init();

            var query = Database.Table<PromotionBenefit>()
                                .Where(x => x._company_id == companyId &&
                                            x.start_datetime <= nowUtc &&
                                            x.end_datetime >= nowUtc &&
                                            x.active)
                                .OrderBy(x => x.create_date);

            var result = await query.ToListAsync();

            return result;
        }


    }
}
