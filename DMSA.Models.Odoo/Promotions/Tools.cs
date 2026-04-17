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

        public static void SetPromotionData(sale_order_line order_line, List<PromotionEvalItem> listPromotionData)
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
        }

        [Obsolete("Ya no se usará ya que los códigos requeridos se generan al momento de almacenar en Odoo")]
        public static void SetPromotionDataGift(sale_order_line order_line, List<PromotionEvalItem> listPromotionData)
        {
            if (listPromotionData == null || !listPromotionData.Any())
                return;

            order_line.origin_gift_line_ids = new int[] { };
            if (listPromotionData.Count > 0)
            {
                //if (listPromotionData[0].Promotion._promotion_type_id == 4 && listPromotionData[0].Promotion._selection_type_id == 1)
                //{
                //    order_line.origin_gift_line_ids_offline =
                //        Newtonsoft.Json.JsonConvert.SerializeObject(
                //            listPromotionData[0]
                //                .RuleSet
                //                .Where(r => r.ProductSequenceApplyList != null)
                //                .SelectMany(r => r.ProductSequenceApplyList)
                //                .Where(p => p.product_id == order_line.product_id)
                //                .ToList()
                //        );
                //}
                //else
                //{                    
                    order_line.origin_gift_line_ids_offline =
                        Newtonsoft.Json.JsonConvert.SerializeObject(
                            listPromotionData
                                .Where(x => x.RuleSet != null)
                                .SelectMany(x => x.RuleSet)
                                .Where(r => r.ProductSequenceApplyList != null)
                                .SelectMany(r => r.ProductSequenceApplyList)
                                .Distinct()
                                .ToList()
                        );                        
                //}
            }
        }
    }
}
