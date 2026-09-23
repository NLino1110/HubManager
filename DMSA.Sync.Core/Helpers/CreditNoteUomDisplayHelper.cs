using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Inventory;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Linq;

namespace DMSA.Sync.Core.Helpers;

public static class CreditNoteUomDisplayHelper
{
    public static string BuildInvoiceHeader(string? docnumMask, DateTime? invoiceDate)
    {
        var doc = string.IsNullOrWhiteSpace(docnumMask) ? "-" : docnumMask.Trim();
        var dateText = invoiceDate.HasValue ? invoiceDate.Value.ToString("dd/MM/yyyy") : "-";
        return $"Nº {doc}   FECHA {dateText}";
    }

    public static void ApplyUomDisplay(credit_note_request_detail detail, uom_uom? uom)
    {
        ApplyUomDisplay(detail, uom, null, clientPermanentlyClosing: false, inventoryUomId: 0);
    }

    public static void ApplyUomDisplay(
        credit_note_request_detail detail,
        uom_uom? invoiceUom,
        uom_uom? inventoryUom,
        bool clientPermanentlyClosing,
        int inventoryUomId)
    {
        if (detail == null)
            return;

        if (detail.client_permanently_closing && detail._uom_id > 0)
        {
            return;
        }

        if (detail.invoice_line_uom_id <= 0 && detail.product_uom_id > 0)
            detail.invoice_line_uom_id = detail.product_uom_id;

        detail.use_inventory_return_uom = false;
        detail.inventory_return_display_swapped = false;
        detail.inventory_uom_id = 0;

        if (clientPermanentlyClosing && inventoryUom != null && inventoryUomId > 0)
        {
            ApplyInventoryReturnUomDisplay(detail, invoiceUom, inventoryUom, inventoryUomId, clientPermanentlyClosing);
            return;
        }

        ApplyInvoiceUomDisplay(detail, invoiceUom);
    }

    private static void ApplyInvoiceUomDisplay(credit_note_request_detail detail, uom_uom? uom)
    {
        if (uom == null || detail.product_uom_id <= 0)
        {
            detail.uom_display_name = "-";
            detail.uom_factor = 1m;
            detail.quantity_available_by_uom = detail.quantity_available;
            detail.NotifyUomDisplayChanged();
            return;
        }

        detail.uom_display_name = ResolveUomDisplayName(uom);
        detail.uom_factor = ResolveUomFactor(uom);
        detail.quantity_available_by_uom = detail.quantity_available * detail.uom_factor;
        detail.NotifyUomDisplayChanged();
    }

    private static void ApplyInventoryReturnUomDisplay(
        credit_note_request_detail detail,
        uom_uom? invoiceUom,
        uom_uom inventoryUom,
        int inventoryUomId,
        bool clientPermanentlyClosing)
    {
        detail.use_inventory_return_uom = true;
        detail.inventory_uom_id = inventoryUomId;
        detail._uom_id = inventoryUomId;
        detail.client_permanently_closing = clientPermanentlyClosing;

        detail.uom_display_name = ResolveUomDisplayName(inventoryUom);
        detail.uom_factor = ResolveUomFactor(inventoryUom);

        var invoiceFactor = ResolveUomFactor(invoiceUom);
        var invoiceQtyAvailable = detail.quantity_available_invoice > 0m
            ? detail.quantity_available_invoice
            : detail.quantity_available;

        if (detail.quantity_available_invoice <= 0m)
            detail.quantity_available_invoice = invoiceQtyAvailable;

        var invoiceUomId = detail.invoice_line_uom_id > 0
            ? detail.invoice_line_uom_id
            : detail.product_uom_id;

        if (invoiceUomId != inventoryUomId)
        {
            detail.inventory_return_display_swapped = true;

            decimal baseQty = detail.quantity_available_base > 0m
                ? detail.quantity_available_base
                : invoiceQtyAvailable * invoiceFactor;

            decimal invPkgQty = detail.uom_factor > 0m
                ? baseQty / detail.uom_factor
                : baseQty;

            detail.quantity_available = baseQty;
            detail.quantity_available_by_uom = invPkgQty;
        }
        else
        {
            detail.inventory_return_display_swapped = false;

            decimal baseQty = detail.quantity_available_base > 0m
                ? detail.quantity_available_base
                : invoiceQtyAvailable * invoiceFactor;

            decimal invPkgQty = detail.uom_factor > 0m
                ? baseQty / detail.uom_factor
                : baseQty;

            detail.quantity_available = invPkgQty;
            detail.quantity_available_by_uom = baseQty;
        }

        detail.NotifyUomDisplayChanged();
    }

