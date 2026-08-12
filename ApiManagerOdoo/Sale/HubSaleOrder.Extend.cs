using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Promotions.Wizard;
using DMSA.Models.Odoo.Sales;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Diagnostics;

namespace ApiManagerOdoo.Sale
{
    public partial class HubSaleOrder
    {        
        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetListByParameter(int[] center_ids, 
            string TextSearch, 
            DateTime date, 
            int limit, 
            int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "center_id.id", "in", center_ids},

                 "|",
                new object[] {"res_partner.name", "ilike", TextSearch},
                new object[] {"res_partner.vat_doc", "ilike", TextSearch},

                new object[] {"write_date", ">=", date.ToString("yyyy-MM-dd 00:00:00") },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, _custom_args, kwargs, true);
        }

        /// <summary>
        /// Busca una orden en Odoo por external_guid (recuperación tras error de duplicado).
        /// </summary>
        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetByExternalGuid(string externalGuid)
        {
            if (string.IsNullOrWhiteSpace(externalGuid))
                return null;

            var kwargs = new
            {
                limit = 1,
                offset = 0,
                fields = new[] { "id", "name", "external_guid", "state" }
            };

            object[] args = Array.Empty<object>();
            object[] customArgs = new object[]
            {
                new object[] { "external_guid", "=", externalGuid }
            };

            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, customArgs, kwargs, true);
        }
    }
}
