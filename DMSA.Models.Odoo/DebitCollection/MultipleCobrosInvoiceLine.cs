using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.DebitCollection
{
    [Table("multiple_cobros_invoice_line")]
    public class MultipleCobrosInvoiceLine : OdooEntity
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("company_id")]
        public int CompanyId { get; set; }

        [Column("multiple_cobros_invoice_id")]
        public int MultipleCobrosInvoiceId { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("acc_number")]
        public string AccNumber { get; set; }

        [Column("bank_id")]
        public long? BankId { get; set; }

        [Column("journal_id")]
        public long? JournalId { get; set; }

        [Column("checkbook_id")]
        public long? CheckbookId { get; set; }

        [Column("checkbook_line_id")]
        public long? CheckbookLineId { get; set; }

        [Column("payment_id")]
        public long? PaymentId { get; set; }

        [Column("number_check_text")]
        public string NumberCheckText { get; set; }

        [Column("withdrawal_date")]
        public DateTime? WithdrawalDate { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("amount_in_words")]
        public string AmountInWords { get; set; }

        [Column("circular")]
        public string Circular { get; set; }

        [Column("partner_id")]
        public long? PartnerId { get; set; }

        [Column("acc_holder_name")]
        public string AccHolderName { get; set; }

        [Column("city_id")]
        public long? CityId { get; set; }

        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [Column("reconciled_date")]
        public DateTime? ReconciledDate { get; set; }

        [Column("state")]
        public string State { get; set; }

        [Column("partner_type")]
        public string PartnerType { get; set; }

        [Column("payment_type")]
        public string PaymentType { get; set; }

        [Column("type_invoice")]
        public string TypeInvoice { get; set; }

        [Column("amount_total")]
        public decimal? AmountTotal { get; set; }

        [Column("partner_bank_id")]
        public long? PartnerBankId { get; set; }

        [Column("observation")]
        public string Observation { get; set; }

        [Column("depositos_confirmar_id")]
        public long? DepositosConfirmarId { get; set; }

        [Column("resumen")]
        public string Resumen { get; set; }

        [Column("subclasificacion_gasto_id")]
        public long? SubclasificacionGastoId { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [Column("diferencia")]
        public decimal? Diferencia { get; set; }

        [Column("monto_aplicado_total")]
        public decimal? MontoAplicadoTotal { get; set; }

        [Column("move_descuadre_id")]
        public long? MoveDescuadreId { get; set; }

        [Column("motivos_no_cuadratura_id")]
        public long? MotivosNoCuadraturaId { get; set; }

        [Column("check_line_id")]
        public long? CheckLineId { get; set; }

        [Column("pf_check_line_id")]
        public long? PfCheckLineId { get; set; }

        // ---------- TARJETAS ----------

        [Column("card_voucher")]
        public string? CardVoucher { get; set; }

        [Column("lote_voucher")]
        public string? LoteVoucher { get; set; }

        [Column("card_bin_text")]
        public string CardBinText { get; set; }

        // ---------- OTROS ----------

        [Column("medio_pago_account_id")]
        public long? MedioPagoAccountId { get; set; }

        [Column("medio_pago_partner_id")]
        public long? MedioPagoPartnerId { get; set; }

        // ---------- CATÁLOGOS ----------

        [Column("card_bin_id")]
        public long? CardBinId { get; set; }

        [Column("card_id")]
        public int? CardId { get; set; }

        [Column("bank_tc_id")]
        public long? BankTcId { get; set; }

        [Column("payment_type_id")]
        public long? PaymentTypeId { get; set; }

        [Column("plan_id")]
        public long? PlanId { get; set; }

        // ---------- PLAZOS / COMISIÓN ----------

        [Column("installments")]
        public int? Installments { get; set; }

        [Column("commission_percent")]
        public decimal? CommissionPercent { get; set; }

        [Column("commission_amount")]
        public decimal? CommissionAmount { get; set; }

        [Column("net_amount")]
        public decimal? NetAmount { get; set; }

        [Column("voucher_id")]
        public long? VoucherId { get; set; }

        // ---------- RELACIONES MANY2MANY ----------

        //[Column("account_move_line_ids")]
        //public List<long> AccountMoveLineIds { get; set; }

        //[Column("account_move_line_search_ids")]
        //public List<long> AccountMoveLineSearchIds { get; set; }
        [Ignore]
        [JsonIgnore]
        public MultipleCobrosInvoiceLineAi[] lines { get; set; }
        [JsonIgnore]
        public int sequence { get; set; }

        [JsonIgnore]
        public string journal_name { get; set; }
    }
}