    public static void RestoreUomDisplayFromPersistence(
        credit_note_request_detail detail,
        IReadOnlyDictionary<int, uom_uom> uomById)
    {
        if (detail == null || !detail.client_permanently_closing || detail._uom_id <= 0)
            return;

        detail.use_inventory_return_uom = true;
        detail.inventory_uom_id = detail._uom_id;

        if (uomById.TryGetValue(detail._uom_id, out var inventoryUom))
        {
            detail.uom_display_name = ResolveUomDisplayName(inventoryUom);
            detail.uom_factor = ResolveUomFactor(inventoryUom);
        }

        var invoiceUomId = detail.invoice_line_uom_id > 0
            ? detail.invoice_line_uom_id
            : detail.product_uom_id;

        detail.inventory_return_display_swapped = invoiceUomId > 0 && invoiceUomId != detail._uom_id;

        if (detail.invoice_line_uom_id > 0)
            detail.product_uom_id = detail.invoice_line_uom_id;

        if (detail.quantity_available_by_uom <= 0m)
            RecalculateQuantityAvailableByUom(detail);

        detail.NotifyUomDisplayChanged();
    }

    private static void RecalculateQuantityAvailableByUom(credit_note_request_detail detail)
    {
        if (detail.inventory_return_display_swapped)
        {
            detail.quantity_available_by_uom = detail.uom_factor > 0m
                ? detail.quantity_available / detail.uom_factor
                : detail.quantity_available;
        }
        else
        {
            detail.quantity_available_by_uom = detail.quantity_available * detail.uom_factor;
        }
    }

    /// <summary>
    /// Cierre + U.M. distintas: usa <see cref="credit_note_request_detail.quantity_available_base"/> de la factura si &gt; 0.
    /// </summary>
    public static void ApplyQuantityAvailableBaseForClosing(credit_note_request_detail detail)
    {
        if (detail == null
            || !detail.client_permanently_closing
            || detail.quantity_available_base <= 0m
            || !SalesAndInventoryUomsDiffer(detail))
        {
            return;
        }

        if (detail.inventory_return_display_swapped)
            detail.quantity_available = detail.quantity_available_base;
        else if (detail.uom_factor > 0m)
            detail.quantity_available = detail.quantity_available_base / detail.uom_factor;
        else
            detail.quantity_available = detail.quantity_available_base;

        RecalculateQuantityAvailableByUom(detail);
        detail.NotifyUomDisplayChanged();
    }

    /// <summary>
    /// Antes de <c>InsertAsync</c> en <c>credit_note_request_detail</c>: UOM, precios convertidos (cierre) y cantidades.
    /// </summary>
    public static async Task PrepareLineForLocalStorageAsync(
        credit_note_request_detail detail,
        string databaseFilename,
        bool clientPermanentlyClosing)
    {
        if (detail == null)
            return;

        StampLineForLocalStorage(detail, clientPermanentlyClosing);
        await EnsureInventoryUomIdForStorageAsync(detail, databaseFilename);

        if (!clientPermanentlyClosing)
        {
            SyncQuantityInvoicedFromAvailable(new[] { detail });
            return;
        }

        var uomDb = new UomUomDb(databaseFilename);
        var uoms = await uomDb.GetItemsAsync();
        var uomById = uoms.ToDictionary(u => u.id);

        if (detail._uom_id > 0)
            RestoreUomDisplayFromPersistence(detail, uomById);

        ApplyClosingUomPriceConversion(detail, uomById);
        SyncQuantityInvoicedFromAvailable(new[] { detail });
    }

