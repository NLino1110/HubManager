using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

using System.Data;


namespace ApiManager
{
    public class HubAccountAccount : HubBase
    {
        string[] fields_array = {
            "id",
            "company_currency_id",
            "company_fiscal_country_code",
            "code",
            "code_store",
            "placeholder_code",
            "deprecated",
            "used",
            "account_type",
            "include_initial_balance",
            "internal_group",
            "reconcile",
            "tax_ids",
            "company_ids",
            "code_mapping_ids",
            "tag_ids",
            "group_id",
            "root_id",
            "name",
            "display_name",
            "aplica_analitica",
            "analitica_requerido",
            "code_number",
            "create_date",
            "write_date"
        };

        public HubAccountAccount(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.account";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByIds(int[] ids)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByNames(string name)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "name", "ilike", name }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<AccountAccount[]>?> GetItemsById(int[] ids)
        {            
            int limit = 300;
            int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids }
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountAccount[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<AccountAccount[]>?> GetItemsByNames(string name)
        {
            int limit = 300;
            int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "name", "ilike", name }
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountAccount[]>>(args, _custom_args, kwargs, true);
        }
    }
}
