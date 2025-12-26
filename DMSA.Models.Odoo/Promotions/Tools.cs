using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Promotions
{
    public class Tools
    {
        public static void ClearPromotionData(sale_order_line order_line)
        {
            order_line.promotion_data = null;
            order_line.promotion_ids = Array.Empty<int>();
            order_line.rule_ids = Array.Empty<int>();
            order_line.origin_gift_line_ids = Array.Empty<int>();
        }

        public static void SetPromotionData(sale_order_line order_line, List<PromotionEvalItemV2> listPromotionData)
        {
            if (listPromotionData == null || !listPromotionData.Any())
                return;

            order_line.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                    listPromotionData
                                );

            order_line.promotion_ids = listPromotionData
                .Where(x => x.Promotion != null)
                .Select(x => x.Promotion.id)
                .Distinct()
                .ToArray();

            order_line.rule_ids = listPromotionData
                   .Where(x => x.RuleSet != null)
                   .SelectMany(x => x.RuleSet)
                   .Select(r => r.id)
                   .Distinct()
                   .ToArray();

            //    order_line.origin_gift_line_ids = listPromotionData
            //.Where(x => x.RuleSet != null)
            //.SelectMany(x => x.RuleSet)
            //.Where(r => r.AllowedGifts != null)
            //.SelectMany(r => r.AllowedGifts)
            //.Distinct()
            //.ToArray();
        }

        public static void SetPromotionDataGift(sale_order_line order_line, List<PromotionEvalItemV2> listPromotionData)
        {
            if (listPromotionData == null || !listPromotionData.Any())
                return;

            order_line.origin_gift_line_ids = listPromotionData
                .Where(x => x.RuleSet != null)
                .SelectMany(x => x.RuleSet)
                .Where(r => !string.IsNullOrWhiteSpace(r.ProductTmplIds))
                .SelectMany(r =>
                {
                    try
                    {
                        return Newtonsoft.Json.JsonConvert
                            .DeserializeObject<int[]>(r.ProductTmplIds)
                            ?? Array.Empty<int>();
                    }
                    catch
                    {
                        // Si viene mal formado el JSON, no rompe todo
                        return Array.Empty<int>();
                    }
                })
                .Distinct()
                .ToArray();
        }
    }
}