    public static void StampLineForLocalStorage(credit_note_request_detail detail, bool clientPermanentlyClosing)
    {
        if (detail == null)
            return;

        detail.client_permanently_closing = clientPermanentlyClosing;

        if (detail._uom_id <= 0 && detail.inventory_uom_id > 0)
            detail._uom_id = detail.inventory_uom_id;

        if (detail.invoice_line_uom_id <= 0 && detail.product_uom_id > 0)
            detail.invoice_line_uom_id = detail.product_uom_id;

        if (detail.client_permanently_closing && detail.invoice_line_uom_id > 0)
            detail.product_uom_id = detail.invoice_line_uom_id;
    }

    public static async Task EnsureInventoryUomIdForStorageAsync(
        credit_note_request_detail detail,
        string databaseFilename)
    {
        if (detail == null || !detail.client_permanently_closing || detail._uom_id > 0)
            return;

        if (detail.inventory_uom_id > 0)
        {
            detail._uom_id = detail.inventory_uom_id;
            return;
        }

        if (detail.product_id <= 0)
            return;

        var productDb = new ProductProductDb(databaseFilename);
        var product = await productDb.GetItemAsync(x => x.id == detail.product_id);
        if (product != null && product._uom_id > 0)
            detail._uom_id = product._uom_id;
    }

    /// <summary>
    /// JsonConvert pierde propiedades con [JsonIgnore]; copiar explícito al clonar para SQLite.
    /// </summary>
    public static void CopyPersistedLocalFields(
        credit_note_request_detail source,
        credit_note_request_detail target)
    {
        if (source == null || target == null)
            return;

        target.client_permanently_closing = source.client_permanently_closing;
        target._uom_id = source._uom_id;
        target.invoice_line_uom_id = source.invoice_line_uom_id;
        target.quantity_available_invoice = source.quantity_available_invoice;
        target.quantity_available_base = source.quantity_available_base;
        target.quantity_available_by_uom = source.quantity_available_by_uom;
        target.price_total = source.price_total;
        target.discount = source.discount;
        target.discount_percentage = source.discount_percentage;
        target.discount_balance = source.discount_balance;
        target.price_unit = source.price_unit;
        target.price_return = source.price_return;
        target.price_subtotal = source.price_subtotal;
        target.siv_price_unit = source.siv_price_unit;
        target.siv_price_return = source.siv_price_return;
        target.account_id = source.account_id;
        target.partner_id = source.partner_id;
        target.currency_id = source.currency_id;
        target.display_type = source.display_type;
        target.analitica_id = source.analitica_id;
        target.amount_currency = source.amount_currency;
        target.disc_amount = source.disc_amount;
        target.quantity_available = source.quantity_available;
        target.quantity_invoiced = source.quantity_invoiced;
        target.original_quantity = source.original_quantity;
    }

    /// <summary>
    /// Al editar NC: precio y descuento desde account_move_line (line_id).
    /// </summary>
    public static async Task EnrichLinesPricingFromInvoiceAsync(
        IEnumerable<credit_note_request_detail> lines,
        string databaseFilename)
    {
        if (lines == null)
            return;

        var materialized = lines.ToList();
        List<int> lineIds = materialized
            .Where(l => l.line_id > 0)
            .Select(l => l.line_id)
            .Distinct()
            .ToList();

        if (lineIds.Count == 0)
            return;

        var moveLineDb = new AccountMoveLineDb(databaseFilename);
        var invoiceLines = await moveLineDb.GetItemsAsync(x => lineIds.Contains(x.id));
        var invoiceLineById = invoiceLines.ToDictionary(x => x.id);

        foreach (var detail in materialized)
        {
            if (detail.line_id <= 0 ||
                !invoiceLineById.TryGetValue(detail.line_id, out var aml))
            {
                continue;
            }

            ApplyInvoiceLineOdooFields(detail, aml);
            detail.quantity_available_base = aml.quantity_available_base;
            if (aml.quantity_available > 0m)
                detail.quantity_available_invoice = aml.quantity_available;
        }
    }

