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
    /// <summary>
    /// Tools.v2.cs — UTILIDADES DE PROMOCIONES (mapear reglas y marcar líneas gift)
    /// ---------------------------------------------------------------------
    /// FromBenefitRule:
    ///   Convierte PromotionEvalItem + PromoRuleMatch + sale_order en un
    ///   PromoRuleItem usable por la UI / motor (incluye ProductSequenceApplyList
    ///   con product_id + sequence de las líneas padre que dispararon la promo).
    ///
    /// SetPromotionDataGift:
    ///   En una línea REGALO escribe origin_gift_line_ids_offline (JSON) con el
    ///   origen offline (product_id, sequence, promo_id, rule_id, total_allowed_gifts).
    ///   Odoo SaleOrderExtend usa ese JSON para buscar la línea padre con
    ///   (product_id, sequence) tras el create del pedido.
    ///   Guarda todos los ítems de ProductSequenceApplyList (deduplicados).
    ///
    /// SetPromotionData:
    ///   En una línea guarda promotion_data, promotion_ids y rule_ids.
    /// </summary>
    public partial class Tools
    {
        /// <summary>
        /// Mapea beneficio + regla + pedido a PromoRuleItem.
        /// ANTES: benefit.Promotion.* y SaleOrder._pricelist_id sin null-check → NRE
        ///   al preparar descuentos/regalos si faltaba Promotion o SaleOrder.
        /// DESPUÉS: retorna null si benefit/Promotion/rule inválidos; pricelist con ??.
        /// </summary>
        public static PromoRuleItem FromBenefitRule(PromotionEvalItem benefit, 
            PromoRuleMatch rule, 
            sale_order SaleOrder)
        {
            if (benefit?.Promotion == null || rule == null)
                return null;

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
                Reasons = rule.Reasons ?? new List<string>(),
                SequenceOrigin = rule.selection_type_id,
                start_date = rule.start_date,
                state = rule.state,
                type = rule.type,
                unlimited_time = rule.unlimited_time,
                value = rule.value,
                variable = rule.variable,
                MaxAllowedGifts = rule.AllowedGifts, //benefit.MaxAllowedGifts,
                TotalTimesAllowed = benefit.TotalTimesAllowed,
                promo_pricelist_id = SaleOrder?._pricelist_id ?? 0,
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

        /// <summary>
        /// Marca en la línea gift el origen offline (product_id + sequence del/los padre(s)).
        /// Serializa TODOS los ítems de ProductSequenceApplyList (no solo el primero),
        /// para que Odoo pueda matchear cada origen por (product_id, sequence).
        /// </summary>
        public static void SetPromotionDataGift(sale_order_line order_line, List<PromoRuleItem> promoRuleItems)
        {
            if (promoRuleItems == null || promoRuleItems.Count == 0)
                return;

            var allOrigins = promoRuleItems
                .Where(r => r?.ProductSequenceApplyList != null)
                .SelectMany(r => r.ProductSequenceApplyList)
                .Where(o => o != null)
                .GroupBy(o => new { o.product_id, o.sequence, o.promo_id, o.rule_id })
                .Select(g => g.First())
                .ToList();

            if (allOrigins.Count == 0)
                return;

            order_line.origin_gift_line_ids = Array.Empty<int>();

            order_line.origin_gift_line_ids_offline =
                Newtonsoft.Json.JsonConvert.SerializeObject(allOrigins);
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

        /// <summary>
        /// Guarda en la línea el detalle de reglas/promos aplicadas (promotion_data,
        /// promotion_ids, rule_ids) para UI y sync.
        /// </summary>
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

        /// <summary>
        /// Líneas padre del pedido (no regalo) desde order_line envuelto en OrderLineWrapper.
        /// </summary>
        public static IEnumerable<sale_order_line> FlattenParentOrderLines(
            IEnumerable<OrderLineWrapper>? orderLines)
        {
            if (orderLines == null)
                yield break;

            foreach (var row in orderLines)
            {
                if (row == null || row.Count <= 2)
                    continue;

                if (row[2] is sale_order_line line && !line.is_gift)
                    yield return line;
            }
        }

        /// <summary>
        /// Resuelve las líneas a las que aplica un descuento para un product_tmpl_id.
        /// Prioriza ProductSequenceApplyList (product_id + sequence); si no hay match,
        /// cae al comportamiento anterior (primera línea con ese template).
        /// </summary>
        public static List<sale_order_line> ResolveDiscountTargetLines(
            IEnumerable<OrderLineWrapper>? orderLines,
            int productTmplTarget,
            IEnumerable<OriginPromoOrderLine>? sequenceApplyList)
        {
            var flat = FlattenParentOrderLines(orderLines).ToList();
            var result = new List<sale_order_line>();

            if (sequenceApplyList != null)
            {
                foreach (var seq in sequenceApplyList.Where(s =>
                    s != null && s.product_tmpl_id == productTmplTarget))
                {
                    var line = flat.FirstOrDefault(l =>
                        l.product_id == seq.product_id && l.sequence == seq.sequence);

                    if (line != null && !result.Contains(line))
                        result.Add(line);
                }
            }

            if (result.Count > 0)
                return result;

            var fallback = flat.FirstOrDefault(l => l.product_tmpl_id == productTmplTarget);
            if (fallback != null)
                result.Add(fallback);

            return result;
        }
    }
}
