using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.Native;
using InputKit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Promotions
{
    public sealed class PromotionEvalResult
    {
        public DateTime NowUtc { get; set; }
        public List<PromotionEvalItem> Items { get; set; } = new();
        public PromotionEvalItem? Best { get; set; }
    }

    public interface IPromotionRepository
    {
        /// Devuelve promociones ya filtradas por compañía/fecha/estado/cliente (si aplica).
        IEnumerable<PromotionBenefit> Search(
            long companyId,
            DateTime nowUtc,
            long? partnerId);
    }

    public sealed class PromotionEngine
    {
        private readonly IPromotionRepository _repo;

        public PromotionEngine(IPromotionRepository repo) => _repo = repo;

        public PromotionEvalResult EvaluatePromotions(
            product_product product,
            int qty,
            res_partner? partner = null,
            PosPaymentMethod? paymentMethod = null,
            DateTime? dateUtc = null,
            res_company? company = null,
            PosTarjetasCanal? targetSegment = null,
            ProductPricelist? channel = null,
            PromotionSelectionType? selectionType = null)
        {
            if (product is null) throw new ArgumentNullException(nameof(product));
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty), "qty debe ser > 0");
            if (company is null) throw new ArgumentNullException(nameof(company));

            var nowUtc = (dateUtc ?? DateTime.UtcNow).AddTicks(-(dateUtc ?? DateTime.UtcNow).Ticks % TimeSpan.TicksPerMinute);

            // 1) Buscar promos candidatas base (estado/fecha/compañía/cliente)
            var candidatas = _repo.Search(company.id, nowUtc, partner?.id)
                .Where(p => p.active && p.state == "authorized")
                .Where(p => (p.start_datetime == null || p.start_datetime <= nowUtc)
                         && (p.end_datetime == null || p.end_datetime >= nowUtc))
                .Where(p => p._company_id == company.id);

            // Nota: inclusión/exclusión de partner se asume ya considerada en repo.Search; si no:
            //if (partner != null)
            //{
            //    candidatas = candidatas.Where(p =>
            //        (!p.CustomersIncludedIds.Any() || p.CustomersIncludedIds.Contains(partner.Id)) &&
            //        (!p.CustomersExcludedIds.Any() || !p.CustomersExcludedIds.Contains(partner.Id)));
            //}

            var items = new List<PromotionEvalItem>();

            foreach (var promo in candidatas)
            {
                var reasons = new List<string>();

                // (A) Segmento por parámetro opcional
                if (targetSegment != null)
                {
                    if (promo._target_segment_id != targetSegment.id)
                        continue;
                    reasons.Add("Coincide el segmento objetivo.");
                }

                // (B) Canal (pricelist) en centers.levels_ids si se pasó channel
                if (channel != null)
                {
                    if (promo._centers_ids != null && promo._centers_ids.Any())
                    {
                        var inAny = promo._centers_ids.Any(c => c._levels_ids.Contains(channel));
                        if (!inAny) continue;
                        reasons.Add("Canal permitido en centers.levels_ids.");
                    }
                    else
                    {
                        reasons.Add("La promoción no define centers; no restringe canal.");
                    }
                }

                // (C) Producto perteneciente a la promo (si hay detalle explícito)
                var productOk = true;
                if (promo._product_promotion_ids != null && promo._product_promotion_ids.Any())
                {
                    productOk = promo._product_promotion_ids.Any(d =>
                        (d.product != null && d.product.id == product.id) ||
                        true);

                        //(d.ProductTemplate != null && d.ProductTemplate.Id == product.Template.Id));
                    if (!productOk) continue;
                    reasons.Add("Producto incluido en la promoción.");
                }

                // (D) Filtros de lealtad
                if (promo._loyalty_filters_ids != null && promo._loyalty_filters_ids.Any())
                {
                    if (!MatchLoyaltyFilters(promo, product, nowUtc, reasons))
                        continue;
                }

                // (E) Reglas activas
                var rules = (promo._promotion_rules_ids ?? new()).Where(r => r.state);
                var anyRuleOk = false;

                foreach (var rule in rules)
                {
                    var rr = new List<string>(reasons);

                    // Vigencia
                    if (!rule.unlimited_time)
                    {
                        var startOk = rule.start_date == null || rule.start_date <= nowUtc.Date;
                        var endOk = rule.end_date == null || rule.end_date >= nowUtc.Date;
                        if (!startOk || !endOk) continue;
                        rr.Add("Dentro de la vigencia de la regla.");
                    }
                    else
                    {
                        rr.Add("Regla sin vigencia (unlimited_time).");
                    }

                    // Método de pago
                    if (paymentMethod != null && rule._payment_method_id != null)
                    {
                        if (rule._payment_method_id != paymentMethod.Id) continue;
                        rr.Add("Coincide el método de pago.");
                    }

                    // Selection type (si se pasó)
                    if (selectionType != null)
                    {
                        var selOk = rule._selection_type_id != null
                            ? rule._selection_type_id == selectionType.Id
                            : (promo._selection_type_id == selectionType.Id);
                        if (!selOk) continue;
                        rr.Add("Coincide el tipo de selección.");
                    }

                    // Cantidad mínima
                    if (!MatchQtyAgainstRule(rule, qty, rr))
                        continue;

                    var discount = rule.discount;
                    var item = new PromotionEvalItem
                    {
                        Promotion = new PromotionHeader
                        {
                            Id = promo.id,
                            Code = promo.code,
                            Name = promo.name,
                            TypeId = promo._promotion_type_id,
                            TypeName = "" //promo.PromotionType?.Name
                        },
                        Rule = new RuleInfo
                        {
                            Id = rule.id,
                            Discount = discount,
                            UnlimitedTime = rule.unlimited_time,
                            StartDate = rule.start_date,
                            EndDate = rule.end_date,
                            PaymentMethodId = rule._payment_method_id,
                            SelectionTypeId = rule._selection_type_id
                        },
                        Discount = discount,
                        Reasons = rr
                    };

                    items.Add(item);
                    anyRuleOk = true;
                }

                // Si no hay reglas, podrías considerar la cabecera como aplicable (no implementado)
                _ = anyRuleOk;
            }

            var best = items.OrderByDescending(i => i.Discount).FirstOrDefault();

            return new PromotionEvalResult
            {
                NowUtc = nowUtc,
                Items = items,
                Best = best
            };
        }

        // ----------------------------
        // Auxiliares
        // ----------------------------
        private static bool MatchLoyaltyFilters(PromotionBenefit promo, product_product product, DateTime nowUtc, List<string> reasonsOut)
        {
            var anyMatch = false;
            var excluded = false;

            foreach (var filt in promo._loyalty_filters_ids)
            {
                var details = filt._detail_ids ?? new LoyaltyFiltersDetail[] { };
                foreach (var det in details)
                {
                    var matched = false;

                    //if (filt.marca && product.Marca != null && !string.IsNullOrEmpty(det.filter_name))
                    //{
                    //    if (product.Marca.Name == det.filter_name) matched = true;
                    //}

                    //if (!matched && filt.categoria && product.Categoria != null && !string.IsNullOrEmpty(det.filter_name))
                    //{
                    //    if (product.Categoria.Name == det.filter_name) matched = true;
                    //}

                    if (!matched) continue;

                    // Ventana del detalle (si aplica, se asume Date-only)
                    if (det.start_date != null && nowUtc.Date < det.start_date.Value.Date) continue;
                    if (det.end_date != null && nowUtc.Date > det.end_date.Value.Date) continue;

                    if (det.exclude) { excluded = true; continue; }

                    anyMatch = true;
                    reasonsOut.Add($"Coincide filtro de lealtad: {det.filter_name}");
                }
            }

            if (excluded) return false;
            // Si hay filtros configurados, exigimos al menos un match positivo
            var hasAnyFilterConfigured = promo._loyalty_filters_ids.Any(f => (f._detail_ids?.Any() ?? false));
            return hasAnyFilterConfigured ? anyMatch : true;
        }

        private static bool MatchQtyAgainstRule(PromoRules rule, int qty, List<string> reasonsOut)
        {
            // Usa el primer campo de cantidad que exista
            int? need = 1; // rule.minimum_value ?? rule.qty ?? rule.MinUnits;
            if (need.HasValue && qty < need.Value) return false;
            if (need.HasValue) reasonsOut.Add($"Cumple cantidad mínima: {need.Value}");
            return true;
        }
    }
}
