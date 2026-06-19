using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class PromotionProductDetailDb : SqliteDbBase<PromotionProductDetail>
    {


        public PromotionProductDetailDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_promotion_product_detail__promo_id ON promotion_product_detail(_promo_id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_promotion_product_detail__bonus_id ON promotion_product_detail(_bonus_id)");
            });
        }


        public async Task<List<PromotionProductDetail>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().ToListAsync();
        }

        public async Task<PromotionProductDetail> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<PromotionProductDetail>> GetItemsByParent(int parentId)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x => x._parent_id == parentId).ToListAsync();
        }

        public async Task<List<PromotionProductDetail>> GetItemsByPromo(int promoId)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x => x._promo_id == promoId).ToListAsync();
        }

        public async Task<List<PromotionProductDetail>> GetItemsByPromoRules(int promoId)
        {
            await Init();
            var rules = await Database.Table<PromoRules>().Where(x => x._promo_id == promoId).ToListAsync();
            var ruleIds = rules.Select(r => r.id).ToList();
            return await Database.Table<PromotionProductDetail>().Where(x => ruleIds.Contains(x._bonus_id)).ToListAsync();
        }

        public async Task<List<PromotionProductDetail>> GetDetailsFull(int promoId)
        {
            await Init();

            // 1) Obtener detalles directos por la promo
            var directItems = await Database.Table<PromotionProductDetail>()
                                            .Where(x => x._promo_id == promoId)
                                            .ToListAsync();

            // 2) Obtener reglas de la promo
            var rules = await Database.Table<PromoRules>()
                                      .Where(x => x._promo_id == promoId)
                                      .ToListAsync();

            List<PromotionProductDetail> ruleItems = new();

            if (rules.Any())
            {
                var ruleIds = rules.Select(r => r.id).ToList();

                // 3) Obtener detalles por reglas
                ruleItems = await Database.Table<PromotionProductDetail>()
                                          .Where(x => ruleIds.Contains(x._bonus_id))
                                          .ToListAsync();
            }

            // 4) Unificar ambas listas sin duplicados
            var fullList = directItems
                .Concat(ruleItems)
                .GroupBy(x => x.id)   // agrupa por ID único del detalle
                .Select(g => g.First())
                .ToList();

            return fullList;
        }
    }
}
