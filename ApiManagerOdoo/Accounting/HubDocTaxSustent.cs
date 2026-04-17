using System.Reflection;
using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json;

namespace ApiManagerOdoo.Accounting
{
    public class HubDocTaxSustent : HubBase
    {
        string[] fields_array = new[] { 
            "id", 
            "name", 
            "code",
            "active",
            "description",
            "display_name", 
            "create_date", 
            "write_date" 
        };

        private readonly Type _type;
        public HubDocTaxSustent(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "doc.tax.sustent";

            _type = typeof(doc_tax_sustent);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni, string code)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] { "code", "=", code }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<doc_tax_sustent[]>?> GetItems(DateTime dateIni, int limit, int index, string code)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = { };
            object[] _custom_args = {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] { "code", "=", code }
            };
            return await SearchRead<ApiResponseOdooRpcT<doc_tax_sustent[]>>(args, _custom_args, kwargs, true);
        }
    }
}
