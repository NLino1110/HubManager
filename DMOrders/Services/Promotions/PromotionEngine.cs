using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DMOrders.Services.Promotions
{
    public static class OperatorEvaluator
    {
        private static readonly Dictionary<string, Func<decimal, decimal, decimal, bool>> _operators = new()
        {
            { "equal_to", (a, b, _) => a >= b },
            { "not_equal_to", (a, b, _) => a != b },
            { "less_than", (a, b, _) => a < b },
            { "greater_than", (a, b, _) => a > b },
            { "less_than_or_equal", (a, b, _) => a <= b },
            { "greater_than_or_equal", (a, b, _) => a >= b },
            { "between_included", (a, b, c) => a >= b && a <= c }, 
            { "between_excluded", (a, b, c) => a > b && a < c },
            { "between_or_greater_than", (a, b, c) => (a >= b && a <= c) || a > c }, 

            //between_included, between_excluded
            //SON CUSTOM, NO EXISTEN EN APLICACION ODOO
        };

        public static bool Evaluate(string op, decimal left, decimal right, decimal maximum)
        {
            if (_operators.TryGetValue(op, out var func))
                return func(left, right, maximum);


            throw new InvalidOperationException($"Operador no soportado: {op}");
        }
    }

    public class PromotionRepository : IPromotionRepository
    {
        public async Task<IEnumerable<PromotionBenefit>> Search(int companyId, DateTime nowUtc)
        {
            // Usas tu clase PromotionBenefitDb (ya implementada en tu base)
            var db = new PromotionBenefitDb(App.Session.odooConnection.DbNameSqlite);

            // Llamas al método que ya tienes para buscar promociones
            var promos = await db.SearchAll(companyId, nowUtc);

            var productPromoDb = new PromotionProductDb(App.Session.odooConnection.DbNameSqlite);
            var productDetailPromoDb = new PromotionProductDetailDb(App.Session.odooConnection.DbNameSqlite);
            var promoRulesDb = new PromoRulesDb(App.Session.odooConnection.DbNameSqlite);
            var promoCentersDb = new PromoCentersDb(App.Session.odooConnection.DbNameSqlite);

            // Asegura listas inicializadas
            foreach (var promo in promos)
            {
                promo._product_promotion_ids = await productPromoDb.GetItemsByPromo(promo.id);

                // 2️⃣ Cargar reglas de la promoción
                promo._promotion_rules_ids = await promoRulesDb.GetItemsByParent(promo.id);

                // 1️⃣ Cargar productos asociados a la promoción -- REGALOS
                //promo._product_details_promotion_ids = await productDetailPromoDb.GetDetailsFull(promo.id);
                promo._product_details_promotion_ids_for_apply = await productDetailPromoDb.GetItemsByPromo(promo.id);

                // 1️⃣-1 Cargar productos asociados a la promoción -- PRODUCTOS QUE DEBEN APLICAR
                promo._product_details_promotion_ids = await productDetailPromoDb.GetItemsByPromoRules(promo.id);

                // 3️⃣ Cargar centros asociados (si aplica)
                promo._centers_ids = await promoCentersDb.GetItemsByParent(promo.id);

                // 4️⃣ Cargar clientes excluidos (IDs)
                //promo.customers_excluded_ids_json = await promoDb.GetExcludedCustomers(promo.id);

                // 5️⃣ Normaliza campos comunes
                promo._company_id = promo._company_id > 0 ? promo._company_id : companyId;
                promo.start_datetime ??= DateTime.MinValue;
                promo.end_datetime ??= DateTime.MaxValue;
                promo.state ??= "authorized";
                promo.active = promo.active;
            }

            return promos;
        }
    }

    // Interfaz que debe implementar tu repo SQLite (devuelve promociones ya filtradas por company/fecha/cliente)
    public interface IPromotionRepository
    {
        Task<IEnumerable<PromotionBenefit>> Search(int companyId, DateTime nowUtc);
    }
        
    /// <summary>
    /// Motor ligero de promociones que trabaja con tus modelos existentes.
    /// Requiere un IPromotionRepository que devuelva promociones con sus detalles cargados.
    /// </summary>
    public sealed partial class PromotionEngineLite
    {
        private decimal qty;
        private decimal totalProductAmount;
        private decimal totalOrder;

        private readonly IPromotionRepository _repo;

        public PromotionEngineLite(IPromotionRepository repo) => _repo = repo;

        decimal GetVariableValue(string variableName)
        {
            return variableName switch
            {
                "qty_product_unts" => qty,
                "total_product_amount" => totalProductAmount,
                "total_order" => totalOrder,
                _ => throw new InvalidOperationException($"Variable no reconocida: {variableName}")
            };
        }

        (string operator_, decimal value_for_eval) fixOperator(string variable_name, decimal value_for_eval_default)
        {
            string operator_ = string.Empty;
            decimal value_for_eval = value_for_eval_default;            

            if (variable_name == "qty_product_unts")
            {
                operator_ = "between_included";
                //r.operator_ = "between_or_greater_than"; 
            }

            if (variable_name == "total_product_amount")
            {
                operator_ = "greater_than_or_equal";
                value_for_eval = totalProductAmount;
            }

            if (variable_name == "total_order")
            {
                operator_ = "greater_than_or_equal";
                value_for_eval = totalOrder;
            }

            return (operator_, value_for_eval);
        }

        //private async Task<bool> ExistsInPromoCenter(List<PromoCenters> centers, int pricelist_id)
        //{
        //    if(centers == null || centers.Count == 0)
        //        return true; // si no hay centros definidos, aplica a todos

        //    foreach(var center in centers)
        //    {
        //        if(!center.levels_ids_json.Equals(string.Empty))
        //        {                    
        //            int[] levels = JArray.Parse(center.levels_ids_json).ToObject<int[]>();                    
        //            if (levels.Contains(pricelist_id))
        //                return true;
        //        }
        //    }

        //    return false;
        //}

        //private async Task<int> TotalTimesPromoCenter(List<PromoCenters> centers, int pricelist_id)
        //{
        //    int TotalTimes = 0;
        //    if (centers == null || centers.Count == 0)
        //        return 0;

        //    foreach (var center in centers)
        //    {
        //        if (!center.levels_ids_json.Equals(string.Empty))
        //        {
        //            int[] levels = JArray.Parse(center.levels_ids_json).ToObject<int[]>();
        //            if (levels.Contains(pricelist_id))
        //                TotalTimes += center.times_inv;
        //        }
        //    }

        //    return TotalTimes;
        //}

        private async Task<(bool exists, int totalTimes)> CheckPromoCenterAsync( List<PromoCenters> centers, int pricelist_id)
        {
            bool exists = false;
            int totalTimes = 0;

            if (centers == null || centers.Count == 0)
                return (true, 0); // aplica a todos, y no suma tiempos

            foreach (var center in centers)
            {
                if (!string.IsNullOrEmpty(center.levels_ids_json))
                {
                    int[] levels = JArray.Parse(center.levels_ids_json).ToObject<int[]>();

                    if (levels.Contains(pricelist_id))
                    {
                        exists = true;
                        totalTimes += center.times_inv;
                    }
                }
            }

            return (exists, totalTimes);
        }

        [Obsolete("VErsio inestable")]
        /// <summary>
        /// Evalúa promociones aplicables para un producto + cantidad en el contexto dado.
        /// - product: objeto product_product (puede ser null si la evaluación es por pedido).
        /// - qty: cantidad (si aplica).
        /// - partner: cliente (opcional).
        /// - companyId: id de la compañía (obligatorio, se usa para filtrar en repo).
        /// - dateUtc: fecha a considerar (opcional).
        /// </summary>
        public async Task<PromotionEvalResult> EvaluatePromotions(
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

            var results = new List<PromotionEvalItem>();

            foreach (var promo in candidates)
            {

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

                // Evaluar reglas
                foreach (var r in rules.Where(rr => rr.state))
                {
                    bool cumple = false;

                    var reasons = new List<string>(baseReasons);
                    int allowed_gifts = 0;

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
                        int base_allowed_gifts =  (int) variableValue / r.value;
                        //Se realiza calculo de allowed_gifts segun r.qty y TotalTimesAllowed
                        // ya que en NxN los regalos dependen de la cantidad comprada
                        // y no es fijo como en bonificaciones
                        // ademas debe evaluarse segun TotalTimesAllowed                        
                        //allowed_gifts = r.qty;
                        if(base_allowed_gifts > TotalTimesAllowed)
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

                        if(cumple)
                        {
                            reasons.Add($"Aplica descuento: {product_tmpl_id}, {r.discount} %");
                        }
                    }

                    // aquí podrías incluir chequeos de método de pago, selección, etc. si los pasas como parámetros.

                    if (cumple)
                    {
                        bool existsDiscountPromo = results.Any(x => x.Promotion._promotion_type_id == 6 && x.ProductTmplId == product_tmpl_id);

                        if (existsDiscountPromo) {
                            reasons.Add($"No se agregará {promo.name} porque ya se aplicó descuento previo");
                            Debug.WriteLine($"No se agregará {promo.name} porque ya se aplicó descuento previo");
                            continue;
                        }

                        // Si llegamos acá, la regla aplica:
                        results.Add(new PromotionEvalItem
                        {
                            Promotion = promo,
                            RuleSet = r,
                            ProductTmplId = product_tmpl_id,
                            ProductId = orderLine.product_id,
                            Discount = r.discount,
                            Reasons = reasons,
                            TotalTimesAllowed= TotalTimesAllowed,
                            PricelistId = pricelist_id,
                            AllowedGifts = allowed_gifts,
                            MaxAllowedGifts = allowed_gifts * TotalTimesAllowed,
                        });
                    }
                }
            }

            return new PromotionEvalResult
            {
                NowUtc = nowUtc,
                Items = results
            };
        }

        /// <summary>
        /// Intenta extraer reglas desde el objeto PromotionBenefit:
        /// - si el modelo tiene una lista de reglas (por convención: _promotion_rules_ids o promotion_rules), la intenta mapear a PromoRule.
        /// - si no encuentra nada devuelve una lista vacía.
        /// </summary>
        private static List<PromoRules> TryExtractRules(PromotionBenefit promo)
        {
            // 1) si en tu PromotionBenefit ya tienes una propiedad _promotion_rules_ids que contenga objetos,
            //    conviértelo aquí. En tus modelos mostrabas _promotion_rules_ids que devolvía empty list; si en BD tienes otra tabla
            //    tu repository idealmente ya debería poblar las reglas directamente en algún campo custom en el objeto.
            try
            {
                // primer intento: reflexión para ver si existe una propiedad 'promotion_rules_ids' o '_promotion_rules_ids' con datos
                var t = promo.GetType();

                var prop = t.GetProperty("_promotion_rules_ids") ?? t.GetProperty("promotion_rules_ids") ?? null;
                if (prop == null) return new List<PromoRules>();

                var value = prop.GetValue(promo);
                if (value == null) return new List<PromoRules>();

                // si value es IEnumerable<PromoRule> ya: cast
                if (value is IEnumerable<PromoRules> listDirect) return listDirect.ToList();

                // si value is IEnumerable<object> intentar mapear dinámicamente
                if (value is System.Collections.IEnumerable enumerable)
                {
                    var outList = new List<PromoRules>();
                    foreach (var it in enumerable)
                    {
                        try
                        {
                            dynamic d = it;
                            var rule = new PromoRules();
                            // mapear campos comúnmente usados (defensivo)
                            try { rule.id = (int)((object)d.id); } catch { }
                            try { rule.discount = Convert.ToDouble(d.discount); } catch { }
                            try { rule.unlimited_time = (bool)d.unlimited_time; } catch { }
                            try { rule.start_date = (DateTime?)d.start_date; } catch { }
                            try { rule.end_date = (DateTime?)d.end_date; } catch { }
                            try { rule._payment_method_id = d._payment_method_id; } catch { }
                            try { rule._selection_type_id = d._selection_type_id; } catch { }
                            try { rule.minimum_value = d.minimum_value; } catch { }
                            try { rule.state = (bool?)d.state ?? true; } catch { }

                            outList.Add(rule);
                        }
                        catch { /* ignoramos mapeos inválidos */ }
                    }

                    return outList;
                }
            }
            catch
            {
                // no podemos extraer reglas — devolvemos vacío
            }

            return new List<PromoRules>();
        }
    }
}
