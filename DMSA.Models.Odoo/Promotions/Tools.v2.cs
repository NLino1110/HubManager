using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Promotions
{
    public partial class Tools
    {
        public static PromoRuleItem FromBenefitRule(PromotionEvalItem benefit, 
            PromoRuleMatch rule, 
            sale_order SaleOrder)
        {
            string promo_type_name = "";

            if (benefit.Promotion._promotion_type_id == 1)
            {
                promo_type_name = "BONIFICACION PARCIAL";
            }
            if (benefit.Promotion._promotion_type_id == 2)
            {
                promo_type_name = "BONIFICACION";
            }
            if (benefit.Promotion._promotion_type_id == 3)
            {
                promo_type_name = "NXN";
            }
            if (benefit.Promotion._promotion_type_id == 6)
            {
                promo_type_name = "DESCUENTOS";
            }

            PromoRuleItem resultRuleItems = new PromoRuleItem();

            resultRuleItems = new PromoRuleItem()
            {
                AllowedGifts = rule.AllowedGifts,
                change_id = rule.change_id,
                code = rule.code,
                count_products = rule.count_products,
                discount = rule.discount,
                discount_base = rule.discount_base,
                end_date = rule.end_date,
                general_grupor_tipo_id_json = rule.general_grupor_tipo_id_json,
                id = rule.id,
                IsDiscount = rule.IsDiscount,
                is_fixed = rule.is_fixed,
                maximum_value = rule.maximum_value,
                minimum_value = rule.minimum_value,
                operator_ = rule.operator_,
                payment_method_id = rule.payment_method_id,
                ProductIdOrigin = rule.ProductIdOrigin,
                productIdParentMatch = rule.productIdParentMatch,
                ProductIds = rule.ProductIds,
                ProductSequenceApplyList = rule.ProductSequenceApplyList,
                ProductTmplId = rule.ProductTmplId,
                ProductTmplIdMaxQty = rule.ProductTmplIdMaxQty,
                ProductTmplIdMaxTotal = rule.ProductTmplIdMaxTotal,
                ProductTmplIds = rule.ProductTmplIds,
                product_id = rule.product_id,
                product_promotion = rule.product_promotion,
                product_uom_id = rule.product_uom_id,
                selection_type_id = benefit.Promotion._selection_type_id,
                promotion_type_id = benefit.Promotion._promotion_type_id,
                promo_code = benefit.Promotion.code,
                promo_id = rule.promo_id,
                promo_name = benefit.Promotion.name,
                promo_type_name = promo_type_name,
                promo_active = benefit.Promotion.active,
                qty = rule.qty,
                raffle_template_id = rule.raffle_template_id,
                Reasons = rule.Reasons,
                SequenceOrigin = rule.selection_type_id,
                start_date = rule.start_date,
                state = rule.state,
                type = rule.type,
                unlimited_time = rule.unlimited_time,
                value = rule.value,
                variable = rule.variable,
                MaxAllowedGifts = rule.AllowedGifts, //benefit.MaxAllowedGifts,
                TotalTimesAllowed = benefit.TotalTimesAllowed,
                promo_pricelist_id = SaleOrder._pricelist_id,
                GiftsForRemove = benefit.GiftsForRemove,
                product_details_promotion_ids = benefit.Promotion._product_details_promotion_ids,
                product_details_promotion_ids_for_apply = benefit.Promotion._product_details_promotion_ids_for_apply,
            };

            return resultRuleItems;
        }

        //////public static void SetPromotionDataGift(sale_order_line order_line, List<PromoRuleItem> listPromotionData)
        //////{
        //////    if (listPromotionData == null || !listPromotionData.Any())
        //////        return;

        //////    order_line.origin_gift_line_ids = new int[] { };
        //////    if (listPromotionData.Count > 0)
        //////    {                
        //////        order_line.origin_gift_line_ids_offline =
        //////            Newtonsoft.Json.JsonConvert.SerializeObject(
        //////                listPromotionData                            
        //////                    .Where(r => r.ProductSequenceApplyList != null)
        //////                    .SelectMany(r => r.ProductSequenceApplyList)
        //////                    .Distinct()
        //////                    .ToList()
        //////            );
        //////    }
        //////}

        public static void SetPromotionDataGift(sale_order_line order_line, List<PromoRuleItem> promoRuleItems)
        {
            var firstPSA = promoRuleItems?
                .FirstOrDefault()?
                .ProductSequenceApplyList?
                .FirstOrDefault();

            if (firstPSA == null)
                return;

            order_line.origin_gift_line_ids = Array.Empty<int>();

            order_line.origin_gift_line_ids_offline =
                Newtonsoft.Json.JsonConvert.SerializeObject(
                    new List<OriginPromoOrderLine> { firstPSA }
                );
        }

        //public static void SetPromotionDataGift(sale_order_line order_line, List<PromoRuleItem> promoRuleItems)
        //{
        //    if (promoRuleItems == null || !promoRuleItems.Any())
        //        return;

        //    order_line.origin_gift_line_ids = new int[] { };

        //    var firstItem = promoRuleItems.FirstOrDefault();

        //    if (firstItem.ProductSequenceApplyList == null || firstItem.ProductSequenceApplyList.Count == 0)
        //        return;
            
        //    var firstPSA = firstItem.ProductSequenceApplyList.FirstOrDefault();

        //    if (firstPSA != null)
        //    {
        //        order_line.origin_gift_line_ids_offline =
        //            Newtonsoft.Json.JsonConvert.SerializeObject(
        //                new List <OriginPromoOrderLine>  { firstPSA }
        //            );
        //    }
        //}

        public static void SetPromotionData(sale_order_line order_line, List<PromoRuleItem> listPromoRuleItem)
        {
            if (listPromoRuleItem == null || !listPromoRuleItem.Any())
                return;

            order_line.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                    listPromoRuleItem
                                );

            order_line.promotion_ids = listPromoRuleItem                
                .Select(x => x.promo_id)
                .Distinct()
                .ToArray();

            order_line.rule_ids = listPromoRuleItem                  
                   .Select(r => r.id)
                   .Distinct()
                   .ToArray();
        }             
    }
}