    public static async Task PrepareLinesForOdooSendAsync(
        IList<credit_note_request_detail> lines,
        string databaseFilename,
        int fallbackPartnerId)
    {
        if (lines == null || lines.Count == 0)
            return;

        Dictionary<int, uom_uom> uomById = new();
        if (lines.Any(l => l.client_permanently_closing))
        {
            var uomDb = new UomUomDb(databaseFilename);
            var uoms = await uomDb.GetItemsAsync();
            uomById = uoms.ToDictionary(u => u.id);
        }

        List<int> lineIds = lines
            .Where(l => l.line_id > 0)
            .Select(l => l.line_id)
            .Distinct()
            .ToList();

        Dictionary<int, account_move_line> invoiceLineById = new();
        Dictionary<int, account_move> moveById = new();

        if (lineIds.Count > 0)
        {
            var moveLineDb = new AccountMoveLineDb(databaseFilename);
            var invoiceLines = await moveLineDb.GetItemsAsync(x => lineIds.Contains(x.id));
            invoiceLineById = invoiceLines.ToDictionary(x => x.id);

            List<int> moveIds = invoiceLines
                .Select(x => x._move_id)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (moveIds.Count > 0)
            {
                var moveDb = new AccountMoveDb(databaseFilename);
                var moves = await moveDb.GetItemsAsync(x => moveIds.Contains(x.id));
                moveById = moves.ToDictionary(x => x.id);
            }
        }

        foreach (var line in lines)
        {
            account_move_line? aml = null;
            account_move? move = null;

            if (line.line_id > 0 && invoiceLineById.TryGetValue(line.line_id, out var invoiceLine))
            {
                aml = invoiceLine;
                moveById.TryGetValue(invoiceLine._move_id, out move);
            }

            if (aml != null)
            {
                line.quantity_available_base = aml.quantity_available_base;
                if (aml.quantity_available > 0m)
                    line.quantity_available_invoice = aml.quantity_available;
            }

            if (line.client_permanently_closing)
            {
                await EnsureInventoryUomIdForStorageAsync(line, databaseFilename);
                RestoreUomDisplayFromPersistence(line, uomById);
                ApplyQuantityAvailableBaseForClosing(line);
            }

            PrepareLineForOdooSend(line, aml, move, fallbackPartnerId, uomById);
        }
    }

    public static void PrepareLineForOdooSend(
        credit_note_request_detail detail,
        account_move_line? aml,
        account_move? move,
        int fallbackPartnerId,
        IReadOnlyDictionary<int, uom_uom>? uomById = null)
    {
        if (detail == null)
            return;

        detail.calculo_unidad_base = detail.client_permanently_closing;
        detail.use_type = "preview";
        detail.already_added = true;
        detail.is_refund = false;

        if (aml != null)
        {
            ApplyInvoiceLineSalesUom(detail, aml);
            detail.quantity_available_base = aml.quantity_available_base;
            if (aml.quantity_available > 0m)
                detail.quantity_available_invoice = aml.quantity_available;

            if (detail.client_permanently_closing)
                ApplyInvoiceLineOdooMetadata(detail, aml);
            else
                ApplyInvoiceLineOdooFields(detail, aml);
        }

        if (move != null && detail.partner_id <= 0)
            detail.partner_id = move._partner_id;
        else if (detail.partner_id <= 0)
            detail.partner_id = fallbackPartnerId;

        if (detail.currency_id <= 0)
            detail.currency_id = 1;

        SyncDiscountFieldsForOdoo(detail);
        PrepareLineForSave(detail);
        ApplyQuantityInvoicedForOdooSend(detail);
    }

    /// <summary>
    /// Odoo: cantidad facturada/disponible desde <see cref="credit_note_request_detail.quantity_available"/>.
    /// Con cierre y U.M. distintas, <see cref="credit_note_request_detail.quantity_available"/> es la Cant. Disp. en unidad base (p. ej. 40, no 4).
    /// </summary>
    public static void SyncQuantityInvoicedFromAvailable(IEnumerable<credit_note_request_detail> lines)
    {
        if (lines == null)
            return;

        foreach (var line in lines)
            ApplyQuantityInvoicedForOdooSend(line);
    }

    private static void ApplyQuantityInvoicedForOdooSend(credit_note_request_detail detail)
    {
        decimal available = ResolveQuantityAvailableForOdooSend(detail);
        detail.quantity_invoiced = available;
        detail.original_quantity = available;
    }

