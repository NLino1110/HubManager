using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DMOrders.Services.Promotions
{
    public class PromotionRepository : IPromotionRepository
    {
        public async Task<IEnumerable<PromotionBenefit>> Search(int companyId, DateTime nowUtc)
        {
            // Usas tu clase PromotionBenefitDb (ya implementada en tu base)
            var db = new PromotionBenefitDb(App.Session.odooConnection.DbNameSqlite);

            // Llamas al método que ya tienes para buscar promociones
            var promos = await db.SearchAll(companyId, nowUtc);

            var productPromoDb = new PromotionProductDetailDb(App.Session.odooConnection.DbNameSqlite);
            var promoRulesDb = new PromoRulesDb(App.Session.odooConnection.DbNameSqlite);
            var promoCentersDb = new PromoCentersDb(App.Session.odooConnection.DbNameSqlite);

            // Asegura listas inicializadas
            foreach (var promo in promos)
            {
                // 1️⃣ Cargar productos asociados a la promoción
                promo._product_promotion_ids = await productPromoDb.GetItemsByPromo(promo.id);

                // 2️⃣ Cargar reglas de la promoción
                promo._promotion_rules_ids = await promoRulesDb.GetItemsByParent(promo.id);

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
    public sealed class PromotionEngineLite
    {
        private readonly IPromotionRepository _repo;

        public PromotionEngineLite(IPromotionRepository repo) => _repo = repo;

        /// <summary>
        /// Evalúa promociones aplicables para un producto + cantidad en el contexto dado.
        /// - product: objeto product_product (puede ser null si la evaluación es por pedido).
        /// - qty: cantidad (si aplica).
        /// - partner: cliente (opcional).
        /// - companyId: id de la compañía (obligatorio, se usa para filtrar en repo).
        /// - dateUtc: fecha a considerar (opcional).
        /// </summary>
        public async Task<PromotionEvalResult> EvaluatePromotions(
            int product_id,
            int qty,            
            int companyId,
            DateTime? dateUtc = null)
        {
            if (qty <= 0)
                throw new ArgumentOutOfRangeException(nameof(qty), "qty debe ser > 0");

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
                var baseReasons = new List<string>();
                baseReasons.Add("Promoción activa y dentro de vigencia.");

                // Si la promoción tiene detalles de productos explícitos:
                bool productMatches = true;
                if (promo._product_promotion_ids != null && promo._product_promotion_ids.Any())
                {
                    if (product_id == 0)
                    {
                        productMatches = false;
                    }
                    else
                    {
                        // promo._product_promotion_ids normalmente será List<PromotionProductDetail>
                        // intentamos comparar por product id (prop name típico: product / product_id)
                        productMatches = promo._product_promotion_ids.Any(d =>
                        {
                            try
                            {
                                // Intentamos leer 'product' o 'product_id' dentro de detail
                                // La clase PromotionProductDetail en tu proyecto debería tener .product?.id o .product_id
                                PromotionProductDetail det = d;
                                if (det == null) return false;
                                                                
                                try
                                {
                                    int pid = det._product_id;
                                    return pid == product_id;
                                }
                                catch { }

                                return false;
                            }
                            catch
                            {
                                return false;
                            }
                        });
                    }

                    if (!productMatches) continue;
                    baseReasons.Add("Producto incluido en detalle de la promoción.");
                }

                // Obtener reglas de la promoción (si existen)
                // En tus modelos originales pones promo._promotion_rules_ids -> en tu modelo devuelve empty list.
                // Aquí intentamos leer una propiedad dinámica que contenga reglas (si existe).
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

                    if (promo._promotion_type_id == 2) // es regalo
                    {
                        if (r.value > 0)
                        {
                            if (qty < r.value) continue;
                            reasons.Add($"Cumple cantidad mínima: {r.value}");
                        }
                    }

                    if (promo._promotion_type_id == 4) // es NXN
                    {
                        if (r.value > 0)
                        {
                            if (qty < r.value) continue;
                            reasons.Add($"Cumple cantidad mínima: {r.value}");
                        }
                    }

                    if (promo._promotion_type_id == 6) // es descuento
                    {
                        if (r.minimum_value > 0)
                        {
                            if (qty < r.minimum_value) continue;
                            reasons.Add($"Cumple cantidad mínima: {r.minimum_value}");
                        }
                    }

                    // aquí podrías incluir chequeos de método de pago, selección, etc. si los pasas como parámetros.

                    // Si llegamos acá, la regla aplica:
                    results.Add(new PromotionEvalItem
                    {
                        Promotion = promo,
                        //Rule = new RuleInfo
                        //{
                        //    Id = r.id,
                        //    Discount = r.discount,
                        //    UnlimitedTime = r.unlimited_time,
                        //    StartDate = r.start_date,
                        //    EndDate = r.end_date,
                        //    PaymentMethodId = r._payment_method_id,
                        //    SelectionTypeId = r._selection_type_id,
                        //    MinQuantity = r.minimum_value
                        //},
                        RuleSet = r,
                        Discount = r.discount,
                        Reasons = reasons
                    });
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
