using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
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

        public async Task<IEnumerable<PromotionBenefit>> SearchAll(int companyId, DateTime nowUtc)
        {
            await Init();

            var query = Database.Table<PromotionBenefit>()
                                .Where(x => x._company_id == companyId &&
                                            x.start_datetime <= nowUtc &&
                                            x.end_datetime >= nowUtc &&
                                            x._target_segment_id == 1 && //Solo fuerza de ventas
                                            x.active &&
                                            x.state == "authorized")
                                .OrderBy(x => x.create_date);

            var result = await query.ToListAsync();

            return result;
        }


    }
}
