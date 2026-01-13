using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.DebitCollection
{
    [Table("multiple_cobros_invoice_line")]
    public class MultipleCobrosInvoiceLine : OdooEntity
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company_id")]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [JsonProperty("multiple_cobros_invoice_id")]
        [Column("multiple_cobros_invoice_id")]
        public int MultipleCobrosInvoiceId { get; set; }

        [JsonProperty("type")]
        [Column("type")]
        public string Type { get; set; }

        [JsonProperty("acc_number")]
        [Column("acc_number")]
        public string AccNumber { get; set; }

        [JsonProperty("bank_id")]
        [Column("bank_id")]
        public long? BankId { get; set; }

        [JsonProperty("journal_id")]
        [Column("journal_id")]
        public long? JournalId { get; set; }

        [JsonProperty("checkbook_id")]
        [Column("checkbook_id")]
        public long? CheckbookId { get; set; }

        [JsonProperty("checkbook_line_id")]
        [Column("checkbook_line_id")]
        public long? CheckbookLineId { get; set; }

        [JsonProperty("payment_id")]
        [Column("payment_id")]
        public long? PaymentId { get; set; }

        [JsonProperty("number_check_text")]
        [Column("number_check_text")]
        public string NumberCheckText { get; set; }

        [JsonProperty("withdrawal_date")]
        [Column("withdrawal_date")]
        public DateTime? WithdrawalDate { get; set; }

        [JsonProperty("amount")]
        [Column("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("amount_in_words")]
        [Column("amount_in_words")]
        public string AmountInWords { get; set; }

        [JsonProperty("circular")]
        [Column("circular")]
        public string Circular { get; set; }

        [JsonProperty("partner_id")]
        [Column("partner_id")]
        public long? PartnerId { get; set; }

        [JsonProperty("acc_holder_name")]
        [Column("acc_holder_name")]
        public string AccHolderName { get; set; }

        [JsonProperty("city_id")]
        [Column("city_id")]
        public long? CityId { get; set; }

        [JsonProperty("payment_date")]
        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [JsonProperty("reconciled_date")]
        [Column("reconciled_date")]
        public DateTime? ReconciledDate { get; set; }

        [JsonProperty("state")]
        [Column("state")]
        public string State { get; set; }

        [JsonProperty("partner_type")]
        [Column("partner_type")]
        public string PartnerType { get; set; }

        [JsonProperty("payment_type")]
        [Column("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("type_invoice")]
        [Column("type_invoice")]
        public string TypeInvoice { get; set; }

        [JsonProperty("amount_total")]
        [Column("amount_total")]
        public decimal? AmountTotal { get; set; }

        [JsonProperty("partner_bank_id")]
        [Column("partner_bank_id")]
        public long? PartnerBankId { get; set; }

        [JsonProperty("observation")]
        [Column("observation")]
        public string Observation { get; set; }

        [JsonProperty("depositos_confirmar_id")]
        [Column("depositos_confirmar_id")]
        public long? DepositosConfirmarId { get; set; }

        [JsonProperty("resumen")]
        [Column("resumen")]
        public string Resumen { get; set; }

        [JsonProperty("subclasificacion_gasto_id")]
        [Column("subclasificacion_gasto_id")]
        public long? SubclasificacionGastoId { get; set; }

        [JsonProperty("user_id")]
        [Column("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("diferencia")]
        [Column("diferencia")]
        public decimal? Diferencia { get; set; }

        [JsonProperty("monto_aplicado_total")]
        [Column("monto_aplicado_total")]
        public decimal? MontoAplicadoTotal { get; set; }

        [JsonProperty("move_descuadre_id")]
        [Column("move_descuadre_id")]
        public long? MoveDescuadreId { get; set; }

        [JsonProperty("motivos_no_cuadratura_id")]
        [Column("motivos_no_cuadratura_id")]
        public long? MotivosNoCuadraturaId { get; set; }

        [JsonProperty("check_line_id")]
        [Column("check_line_id")]
        public long? CheckLineId { get; set; }

        [JsonProperty("pf_check_line_id")]
        [Column("pf_check_line_id")]
        public long? PfCheckLineId { get; set; }

        // ---------- TARJETAS ----------

        [JsonProperty("card_voucher")]
        [Column("card_voucher")]
        public string? CardVoucher { get; set; }

        [JsonProperty("lote_voucher")]
        [Column("lote_voucher")]
        public string? LoteVoucher { get; set; }

        [JsonProperty("card_bin_text")]
        [Column("card_bin_text")]
        public string CardBinText { get; set; }

        // ---------- OTROS ----------

        [JsonProperty("medio_pago_account_id")]
        [Column("medio_pago_account_id")]
        public long? MedioPagoAccountId { get; set; }

        [JsonProperty("medio_pago_partner_id")]
        [Column("medio_pago_partner_id")]
        public long? MedioPagoPartnerId { get; set; }

        // ---------- CATÁLOGOS ----------

        [JsonProperty("card_bin_id")]
        [Column("card_bin_id")]
        public long? CardBinId { get; set; }

        [JsonProperty("card_id")]
        [Column("card_id")]
        public int? CardId { get; set; }

        [JsonProperty("bank_tc_id")]
        [Column("bank_tc_id")]
        public long? BankTcId { get; set; }

        [JsonProperty("payment_type_id")]
        [Column("payment_type_id")]
        public long? PaymentTypeId { get; set; }

        [JsonProperty("plan_id")]
        [Column("plan_id")]
        public long? PlanId { get; set; }

        // ---------- PLAZOS / COMISIÓN ----------

        [JsonProperty("installments")]
        [Column("installments")]
        public int? Installments { get; set; }

        [JsonProperty("commission_percent")]
        [Column("commission_percent")]
        public decimal? CommissionPercent { get; set; }

        [JsonProperty("commission_amount")]
        [Column("commission_amount")]
        public decimal? CommissionAmount { get; set; }

        [JsonProperty("net_amount")]
        [Column("net_amount")]
        public decimal? NetAmount { get; set; }

        [JsonProperty("voucher_id")]
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

        [Ignore]
        [JsonIgnore]
        public List<object> lines_obj { get; set; }

        [JsonIgnore]
        public int sequence { get; set; }

        [JsonIgnore]
        public string journal_name { get; set; }


        //[JsonProperty("bank_account_id")]
        //[Column("bank_account_id")]
        //public int? BankAccountId { get; set; }
    }
}
