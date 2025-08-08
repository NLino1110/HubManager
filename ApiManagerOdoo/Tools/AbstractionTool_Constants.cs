//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace AppManagerOdoo
{
    using DMSA.Models.Odoo.Native;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public partial class AbstractionTool
    {
        //Define el origen y destino de la estructura para la tabla FAC_NOTACREDITO_CAB
        public List<Dictionary<string, string>> fieldsMapper_FAC_NOTACREDITO_CAB = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "source_list", "company_id" },
                        { "source_field", "{id}" },
                        { "target_field", "CODEMPRESA" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "partner_id" },
                        { "source_field", "{id}" },
                        { "target_field", "CODCLIENTE" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "partner_id" },
                        { "source_field", "{name}" },
                        { "target_field", "DATOS_CLIENTE" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "none" },
                        { "target_value", "0" },
                        { "target_field", "TIPOCLIENTE" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "none" },
                        { "target_value", "0" },
                        { "target_field", "CODAGENCIA" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "none" },
                        { "target_value", "FAC" }, //TODO: extraer valor del api
                        { "target_field", "CODTIPOCMPR" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{id}" },
                        { "target_field", "NUMCMPRVENTA" },
                        { "numeric", "true" },
                        { "integer", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{name}" },
                        { "target_field", "NUMDOCUMENTO" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "invoice_user_id" },
                        { "source_field", "{id}-{name}" },
                        { "target_field", "VENDEDOR" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{amount_untaxed_signed}" },
                        { "target_field", "SUBTOTAL" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "none" },
                        { "target_value", "0" },
                        { "target_field", "DESCUENTO" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{amount_tax}" },
                        { "target_field", "IMPUESTO" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{amount_total}" },
                        { "target_field", "TOTAL" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{invoice_date}" },
                        { "target_field", "FECHAREGISTRO" },
                        //{ "datetime", "true" },
                    },
                };


        public List<Dictionary<string, string>> fieldsMapper_FAC_NOTACREDITO_DET = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{id}" },
                        { "target_field", "line_id" },
                        { "numeric", "true" },
                        { "integer", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "move_id" },
                        { "source_field", "{id}" },
                        { "target_field", "NUMCMPRVENTA" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{sequence}" },
                        { "target_field", "NUMCMPRVENTADET" },
                        { "numeric", "true" },
                        { "integer", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "product_id" },
                        { "source_field", "{id}" },
                        { "target_field", "CODARTICULO" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{name}" },
                        { "target_field", "ARTICULO" }
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{quantity}" },                        
                        { "target_field", "CANTIDAD" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{price_unit}" },                        
                        { "target_field", "PRECIO" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{price_subtotal}" },
                        { "target_field", "SUBTOTAL" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{discount_balance}" },
                        { "target_field", "DESCUENTO" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{discount_percentage}" },
                        { "target_field", "PORCDESCUENTO" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{price_total}" },
                        { "target_field", "TOTAL" },
                        { "numeric", "true" },
                    },
                    new Dictionary<string, string>
                    {
                        { "source_list", "none" },
                        { "source_field", "{display_type}" },
                        { "target_field", "display_type" }
                    },
                };

    }
}
