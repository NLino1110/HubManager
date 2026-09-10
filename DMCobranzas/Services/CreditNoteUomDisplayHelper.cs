using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Inventory;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMCobranzas.Services;

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
        if (detail == null)
            return;

        if (uom == null || detail.product_uom_id <= 0)
        {
            detail.uom_display_name = "-";
            detail.uom_factor = 1m;
            detail.quantity_available_by_uom = detail.quantity_available;
            detail.NotifyUomDisplayChanged();
            return;
        }

        detail.uom_display_name = !string.IsNullOrWhiteSpace(uom.display_name)
            ? uom.display_name
            : uom.name ?? "-";

        detail.uom_factor = (decimal)(uom.factor_inv ?? 1d);
        if (detail.uom_factor <= 0m)
            detail.uom_factor = 1m;

        detail.quantity_available_by_uom = detail.quantity_available * detail.uom_factor;
        detail.NotifyUomDisplayChanged();
    }

    public static async Task EnrichLinesAsync(
        IEnumerable<credit_note_request_detail> lines,
        string databaseFilename)
    {
        if (lines == null)
            return;

        var materialized = lines.ToList();
        if (materialized.Count == 0)
            return;

        var uomDb = new UomUomDb(databaseFilename);
        var uoms = await uomDb.GetItemsAsync();
        var uomById = uoms.ToDictionary(u => u.id);

        foreach (var line in materialized)
        {
            uomById.TryGetValue(line.product_uom_id, out var uom);
            ApplyUomDisplay(line, uom);
            line.invoice_header = BuildInvoiceHeader(line.docnum_mask, line.invoice_date);
        }
    }
}
