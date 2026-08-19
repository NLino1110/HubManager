namespace DMSA.Models.Odoo.Native
{
    /// <summary>
    /// Respuesta parcial de Odoo web_read para saldos de res.partner (Cobranzas).
    /// </summary>
    public class res_partner_saldos_read
    {
        public int id { get; set; }
        public decimal saldo_vencido { get; set; }
        public decimal saldo_por_vencer { get; set; }
        public decimal saldo_a_favor { get; set; }
        public decimal saldo_total { get; set; }
        public decimal saldo_ch_posfechado { get; set; }
    }
}
