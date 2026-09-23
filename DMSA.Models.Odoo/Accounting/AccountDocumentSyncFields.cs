namespace DMSA.Models.Odoo.Accounting
{
    /// <summary>
    /// Campos de search_read HTTP y del ZIP Odoo (mnsa.mobile.document.pack).
    /// Deben coincidir con HEADER_FIELDS / LINE_FIELDS en mnsa_mobile_document_pack.py.
    /// </summary>
    public static class AccountDocumentSyncFields
    {
        public static readonly string[] Header =
        {
            "id",
            "name",
            "partner_id",
            "invoice_date",
            "invoice_date_due",
            "payment_state",
            "state",
            "move_type",
            "journal_id",
            "amount_residual",
            "amount_untaxed_signed",
            "amount_total_signed",
            "amount_total",
            "amount_tax",
            "l10n_latam_document_type_id",
            "invoice_user_id",
            "company_id",
            "team_id",
            "invoice_line_ids",
            "reversed_entry_id",
            "ref",
            "refund_invoice_ids",
            "docnum_mask",
            "partner_sale_id",
            "pf_promised_amount",
            "is_nota_debito",
            "create_date",
            "write_date",
        };

        public static readonly string[] Line =
        {
            "id",
            "move_id",
            "sequence",
            "name",
            "product_id",
            "quantity",
            "quantity_available",
            "quantity_available_base",
            "price_unit",
            "price_subtotal",
            "discount_balance",
            "price_total",
            "discount",
            "tax_ids",
            "product_uom_id",
            "analytic_line_ids",
            "display_type",
            "account_id",
            "analitica_id",
            "create_date",
            "write_date",
        };
    }
}