    private static decimal ResolveQuantityAvailableForOdooSend(credit_note_request_detail detail)
    {
        if (detail.quantity_available > 0m)
            return detail.quantity_available;

        if (detail.quantity_available_invoice > 0m)
            return detail.quantity_available_invoice;

        if (detail.original_quantity > 0m)
            return detail.original_quantity;

        return detail.quantity_invoiced;
    }

    private static void SyncDiscountFieldsForOdoo(credit_note_request_detail detail)
    {
        if (detail.discount > 0m && detail.discount_percentage <= 0m)
            detail.discount_percentage = detail.discount;
        else if (detail.discount_percentage > 0m && detail.discount <= 0m)
            detail.discount = detail.discount_percentage;
    }

    private static void ApplyInvoiceLinePricing(
        credit_note_request_detail detail,
        account_move_line aml)
    {
        detail.price_unit = aml.price_unit;
        if (detail.price_return <= 0m)
            detail.price_return = aml.price_unit;

        detail.price_subtotal = aml.price_subtotal;

        if (aml.price_total > 0m)
            detail.price_total = aml.price_total;
        else if (detail.price_total <= 0m)
            detail.price_total = aml.price_unit;

        var discountPct = aml.discount > 0m ? aml.discount : aml.discount_percentage;
        detail.discount = discountPct;
        detail.discount_percentage = discountPct;

        detail.discount_balance = aml.discount_balance;
    }

    private static void ApplyInvoiceLineOdooFields(
        credit_note_request_detail detail,
        account_move_line aml)
    {
        ApplyInvoiceLinePricing(detail, aml);

        if (detail.account_id <= 0)
            detail.account_id = aml._account_id;

        if (string.IsNullOrWhiteSpace(detail.display_type))
        {
            detail.display_type = string.IsNullOrWhiteSpace(aml.display_type)
                ? "product"
                : aml.display_type;
        }

        detail.amount_currency = aml.price_subtotal;
        detail.disc_amount = aml.discount_balance;

        detail.analitica_id = ResolveAnaliticaIdFromMoveLine(aml, detail.analitica_id);
    }

    private static void ApplyInvoiceLineOdooMetadata(
        credit_note_request_detail detail,
        account_move_line aml)
    {
        if (detail.account_id <= 0)
            detail.account_id = aml._account_id;

        if (string.IsNullOrWhiteSpace(detail.display_type))
        {
            detail.display_type = string.IsNullOrWhiteSpace(aml.display_type)
                ? "product"
                : aml.display_type;
        }

        detail.disc_amount = aml.discount_balance;
        detail.analitica_id = ResolveAnaliticaIdFromMoveLine(aml, detail.analitica_id);

        if (detail.discount <= 0m && detail.discount_percentage <= 0m)
        {
            var discountPct = aml.discount > 0m ? aml.discount : aml.discount_percentage;
            detail.discount = discountPct;
            detail.discount_percentage = discountPct;
        }
    }

