using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubResPartnerSafe : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "vat",
                "vat_doc",
                "name",
                "display_name",
                "email",
                "phone",
                "mobile",
                "street",
                "street2",
                "zip",
                "city",
                "create_date",
                "write_date",
                "order_number",
                "agency_code",
                "client_code",
                "company_code",
                "seller_code",
                "user_code",
                "application_origin",
                "application_version",
                "platform_origin",
                "platform_modified",
                "device_brand",
                "device_model",
                "state_code"
        };

        public HubResPartnerSafe(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.partner.safe";
        }

        public async Task<ApiResponseOdooRpcT<res_partner_safe[]>?> GetByIds(int limit, int index, int[] ids)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"id", "in", ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner_safe[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<res_partner_safe[]>?> GetByWriteDate(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner_safe[]>>(args, _custom_args, kwargs, true);
        }
    }
}
