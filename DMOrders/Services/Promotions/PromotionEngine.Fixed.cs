using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Promotions
{
    public partial class PromotionEngineLite
    {
        public async Task<ObservableCollection<PromotionEvalResultV2>> EvaluatePromotionsV2(sale_order saleOrder)
        {
            // Variables de contexto
            totalOrder = saleOrder.amount_total;

            ObservableCollection<PromotionEvalResultV2> AppliedPromotionResults = new ObservableCollection<PromotionEvalResultV2>();

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
                // fechas de vigencia de la cabecera (start_datetime / end_datetime)
                .Where(p =>
                    (p.start_datetime == null || p.start_datetime <= nowUtc) &&
                    (p.end_datetime == null || p.end_datetime >= nowUtc))
                .ToList();

            var results = new List<PromotionEvalItemV2>();
            
            foreach (var promo in candidates)
            {
                var FullRuleSet = new List<PromoRuleMatch>();
                
                int FullAllowedGifts = 0;
                var (inCenter, TotalTimesAllowed) = await CheckPromoCenterAsync(promo._centers_ids, saleOrder._pricelist_id);

                if (!inCenter)
                    continue;

                foreach (var lineItem in saleOrder.order_line)
                {
                    var line = (sale_order_line)lineItem[2];
                    var product_tmpl_id = line.product_tmpl_id;
                    var qty = (int)line.product_uom_qty;
                                       

                    var (resultRules, allowed_gifts) = await EvaluateLineByBenefit(
                        product_tmpl_id: product_tmpl_id,
                        orderLine: line,
                        qty: qty,
                        totalProductAmount: totalProductAmount,
                        totalOrder: totalOrder,
                        companyId: company_id,
                        pricelist_id: saleOrder._pricelist_id,
                        promo: promo,
                        TotalTimesAllowed
                        );

                    Debug.WriteLine("EvaluateLineByBenefit result:");
                    Debug.WriteLine(resultRules);

                    if(resultRules != null && resultRules.Any())
                    {
                        foreach (var rule in resultRules)
                        {
                            if (promo._promotion_type_id == 2) // es regalo
                            {

                            }

                            if (promo._promotion_type_id == 4) // es nxn
                            {

                            }

                            if (promo._promotion_type_id == 6) // es descuento
                            {

                            }

                            Debug.WriteLine($"Regla aplicada: {rule.id} - Promo: {promo.name} - Producto: {product_tmpl_id}");
                        }
                        FullRuleSet.AddRange(resultRules);
                        //FullTotalTimesAllowed += TotalTimesAllowed;
                        FullAllowedGifts += allowed_gifts;
                        Debug.WriteLine("Aplicar la promocion por beneficio");
                    }
                }

                if(!FullRuleSet.Any())
                    continue;

                var promoEvalItem = new PromotionEvalItemV2
                {
                    Promotion = promo,
                    RuleSet = FullRuleSet,
                    TotalTimesAllowed = TotalTimesAllowed,
                    PricelistId = saleOrder._pricelist_id,
                    MaxAllowedGifts = FullAllowedGifts
                };

                AppliedPromotionResults.Add(new PromotionEvalResultV2
                {
                    NowUtc = nowUtc,
                    Items = new List<PromotionEvalItemV2> { promoEvalItem }
                });
            }

            Debug.WriteLine("==============================");

            //foreach (var lineItem in saleOrder.order_line)
            //{
            //    var line = (sale_order_line) lineItem[2];

            //    if (line.product_tmpl_id == 0 && line.is_gift)
            //    {
            //        continue;
            //    }

            //    var product_tmpl_id = line.product_tmpl_id;

            //    if (product_tmpl_id == 0)
            //    {
            //        var product_template_id = await productDb.GetItem(line.product_id);
            //        product_tmpl_id = product_template_id._product_tmpl_id;
            //    }

            //    var qty = (int)line.product_uom_qty;
                
            //    totalProductAmount = line.price_total;

            //    // 4️⃣ Evaluar promociones
            //    var result = await EvaluateLine(
            //        product_tmpl_id: product_tmpl_id,
            //        orderLine: line,
            //        qty: qty,
            //        totalProductAmount: totalProductAmount,
            //        totalOrder: totalOrder,
            //        companyId: company_id,
            //        pricelist_id: saleOrder._pricelist_id
            //    );

            //    if (result.Best != null)
            //    {
            //        //Debug.WriteLine($"Promo aplicada: {result.Best.Promotion.Name} ({result.Best.Discount}%) al producto {product.name}");
            //        // Opcional: agregar a tu lista de promociones aplicadas
            //        //var benefit = (await repo.Search(company_id, DateTime.UtcNow))
            //        //                  .FirstOrDefault(p => p.id == result.Best.Promotion.id);

            //        //if (benefit != null)
            //        AppliedPromotionResults.Add(result);

            //        Debug.WriteLine("Aplicar la promocion automatica");
            //        foreach (var item in result.Items)
            //        {
            //            //Tipo automatico + bonificado
            //            if (item.Promotion._selection_type_id == 1 && item.Promotion._promotion_type_id == 2)
            //            {
            //                Debug.WriteLine(item.IsBest);
            //            }
            //        }
            //    }                
            //}

            return AppliedPromotionResults;
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

            this.qty = qty;
            this.totalProductAmount = totalProductAmount;
            this.totalOrder = totalOrder;

            var nowUtc = (dateUtc ?? DateTime.UtcNow).AddTicks(-(dateUtc ?? DateTime.UtcNow).Ticks % TimeSpan.TicksPerMinute);

            var results = new List<PromotionEvalItemV2>();
            var FullRuleSet = new List<PromoRuleMatch>();
                        
            var RuleSet = new List<PromoRuleMatch>();

            //var (inCenter, TotalTimesAllowed) = await CheckPromoCenterAsync(promo._centers_ids, pricelist_id);

            //Debug.WriteLine("inCenter");
            //Debug.WriteLine(inCenter);

            var baseReasons = new List<string>();
            baseReasons.Add("Promoción activa y dentro de vigencia.");

            // Si la promoción tiene detalles de productos explícitos:
            bool productMatches = true;

            List<int> fullProductDetails = new();

            foreach (var productIds in promo._product_promotion_ids)
            {
                var ids = JsonConvert.DeserializeObject<int[]>(productIds.general_product_id_json);
                if (ids != null)
                    fullProductDetails.AddRange(ids);
            }

            if (fullProductDetails.Any())
            {
                if (product_tmpl_id == 0)
                {
                    productMatches = false;
                }
                else
                {
                    productMatches = fullProductDetails.Contains(product_tmpl_id);
                }

                if (!productMatches) return (null,0);
                baseReasons.Add("Producto incluido en detalle de la promoción.");
            }

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

                    //cumple = OperatorEvaluator.Evaluate(r.operator_, variableValue, r.value, 0);

                    decimal value_for_eval = r.minimum_value;
                    decimal value_for_eval_max = r.maximum_value;
                    string operator_ = "";
                    (operator_, value_for_eval) = fixOperator(r.variable, value_for_eval);
                    cumple = OperatorEvaluator.Evaluate(operator_, variableValue, value_for_eval, value_for_eval_max);

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

                    //Si es manual
                    allowed_gifts = r.qty; //(int)Math.Floor((double)qty / r.value);

                    //Si es automático
                    //if (promo._selection_type_id == 1)
                    //{
                    //    // ej. 10 / 5 = 2 -> 2 regalos
                    //    int base_allowed_gifts = r.qty; // / r.value;
                    //    //Se realiza calculo de allowed_gifts segun r.qty y TotalTimesAllowed
                    //    // ya que en NxN los regalos dependen de la cantidad comprada
                    //    // y no es fijo como en bonificaciones
                    //    // ademas debe evaluarse segun TotalTimesAllowed                        
                    //    //allowed_gifts = r.qty;
                    //    if (base_allowed_gifts > TotalTimesAllowed)
                    //    {
                    //        allowed_gifts = TotalTimesAllowed * r.qty;
                    //    }
                    //    else
                    //        allowed_gifts = base_allowed_gifts * r.qty;
                    //}

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
                        ProductId = orderLine.product_id,
                        IsDiscount = promo._promotion_type_id == 6,
                        Discount = r.discount,
                        Reasons = reasons,
                        AllowedGifts = allowed_gifts,
                    };

                    RuleSet.Add(newRuleSet);
                    FullRuleSet.Add(newRuleSet);
                }

                if (!RuleSet.Any())
                    continue;

                //results.Add(new PromotionEvalItemV2
                //{
                //    Promotion = promo,
                //    RuleSet = RuleSet,
                //    TotalTimesAllowed = TotalTimesAllowed,
                //    PricelistId = pricelist_id,
                //    MaxAllowedGifts = allowed_gifts, //allowed_gifts * TotalTimesAllowed
                //});
            }

            return (RuleSet, allowed_gifts);
        }


        public async Task<PromotionEvalResultV2> EvaluateLine(
            int product_tmpl_id,
            sale_order_line orderLine,
            int qty,
            decimal totalProductAmount,
            decimal totalOrder,
            int companyId,
            int pricelist_id,
            DateTime? dateUtc = null
            )
        {
            if (qty <= 0)
                throw new ArgumentOutOfRangeException(nameof(qty), "qty debe ser > 0");

            this.qty = qty;
            this.totalProductAmount = totalProductAmount;
            this.totalOrder = totalOrder;

            var nowUtc = (dateUtc ?? DateTime.UtcNow).AddTicks(-(dateUtc ?? DateTime.UtcNow).Ticks % TimeSpan.TicksPerMinute);

            // 1) pedir candidatas al repo
            var candidates = (await _repo.Search(companyId, nowUtc))
                .Where(p => p != null)
                .Where(p => p.active)
                //.Where(p => string.Equals(p.state ?? string.Empty, "authorized", StringComparison.OrdinalIgnoreCase))
                // fechas de vigencia de la cabecera (start_datetime / end_datetime)
                .Where(p =>
                    (p.start_datetime == null || p.start_datetime <= nowUtc) &&
                    (p.end_datetime == null || p.end_datetime >= nowUtc))
                .ToList();

            var results = new List<PromotionEvalItemV2>();
            var FullRuleSet = new List<PromoRuleMatch>();

            foreach (var promo in candidates)
            {
                var RuleSet = new List<PromoRuleMatch>();

                var (inCenter, TotalTimesAllowed) = await CheckPromoCenterAsync(promo._centers_ids, pricelist_id);

                Debug.WriteLine("inCenter");
                Debug.WriteLine(inCenter);

                var baseReasons = new List<string>();
                baseReasons.Add("Promoción activa y dentro de vigencia.");

                // Si la promoción tiene detalles de productos explícitos:
                bool productMatches = true;

                List<int> fullProductDetails = new();

                foreach (var productIds in promo._product_promotion_ids)
                {
                    var ids = JsonConvert.DeserializeObject<int[]>(productIds.general_product_id_json);
                    if (ids != null)
                        fullProductDetails.AddRange(ids);
                }

                if (fullProductDetails.Any())
                {
                    if (product_tmpl_id == 0)
                    {
                        productMatches = false;
                    }
                    else
                    {
                        productMatches = fullProductDetails.Contains(product_tmpl_id);
                    }

                    if (!productMatches) continue;
                    baseReasons.Add("Producto incluido en detalle de la promoción.");
                }

                // Obtener reglas de la promoción (si existen)
                // En tus modelos originales pones promo._promotion_rules_ids -> en tu modelo devuelve empty list.
                // Aquí intentamos leer una propiedad dinámica que contenga reglas (si existe).
                // CAMBIAS POR LOS DEL DATO YA OBTENIDO TryExtractRules ya no es necesario
                List<PromoRules> rules = TryExtractRules(promo);

                // Si no hay reglas, tratamos la cabecera como posible (pero normalmente quieres reglas)
                if (rules == null || !rules.Any())
                {
                    // considerar la cabecera como aplicable sin reglas — añadimos un resultado simple
                    //results.Add(new PromotionEvalItem
                    //{
                    //    Promotion = promo,
                    //    Discount = 0,
                    //    Reasons = new List<string>(baseReasons) { "Promoción sin reglas explícitas (cabecera aplicable)." }
                    //});
                    continue;
                }

                int allowed_gifts = 0;
                // Evaluar reglas
                foreach (var r in rules.Where(rr => rr.state))
                {                   

                    bool cumple = false;

                    var reasons = new List<string>(baseReasons);
                    

                    // tiempo de la regla
                    if (!r.unlimited_time)
                    {
                        if (r.start_date.HasValue && nowUtc.Date < r.start_date.Value.Date) continue;
                        if (r.end_date.HasValue && nowUtc.Date > r.end_date.Value.Date) continue;
                        reasons.Add("Dentro de vigencia de la regla.");
                    }
                    else reasons.Add("Regla sin vigencia (unlimited_time).");

                    // cantidad mínima
                    //if (r.value.HasValue)
                    //{
                    //    if (qty < r.value.Value) continue;
                    //    reasons.Add($"Cumple cantidad mínima: {r.value.Value}");
                    //}

                    //1   BONIFICACION PARCIAL
                    //2   BONIFICACIONES
                    //3   CUPON
                    //4   N X N
                    //5   SORTEO
                    //6   DESCUENTOS
                    //7   FIDELIZACION
                    //('qty_product_unts', 'CANT. PRODUCTO (UNIDADES)'),
                    //('total_product_amount', 'TOTAL PRODUCTO (MONTO)'),
                    //('total_order', 'TOTAL PEDIDO')

                    //('less_than', '< (MENOR QUE)'),
                    //('greater_than', '> (MAYOR QUE)'),
                    //('less_than_or_equal', '<= (MENOR O IGUAL QUE)'),
                    //('greater_than_or_equal', '>= (MAYOR O IGUAL QUE)'),
                    //('equal_to', '= (IGUAL A)'),
                    //('not_equal_to', '<> (DISTINTO DE)')

                    if (promo._promotion_type_id == 2) // es regalo
                    {
                        decimal variableValue = GetVariableValue(r.variable);
                        cumple = OperatorEvaluator.Evaluate(r.operator_, variableValue, r.value, 0);

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

                        //Si es manual
                        allowed_gifts = r.qty; //(int)Math.Floor((double)qty / r.value);

                        //Si es automático
                        //if (promo._selection_type_id == 1)
                        //{
                        //    // ej. 10 / 5 = 2 -> 2 regalos
                        //    int base_allowed_gifts = r.qty; // / r.value;
                        //    //Se realiza calculo de allowed_gifts segun r.qty y TotalTimesAllowed
                        //    // ya que en NxN los regalos dependen de la cantidad comprada
                        //    // y no es fijo como en bonificaciones
                        //    // ademas debe evaluarse segun TotalTimesAllowed                        
                        //    //allowed_gifts = r.qty;
                        //    if (base_allowed_gifts > TotalTimesAllowed)
                        //    {
                        //        allowed_gifts = TotalTimesAllowed * r.qty;
                        //    }
                        //    else
                        //        allowed_gifts = base_allowed_gifts * r.qty;
                        //}

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

                        if (r.variable == "qty_product_unts")
                        {
                            r.operator_ = "between_included";
                            //r.operator_ = "between_or_greater_than"; 
                        }

                        if (r.variable == "total_product_amount")
                        {
                            r.operator_ = "greater_than_or_equal";
                            value_for_eval = totalProductAmount;
                        }

                        if (r.variable == "total_order")
                        {
                            r.operator_ = "greater_than_or_equal";
                            value_for_eval = totalOrder;
                        }

                        cumple = OperatorEvaluator.Evaluate(r.operator_, variableValue, value_for_eval, value_for_eval_max);

                        if (cumple)
                        {
                            reasons.Add($"Aplica descuento: {product_tmpl_id}, {r.discount} %");
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
                            ProductId = orderLine.product_id,
                            IsDiscount = promo._promotion_type_id == 6,
                            Discount = r.discount,
                            Reasons = reasons
                        };

                        RuleSet.Add(newRuleSet);
                        FullRuleSet.Add(newRuleSet);
                    }
                }

                if (!RuleSet.Any())
                    continue;

                results.Add(new PromotionEvalItemV2
                {
                    Promotion = promo,
                    RuleSet = RuleSet,
                    TotalTimesAllowed = TotalTimesAllowed,
                    PricelistId = pricelist_id,
                    MaxAllowedGifts = allowed_gifts, //allowed_gifts * TotalTimesAllowed
                });
            }

            return new PromotionEvalResultV2
            {
                NowUtc = nowUtc,
                Items = results
            };
        }
    }
}