    private static int ResolveAnaliticaIdFromMoveLine(account_move_line aml, int existingAnaliticaId = 0)
    {
        if (aml._analitica_id > 0)
            return aml._analitica_id;

        if (existingAnaliticaId > 0)
            return existingAnaliticaId;

        if (string.IsNullOrWhiteSpace(aml.analytic_line_ids_json))
            return 0;

        var analytics = aml.analytic_line_ids_json
            .Trim('[', ']')
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var n) ? n : 0)
            .Where(n => n != 0)
            .ToArray();

        return analytics.Length > 0 ? analytics[0] : 0;
    }

    /// <summary>
    /// Payload Odoo (<c>product_uom_id</c>): con cierre permanente → <see cref="credit_note_request_detail._uom_id"/> (inventario);
    /// sin cierre → <see cref="credit_note_request_detail.product_uom_id"/> local (ventas, flujo habitual).
    /// </summary>
    public static void PrepareLineForSave(credit_note_request_detail detail)
    {
        if (detail == null || !detail.client_permanently_closing)
            return;

        int inventoryUomId = ResolveInventoryUomId(detail);
        if (inventoryUomId > 0)
            detail.product_uom_id = inventoryUomId;

        if (detail.inventory_return_display_swapped && detail.uom_factor > 0m)
            detail.quantity = detail.quantity / detail.uom_factor;
    }

    private static void ApplyInvoiceLineSalesUom(
        credit_note_request_detail detail,
        account_move_line aml)
    {
        if (aml._product_uom_id > 0)
            detail.invoice_line_uom_id = aml._product_uom_id;
    }

    private static int ResolveSalesUomId(credit_note_request_detail detail) =>
        detail.invoice_line_uom_id > 0 ? detail.invoice_line_uom_id : detail.product_uom_id;

    private static int ResolveInventoryUomId(credit_note_request_detail detail) =>
        detail._uom_id > 0 ? detail._uom_id : detail.inventory_uom_id;

    private static bool SalesAndInventoryUomsDiffer(credit_note_request_detail detail)
    {
        int salesUomId = ResolveSalesUomId(detail);
        int inventoryUomId = ResolveInventoryUomId(detail);
        return salesUomId > 0 && inventoryUomId > 0 && salesUomId != inventoryUomId;
    }

    /// <summary>
    /// Cierre: convierte solo <see cref="credit_note_request_detail.price_return"/> / <see cref="credit_note_request_detail.siv_price_return"/>
    /// si U.M. distintas; recalcula <see cref="credit_note_request_detail.price_subtotal"/> y <see cref="credit_note_request_detail.price_total"/>
    /// con cantidad a devolver. <see cref="credit_note_request_detail.price_unit"/> y <see cref="credit_note_request_detail.amount_currency"/> de factura.
    /// </summary>
    public static void ApplyClosingUomPriceConversion(
        credit_note_request_detail detail,
        IReadOnlyDictionary<int, uom_uom> uomById)
    {
        if (detail == null || !detail.client_permanently_closing)
            return;

        if (SalesAndInventoryUomsDiffer(detail)
            && TryGetUomPriceRatio(detail, uomById, out decimal ratio)
            && ratio != 1m
            && !IsReturnPriceAlreadyConvertedForClosing(detail))
        {
            decimal returnBase = detail.price_return > 0m ? detail.price_return : detail.price_unit;
            if (returnBase > 0m)
                detail.price_return = RoundUnitPrice(returnBase * ratio);

            decimal sivReturnBase = detail.siv_price_return > 0m ? detail.siv_price_return : detail.siv_price_unit;
            if (sivReturnBase > 0m)
                detail.siv_price_return = RoundUnitPrice(sivReturnBase * ratio);
        }

        RecalculateReturnLineTotalsForClosing(detail);
        detail.NotifyPricingDisplayChanged();
    }

    /// <summary>
    /// Subtotal/total de la línea según cantidad a devolver y precio convertido (sin IVA + IVA de la factura).
    /// </summary>
    public static void RecalculateReturnLineTotalsForClosing(credit_note_request_detail detail)
    {
        if (detail == null || !detail.client_permanently_closing)
            return;

        decimal qty = detail.quantity;
        if (qty <= 0m)
            return;

        decimal taxMultiplier = ResolveLineTaxMultiplier(detail);

        if (detail.siv_price_return > 0m)
        {
            detail.price_subtotal = RoundUnitPrice(detail.siv_price_return * qty);
            detail.price_total = RoundUnitPrice(detail.price_subtotal * taxMultiplier);
            return;
        }

        if (detail.price_return > 0m)
        {
            detail.price_subtotal = RoundUnitPrice(detail.price_return * qty);
            if (taxMultiplier > 1m)
                detail.price_total = RoundUnitPrice(detail.price_subtotal * taxMultiplier);
            else
                detail.price_total = detail.price_subtotal;
        }
    }

    private static decimal ResolveLineTaxMultiplier(credit_note_request_detail detail)
    {
        if (detail.siv_price_unit > 0m && detail.price_unit > 0m)
            return detail.price_unit / detail.siv_price_unit;

        if (detail.price_subtotal > 0m && detail.price_total > 0m)
            return detail.price_total / detail.price_subtotal;

        return 1m;
    }

    /// <summary>Evita doble conversión (p. ej. 0.55 → 0.055) al guardar tras Enrich.</summary>
    private static bool IsReturnPriceAlreadyConvertedForClosing(credit_note_request_detail detail)
    {
        if (detail.price_unit <= 0m || detail.price_return <= 0m)
            return false;

        return detail.price_return < detail.price_unit * 0.99m;
    }

    private static bool TryGetUomPriceRatio(
        credit_note_request_detail detail,
        IReadOnlyDictionary<int, uom_uom> uomById,
        out decimal ratio)
    {
        ratio = 1m;
        if (uomById == null || uomById.Count == 0)
            return false;

        int salesUomId = ResolveSalesUomId(detail);
        int inventoryUomId = ResolveInventoryUomId(detail);

        if (!uomById.TryGetValue(salesUomId, out var invoiceUom)
            || !uomById.TryGetValue(inventoryUomId, out var inventoryUom))
        {
            return false;
        }

        decimal invoiceFactor = ResolveUomFactor(invoiceUom);
        decimal inventoryFactor = ResolveUomFactor(inventoryUom);
        if (invoiceFactor <= 0m)
            invoiceFactor = 1m;
        if (inventoryFactor <= 0m)
            inventoryFactor = 1m;

        ratio = inventoryFactor / invoiceFactor;
        return true;
    }

    private static decimal RoundUnitPrice(decimal value) =>
        Math.Round(value, 4, MidpointRounding.AwayFromZero);

    public static async Task EnrichLinesAsync(
        IEnumerable<credit_note_request_detail> lines,
        string databaseFilename,
        bool clientPermanentlyClosing = false)
    {
        if (lines == null)
            return;

        var materialized = lines.ToList();
        if (materialized.Count == 0)
            return;

        var uomDb = new UomUomDb(databaseFilename);
        var uoms = await uomDb.GetItemsAsync();
        var uomById = uoms.ToDictionary(u => u.id);

        Dictionary<int, int> inventoryUomByProductId = new();
        if (clientPermanentlyClosing)
        {
            List<int> productIds = materialized
                .Select(l => l.product_id)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (productIds.Count > 0)
            {
                var productDb = new ProductProductDb(databaseFilename);
                var products = await productDb.GetItemsAsync(x => productIds.Contains(x.id));
                inventoryUomByProductId = products
                    .Where(p => p._uom_id > 0)
                    .ToDictionary(p => p.id, p => p._uom_id);
            }
        }

        foreach (var line in materialized)
        {
            if (line.client_permanently_closing && line._uom_id > 0)
            {
                RestoreUomDisplayFromPersistence(line, uomById);
                ApplyQuantityAvailableBaseForClosing(line);
                ApplyClosingUomPriceConversion(line, uomById);
                line.invoice_header = BuildInvoiceHeader(line.docnum_mask, line.invoice_date);
                continue;
            }

            uomById.TryGetValue(ResolveSalesUomId(line), out var invoiceUom);

            uom_uom? inventoryUom = null;
            var inventoryUomId = 0;
            if (clientPermanentlyClosing &&
                inventoryUomByProductId.TryGetValue(line.product_id, out inventoryUomId))
            {
                uomById.TryGetValue(inventoryUomId, out inventoryUom);
            }

            ApplyUomDisplay(
                line,
                invoiceUom,
                inventoryUom,
                clientPermanentlyClosing,
                inventoryUomId);

            if (clientPermanentlyClosing)
            {
                line.client_permanently_closing = true;
                ApplyQuantityAvailableBaseForClosing(line);
                ApplyClosingUomPriceConversion(line, uomById);
            }

            line.invoice_header = BuildInvoiceHeader(line.docnum_mask, line.invoice_date);
        }
    }

    private static string ResolveUomDisplayName(uom_uom uom) =>
        !string.IsNullOrWhiteSpace(uom.display_name)
            ? uom.display_name
            : uom.name ?? "-";

    private static decimal ResolveUomFactor(uom_uom? uom)
    {
        var factor = (decimal)(uom?.factor_inv ?? 1d);
        return factor > 0m ? factor : 1m;
    }
}
