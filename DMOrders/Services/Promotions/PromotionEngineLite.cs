using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using Newtonsoft.Json.Linq;

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

        private async Task<(bool exists, int totalTimes)> CheckPromoCenterAsync( List<PromoCenters> centers, 
            int pricelist_id, 
            PromotionBenefit promotionBenefit)
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
                        //No eliminar - funciona correctamente 
                        // pero por ahora se pasara por alto
                        totalTimes += center.times_inv;

                        //Si es que es descuento es ilimitado
                        if (promotionBenefit._promotion_type_id == 6)
                        {
                            //----POR AHORA 1000
                            totalTimes += 1000;
                        }
                    }
                }
            }

            return (exists, totalTimes);
        }

        private static List<PromoRules> TryExtractRules(PromotionBenefit promo)
        {            
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
