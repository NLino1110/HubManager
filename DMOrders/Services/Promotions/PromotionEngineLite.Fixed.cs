using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Services.Promotions
{
    public class RuleProductMatch
    {
        public int RuleId { get; set; }
        public int ProductTmplId { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
        public int AllowedGifts { get; set; }
    }

    public partial class PromotionEngineLite
    {
        public async Task<ObservableCollection<PromotionEvalResult>> EvaluatePromotions(sale_order saleOrder)
        {
            // Variables de contexto
            totalOrder = saleOrder.amount_total;

            ObservableCollection<PromotionEvalResult> AppliedPromotionResults = new ObservableCollection<PromotionEvalResult>();

            string dbNameSqlite = App.Session.odooConnection.DbNameSqlite;
            var productDb = new ProductProductDb(dbNameSqlite);

            var partner = saleOrder._partner_id;
            var company_id = saleOrder._company_id;
            DateTime? dateUtc = null;
            var nowUtc = (dateUtc ?? DateTime.UtcNow).AddTicks(-(dateUtc ?? DateTime.UtcNow).Ticks % TimeSpan.TicksPerMinute);

            var candidates = (await _repo.Search(company_id, nowUtc))
                .Where(p => p != null)
                .Where(p => p.active)
                //.Where(p => string.Equals(p.state ?? string.Empty, "authorized", StringComparison.OrdinalIgnoreCase))                
                .Where(p =>
                    (p.start_datetime == null || p.start_datetime <= nowUtc) &&
                    (p.end_datetime == null || p.end_datetime >= nowUtc))
                .ToList();

            var results = new List<PromotionEvalItem>();

            foreach (var promo in candidates)
            {
                var FullRuleSet = new List<PromoRuleMatch>();
                int timesForApply = 0;
                int FullAllowedGifts = 0;
                var (inCenter, TotalTimesAllowed) = await CheckPromoCenterAsync(promo._centers_ids, saleOrder._pricelist_id, promo);

                Debug.WriteLine("Promo " + promo.name);

                if (!inCenter)
                    continue;

                var (rulesApply, allowed_gifts) = await EvaluateBenefit(saleOrder, company_id, saleOrder._pricelist_id, promo, TotalTimesAllowed);

                if(rulesApply!= null && rulesApply.Any())
                {
                    FullRuleSet.AddRange(rulesApply);
                    FullAllowedGifts += allowed_gifts;
                }

                if (!FullRuleSet.Any())
                    continue;

                var promoEvalItem = new PromotionEvalItem
                {
                    Promotion = promo,
                    RuleSet = FullRuleSet,
                    TotalTimesAllowed = TotalTimesAllowed,
                    PricelistId = saleOrder._pricelist_id,
                    MaxAllowedGifts = FullAllowedGifts
                };

                AppliedPromotionResults.Add(new PromotionEvalResult
                {
                    NowUtc = nowUtc,
                    Items = new List<PromotionEvalItem> { promoEvalItem }
                });
            }

            return AppliedPromotionResults;
        }
        

        public async Task<(decimal TotalProductAmount, 
            int TotalQty, 
            List<int> ProductApplyList,
            List<OriginPromoOrderLine> ProductSequenceApplyList,
            sale_order_line productWithMaxValue,
            sale_order_line productWithMaxQty)>
                    CalculateValuesAsync(
                        List<PromotionProductDetail> productsApplyList,
                        List<sale_order_line> orderLines,
                        int TotalTimesAllowed
                    )
        {
            decimal totalAmount = 0;
            int totalQty = 0;
            var productFoundList = new List<int>();
            var productSequenceFoundList = new List<OriginPromoOrderLine>();

            if (productsApplyList == null || productsApplyList.Count == 0)
                return (0, 0, productFoundList, productSequenceFoundList, null, null);

            if (orderLines == null || orderLines.Count == 0)
                return (0, 0, productFoundList, productSequenceFoundList, null, null);

            // 1️⃣ Crear HashSet de product_ids de la promoción (más rápido)
            var productSet = new HashSet<int>(
                productsApplyList
                    .Where(x => x._product_id > 0)
                    .Select(x => x._product_id)
            );

            // 2️⃣ Recorrer líneas de pedido
            foreach (var line in orderLines)
            {
                int product_id = line.product_id;
                int tmplId = line.product_tmpl_id;
                int sequence = line.sequence;

                if (productSet.Contains(tmplId))
                {
                    int qty = (int)line.product_uom_qty;
                    decimal subtotal = line.price_subtotal;
                    decimal total = line.price_total;

                    totalQty += qty;
                    totalAmount += total; // subtotal;

                    productFoundList.Add(tmplId);
                    productSequenceFoundList.Add(new OriginPromoOrderLine {
                        sequence = sequence,
                        product_id = product_id,
                        product_tmpl_id = tmplId,
                    });
                }
            }

            var productWithMaxValue = orderLines
                .Where(x => productFoundList.Contains(x.product_tmpl_id))
                .OrderByDescending(x => x.price_total)
                .FirstOrDefault();

            var productWithMaxQty = orderLines
                .Where(x => productFoundList.Contains(x.product_tmpl_id))
                .OrderByDescending(x => x.product_uom_qty)
                .FirstOrDefault();


            return (totalAmount, totalQty, productFoundList, productSequenceFoundList, productWithMaxValue, productWithMaxQty);
        }

        public async Task<(List<PromoRuleMatch>, int allowed_gifts)> EvaluateBenefit(            
            sale_order SaleOrder,
            int companyId,
            int pricelist_id,
            PromotionBenefit promo,
            int TotalTimesAllowed,
            DateTime? dateUtc = null
            )
        {
            int productIdParentMatch = 0;

            var nowUtc = (dateUtc ?? DateTime.UtcNow).AddTicks(-(dateUtc ?? DateTime.UtcNow).Ticks % TimeSpan.TicksPerMinute);

            var results = new List<PromotionEvalItem>();
            var RuleSet = new List<PromoRuleMatch>();
            var baseReasons = new List<string>();

            baseReasons.Add($"Promoción activa y dentro de vigencia {promo.name}");

            bool productMatches = false;

            var OrderProductList = SaleOrder.order_line
                .Where(li => li.Count > 2 && ((sale_order_line)li[2]).is_gift != true)                
                .Select(li => (sale_order_line)li[2])
                .ToList();

            var (TotalProductAmount, TotalQty, ProductApplyList, ProductSequenceApplyList, productWithMaxValue, productWithMaxQty) = await CalculateValuesAsync(
                productsApplyList: promo._product_details_promotion_ids_for_apply,
                orderLines: OrderProductList,
                TotalTimesAllowed: TotalTimesAllowed
                );

            if (ProductApplyList != null && ProductApplyList.Count > 0) productMatches = true;

            if (!productMatches) return (null, 0);

            this.qty = TotalQty;
            this.totalProductAmount = TotalProductAmount;

            baseReasons.Add("Productos incluido en detalle de la promoción.");

            List<PromoRules> rules = TryExtractRules(promo);

            // Si no hay reglas, tratamos la cabecera como posible (pero normalmente quieres reglas)
            if (rules == null || !rules.Any())
            {
                return (null, 0);
            }

            int allowed_gifts = 0;
            // Evaluar reglas
            foreach (var ruleItem in rules.Where(rr => rr.state))
            {
                bool cumple = false;

                var reasons = new List<string>(baseReasons);

                // tiempo de la regla
                if (!ruleItem.unlimited_time)
                {
                    if (ruleItem.start_date.HasValue && nowUtc.Date < ruleItem.start_date.Value.Date) continue;
                    if (ruleItem.end_date.HasValue && nowUtc.Date > ruleItem.end_date.Value.Date) continue;
                    reasons.Add("Dentro de vigencia de la regla.");
                }
                else reasons.Add("Regla sin vigencia (unlimited_time).");

                if (promo._promotion_type_id == 2) // es regalo
                {
                    decimal variableValue = GetVariableValue(ruleItem.variable);
                    string operator_ = "";
                    operator_ = ruleItem.operator_;

                    //MODO 1
                    cumple = OperatorEvaluator.Evaluate(ruleItem.operator_, variableValue, ruleItem.value, 0);

                    //MODO 2
                    if (ruleItem.minimum_value > 0)
                    {
                        decimal value_for_eval = ruleItem.value;
                        decimal value_for_eval_min = ruleItem.minimum_value;
                        decimal value_for_eval_max = ruleItem.maximum_value;
                        if (value_for_eval_max == 0)
                            value_for_eval_max = 1000000000;

                        //(operator_, value_for_eval_min) = fixOperator(r.variable, value_for_eval_min);
                        cumple = OperatorEvaluator.Evaluate(operator_, variableValue, value_for_eval_min, value_for_eval_max);
                    }

                    if (cumple)
                    {
                        reasons.Add($"Cumple {ruleItem.variable} {ruleItem.operator_} {ruleItem.value}");
                    }
                    else
                    {
                        reasons.Add($"No cumple {ruleItem.variable} {ruleItem.operator_} {ruleItem.value}");
                        continue;
                    }

                    if (ruleItem.minimum_value != 0 && variableValue < ruleItem.minimum_value)
                    {
                        reasons.Add($"No cumple mínimo: {ruleItem.minimum_value}");
                        continue;
                    }

                    if (ruleItem.maximum_value != 0 && variableValue > ruleItem.maximum_value)
                    {
                        reasons.Add($"No cumple máximo: {ruleItem.maximum_value}");
                        continue;
                    }

                    //Si es manual (quizas aqui se deba solo usar modo 1)
                    //MODO 1
                    allowed_gifts = ruleItem.qty; //(int)Math.Floor((double)qty / r.value);

                    if (ruleItem.variable == "total_product_amount")
                    {
                        //allowed_gifts = (int)Math.Floor(variableValue / r.value);
                        Debug.WriteLine(allowed_gifts);
                    }
                    else if(ruleItem.variable == "total_order")
                    {

                    }
                    else
                    {
                        //MODO 1 - AUTOMATICO
                        if (promo._selection_type_id == 1)
                        {
                            allowed_gifts = (int)Math.Floor((double)qty / ruleItem.value) * ruleItem.qty;
                        }

                        //MODO 2 - MANUAL
                        if (promo._selection_type_id == 2)
                        {
                            allowed_gifts = (int)Math.Floor((double)qty / ruleItem.value) * ruleItem.qty;
                        }
                    }
                }

                if (promo._promotion_type_id == 4) // es NXN
                {
                    decimal variableValue = GetVariableValue(ruleItem.variable);
                    cumple = OperatorEvaluator.Evaluate(ruleItem.operator_, variableValue, ruleItem.value, 0);

                    if (cumple)
                    {
                        reasons.Add($"Aplica NxN: {ruleItem.discount} %");
                    }
                    else
                    {
                        reasons.Add($"No cumple NxN {ruleItem.variable} {ruleItem.operator_} {ruleItem.value}");
                        continue;
                    }
                                        
                    int base_allowed_gifts = (int) variableValue / ruleItem.value;
                    
                    if (base_allowed_gifts > TotalTimesAllowed)
                    {
                        allowed_gifts = TotalTimesAllowed * ruleItem.qty;
                    }
                    else
                    {
                        allowed_gifts = base_allowed_gifts * ruleItem.qty;
                    }                        
                }

                if (promo._promotion_type_id == 6) // es descuento
                {
                    decimal variableValue = GetVariableValue(ruleItem.variable);
                    decimal value_for_eval = ruleItem.minimum_value;
                    decimal value_for_eval_max = ruleItem.maximum_value;
                    string operator_ = "";

                    (operator_, value_for_eval) = fixOperator(ruleItem.variable, value_for_eval);

                    cumple = OperatorEvaluator.Evaluate(operator_, variableValue, value_for_eval, value_for_eval_max);

                    if (cumple)
                    {
                        reasons.Add($"Aplica descuento: {ruleItem.discount} %");
                    }
                    else
                    {
                        reasons.Add($"No cumple {ruleItem.variable} {ruleItem.operator_} {ruleItem.value}");
                        continue;
                    }
                }

                // aquí podrías incluir chequeos de método de pago, selección, etc. si los pasas como parámetros.

                if (cumple)
                {

                    //foreach(var applyItem in ProductSequenceApplyList)
                    //{
                    //    applyItem.promo_id = ruleItem._promo_id;
                    //    applyItem.rule_id = ruleItem.id;
                    //    applyItem.total_allowed_gifts = allowed_gifts;
                    //}

                    var clonedList = ProductSequenceApplyList
                    .Select(x => new OriginPromoOrderLine()
                    {
                        sequence = x.sequence,
                        product_id = x.product_id,
                        product_tmpl_id = x.product_tmpl_id,                                           
                        promo_id = ruleItem._promo_id,
                        rule_id = ruleItem.id,
                        total_allowed_gifts = allowed_gifts,                        
                    })
                    .ToList();

                    var newRuleSet = new PromoRuleMatch
                    {
                        id = ruleItem.id,
                        promo_id = ruleItem._promo_id,
                        promotion_type_id = ruleItem._promotion_type_id,
                        product_id = ruleItem._product_id,
                        product_uom_id = ruleItem._product_uom_id,
                        selection_type_id = ruleItem._selection_type_id,
                        payment_method_id = ruleItem._payment_method_id,
                        raffle_template_id = ruleItem._raffle_template_id,
                        change_id = ruleItem._change_id,
                        general_grupor_tipo_id_json = ruleItem.general_grupor_tipo_id_json,
                        variable = ruleItem.variable,
                        operator_ = ruleItem.operator_,
                        value = ruleItem.value,
                        minimum_value = ruleItem.minimum_value,
                        maximum_value = ruleItem.maximum_value,
                        product_promotion = ruleItem.product_promotion,
                        code = ruleItem.code,
                        qty = ruleItem.qty,
                        is_fixed = ruleItem.is_fixed,
                        discount = ruleItem.discount,
                        discount_base = ruleItem.discount_base == 0 ? ruleItem.discount: ruleItem.discount_base,
                        count_products = ruleItem.count_products,
                        start_date = ruleItem.start_date,
                        end_date = ruleItem.end_date,
                        unlimited_time = ruleItem.unlimited_time,
                        state = ruleItem.state,
                        type = ruleItem.type,
                        ProductTmplId = 0,
                        ProductIdOrigin = 0,
                        IsDiscount = promo._promotion_type_id == 6,
                        //Discount = ruleItem.discount,
                        Reasons = reasons,
                        AllowedGifts = allowed_gifts,
                        productIdParentMatch = productIdParentMatch,
                        ProductTmplIds = JsonConvert.SerializeObject(ProductApplyList),
                        ProductSequenceApplyList = clonedList,
                        ProductTmplIdMaxTotal = productWithMaxValue != null ? productWithMaxValue.product_tmpl_id : 0,
                        ProductTmplIdMaxQty = productWithMaxQty != null ? productWithMaxQty.product_tmpl_id : 0
                    };

                    RuleSet.Add(newRuleSet);
                }

                if (!RuleSet.Any())
                    continue;
            }

            return (RuleSet, allowed_gifts);
        }

        public async Task<(List<PromoRuleMatch>, int allowed_gifts)> EvaluateLineByBenefit(
            int product_tmpl_id,
            sale_order_line orderLine,
            int qty,
            decimal totalProductAmount,
            decimal totalOrder,
            int companyId,
            int pricelist_id,
            PromotionBenefit promo,
            int TotalTimesAllowed,
            DateTime? dateUtc = null            
            )
        {
            if (qty <= 0)
                throw new ArgumentOutOfRangeException(nameof(qty), "qty debe ser > 0");

            int productIdParentMatch = 0;
            this.qty = qty;
            this.totalProductAmount = totalProductAmount;
            this.totalOrder = totalOrder;

            var nowUtc = (dateUtc ?? DateTime.UtcNow).AddTicks(-(dateUtc ?? DateTime.UtcNow).Ticks % TimeSpan.TicksPerMinute);

            var results = new List<PromotionEvalItem>();
            var FullRuleSet = new List<PromoRuleMatch>();
                        
            var RuleSet = new List<PromoRuleMatch>();

            var baseReasons = new List<string>();
            baseReasons.Add("Promoción activa y dentro de vigencia.");

            // Si la promoción tiene detalles de productos explícitos:
            bool productMatches = false;

            if (product_tmpl_id == 0)
            {                
                return (null, 0);
            }

            List<int> fullProductDetails = new();
            
            foreach (var productIds in promo._product_details_promotion_ids_for_apply)
            {
                if (productIds._product_id == product_tmpl_id)
                {
                    productMatches = true;
                    break;
                }
            }

            if (!productMatches) return (null, 0);
            baseReasons.Add("Producto incluido en detalle de la promoción.");
            
            List<PromoRules> rules = TryExtractRules(promo);

            // Si no hay reglas, tratamos la cabecera como posible (pero normalmente quieres reglas)
            if (rules == null || !rules.Any())
            {                
                return (null,0);
            }

            int allowed_gifts = 0;
            // Evaluar reglas
            foreach (var r in rules.Where(rr => rr.state))
            {
                bool cumple = false;

                var reasons = new List<string>(baseReasons);

                if(r.variable == "total_product_amount")
                {
                    reasons.Add("Producto existe en regla, monto debe acumularse.");
                    var newRuleSet = new PromoRuleMatch
                    {
                        id = r.id,
                        promo_id = r._promo_id,
                        promotion_type_id = r._promotion_type_id,
                        product_id = r._product_id,
                        product_uom_id = r._product_uom_id,
                        selection_type_id = r._selection_type_id,
                        payment_method_id = r._payment_method_id,
                        raffle_template_id = r._raffle_template_id,
                        change_id = r._change_id,
                        general_grupor_tipo_id_json = r.general_grupor_tipo_id_json,
                        variable = r.variable,
                        operator_ = r.operator_,
                        value = r.value,
                        minimum_value = r.minimum_value,
                        maximum_value = r.maximum_value,
                        product_promotion = r.product_promotion,
                        code = r.code,
                        qty = r.qty,
                        is_fixed = r.is_fixed,
                        discount = r.discount,
                        discount_base = r.discount_base,
                        count_products = r.count_products,
                        start_date = r.start_date,
                        end_date = r.end_date,
                        unlimited_time = r.unlimited_time,
                        state = r.state,
                        type = r.type,
                        ProductTmplId = product_tmpl_id,
                        ProductIdOrigin = orderLine.product_id,
                        IsDiscount = promo._promotion_type_id == 6,
                        //Discount = r.discount,
                        Reasons = reasons,
                        AllowedGifts = allowed_gifts,
                        productIdParentMatch = productIdParentMatch
                    };

                    RuleSet.Add(newRuleSet);
                    continue;
                }

                // tiempo de la regla
                if (!r.unlimited_time)
                {
                    if (r.start_date.HasValue && nowUtc.Date < r.start_date.Value.Date) continue;
                    if (r.end_date.HasValue && nowUtc.Date > r.end_date.Value.Date) continue;
                    reasons.Add("Dentro de vigencia de la regla.");
                }
                else reasons.Add("Regla sin vigencia (unlimited_time).");

                if (promo._promotion_type_id == 2) // es regalo
                {
                    decimal variableValue = GetVariableValue(r.variable);
                    string operator_ = "";
                    operator_ = r.operator_;

                    //MODO 1
                    cumple = OperatorEvaluator.Evaluate(r.operator_, variableValue, r.value, 0);

                    //MODO 2
                    //decimal value_for_eval = r.minimum_value;
                    //decimal value_for_eval_max = r.maximum_value;                    
                    //(operator_, value_for_eval) = fixOperator(r.variable, value_for_eval);
                    //cumple = OperatorEvaluator.Evaluate(operator_, variableValue, value_for_eval, value_for_eval_max);

                    if (cumple)
                    {
                        reasons.Add($"Cumple {r.variable} {r.operator_} {r.value}");
                    }
                    else
                    {
                        reasons.Add($"No cumple {r.variable} {r.operator_} {r.value}");
                        continue;
                    }

                    if (r.minimum_value != 0 && variableValue < r.minimum_value)
                    {
                        reasons.Add($"No cumple mínimo: {r.minimum_value}");
                        continue;
                    }

                    if (r.maximum_value != 0 && variableValue > r.maximum_value)
                    {
                        reasons.Add($"No cumple máximo: {r.maximum_value}");
                        continue;
                    }

                    //Si es manual (quizas aqui se deba solo usar modo 1)
                    //MODO 1
                    allowed_gifts = r.qty; //(int)Math.Floor((double)qty / r.value);

                    //MODO 2 - MANUAL
                    if (promo._selection_type_id == 2)
                        allowed_gifts = (int)Math.Floor((double)qty / r.value);
                }

                if (promo._promotion_type_id == 4) // es NXN
                {
                    //decimal variableValue = GetVariableValue(r.variable);
                    //decimal value_for_eval = r.minimum_value;
                    //decimal value_for_eval_max = r.maximum_value;

                    //if (r.variable == "qty_product_unts")
                    //{
                    //    r.operator_ = "between_included";
                    //}

                    //cumple = OperatorEvaluator.Evaluate(r.operator_, variableValue, value_for_eval, value_for_eval_max);

                    decimal variableValue = GetVariableValue(r.variable);
                    cumple = OperatorEvaluator.Evaluate(r.operator_, variableValue, r.value, 0);

                    if (cumple)
                    {
                        reasons.Add($"Aplica NxN: {product_tmpl_id}, {r.discount} %");
                    }
                    else
                    {
                        reasons.Add($"No cumple {r.variable} {r.operator_} {r.value}");
                        continue;
                    }

                    // ej. 10 / 5 = 2 -> 2 regalos
                    int base_allowed_gifts = (int)variableValue / r.value;
                    //Se realiza calculo de allowed_gifts segun r.qty y TotalTimesAllowed
                    // ya que en NxN los regalos dependen de la cantidad comprada
                    // y no es fijo como en bonificaciones
                    // ademas debe evaluarse segun TotalTimesAllowed                        
                    //allowed_gifts = r.qty;
                    if (base_allowed_gifts > TotalTimesAllowed)
                    {
                        allowed_gifts = TotalTimesAllowed * r.qty;
                    }
                    else
                        allowed_gifts = base_allowed_gifts * r.qty;
                }

                if (promo._promotion_type_id == 6) // es descuento
                {
                    decimal variableValue = GetVariableValue(r.variable);
                    decimal value_for_eval = r.minimum_value;
                    decimal value_for_eval_max = r.maximum_value;
                    string operator_ = "";

                    (operator_, value_for_eval) = fixOperator(r.variable, value_for_eval);

                    //if (r.variable == "qty_product_unts")
                    //{
                    //    r.operator_ = "between_included";
                    //    //r.operator_ = "between_or_greater_than"; 
                    //}

                    //if (r.variable == "total_product_amount")
                    //{
                    //    r.operator_ = "greater_than_or_equal";
                    //    value_for_eval = totalProductAmount;
                    //}

                    //if (r.variable == "total_order")
                    //{
                    //    r.operator_ = "greater_than_or_equal";
                    //    value_for_eval = totalOrder;
                    //}

                    cumple = OperatorEvaluator.Evaluate(operator_, variableValue, value_for_eval, value_for_eval_max);

                    if (cumple)
                    {
                        reasons.Add($"Aplica descuento: {product_tmpl_id}, {r.discount} %");
                    }
                    else
                    {
                        reasons.Add($"No cumple {r.variable} {r.operator_} {r.value}");
                        continue;
                    }
                }

                // aquí podrías incluir chequeos de método de pago, selección, etc. si los pasas como parámetros.

                if (cumple)
                {
                    bool existsDiscountPromo = FullRuleSet.Any(x => x.IsDiscount == true && x.ProductTmplId == product_tmpl_id);

                    if (existsDiscountPromo)
                    {
                        reasons.Add($"No se agregará {promo.name} porque ya se aplicó descuento previo");
                        Debug.WriteLine($"No se agregará {promo.name} porque ya se aplicó descuento previo");
                        continue;
                    }

                    // Si llegamos acá, la regla aplica:
                    //Se convierte r en un PromoRuleMatch para agregar al RuleSet
                    var newRuleSet = new PromoRuleMatch
                    {
                        id = r.id,
                        promo_id = r._promo_id,
                        promotion_type_id = r._promotion_type_id,
                        product_id = r._product_id,
                        product_uom_id = r._product_uom_id,
                        selection_type_id = r._selection_type_id,
                        payment_method_id = r._payment_method_id,
                        raffle_template_id = r._raffle_template_id,
                        change_id = r._change_id,
                        general_grupor_tipo_id_json = r.general_grupor_tipo_id_json,
                        variable = r.variable,
                        operator_ = r.operator_,
                        value = r.value,
                        minimum_value = r.minimum_value,
                        maximum_value = r.maximum_value,
                        product_promotion = r.product_promotion,
                        code = r.code,
                        qty = r.qty,
                        is_fixed = r.is_fixed,
                        discount = r.discount,
                        discount_base = r.discount_base,
                        count_products = r.count_products,
                        start_date = r.start_date,
                        end_date = r.end_date,
                        unlimited_time = r.unlimited_time,
                        state = r.state,
                        type = r.type,
                        ProductTmplId = product_tmpl_id,
                        ProductIdOrigin = orderLine.product_id,
                        IsDiscount = promo._promotion_type_id == 6,
                        //Discount = r.discount,
                        Reasons = reasons,
                        AllowedGifts = allowed_gifts,
                        productIdParentMatch = productIdParentMatch
                    };

                    RuleSet.Add(newRuleSet);
                    FullRuleSet.Add(newRuleSet);
                }

                if (!RuleSet.Any())
                    continue;

            }

            return (RuleSet, allowed_gifts);
        }
    }
}
