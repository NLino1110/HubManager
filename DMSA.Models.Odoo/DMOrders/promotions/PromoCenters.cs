using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Sales;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    // Tabla equivalente a 'promo.centers' en Odoo
    [Table("promo_centers")]
    public class PromoCenters : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        // --- Many2one: promo_id -> promotion.benefit ---
        [Ignore]
        [JsonProperty("promo_id")]
        public JToken promo_id { get; set; }

        [JsonIgnore]
        public int _promo_id
        {
            get => GetId(promo_id);
            set => promo_id = SetId(promo_id, value);
        }

        // --- Many2many: res_center_id -> res.center ---
        [Ignore]
        [JsonProperty("res_center_id")]
        public JToken res_center_id { get; set; }

        [JsonIgnore]
        public int[] _res_center_id
        {
            get => GetIds(res_center_id);
            set => res_center_id = SetIds(res_center_id, value);
        }

        // --- Many2many: levels_ids -> product.pricelist ---
        [Ignore]
        [JsonProperty("levels_ids")]
        public JToken levels_ids { get; set; }

        [JsonIgnore]
        public List<product_pricelist> _levels_ids
        {
            //get => GetIds(levels_ids);
            //set => levels_ids = SetIds(levels_ids, value);
            get => Array.Empty<product_pricelist>().ToList();
        }

        // --- Many2many: payment_method_ids -> pos.payment.method ---
        [Ignore]
        [JsonProperty("payment_method_ids")]
        public JToken payment_method_ids { get; set; }

        [JsonIgnore]
        public int[] _payment_method_ids
        {
            get => GetIds(payment_method_ids);
            set => payment_method_ids = SetIds(payment_method_ids, value);
        }

        // --- Many2many: pos_plazos_banco_ids -> pos.plazos.banco ---
        [Ignore]
        [JsonProperty("pos_plazos_banco_ids")]
        public JToken pos_plazos_banco_ids { get; set; }

        [JsonIgnore]
        public int[] _pos_plazos_banco_ids
        {
            get => GetIds(pos_plazos_banco_ids);
            set => pos_plazos_banco_ids = SetIds(pos_plazos_banco_ids, value);
        }

        // --- Campos simples ---
        [JsonProperty("times_inv")]
        public int times_inv { get; set; }

        [JsonProperty("bank_terms_apply")]
        public bool bank_terms_apply { get; set; } = false;

        // ----------------------------------------------------------------------
        // Nota: Se asume que OdooEntity implementa:
        //   int    GetId(JToken token)
        //   JToken SetId(JToken token, int id)
        //   int[]  GetIds(JToken token)          // para Many2many
        //   JToken SetIds(JToken token, int[] v) // para Many2many
        // Si aún no tienes GetIds/SetIds, dime y te paso una implementación segura.
        // ----------------------------------------------------------------------
    }
}
