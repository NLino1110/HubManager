using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubAccountTax : HubBase
    {
        string[] fields_array = new[] {
                "id", 
                "name", 
                "type_tax_use", 
                "tax_scope", 
                "amount_type", 
                "active", 
                "company_id",             
                "sequence", 
                "amount", 
                "description", 
                "invoice_label", 
                "price_include", 
                "company_price_include", 
                "price_include_override", 
                "include_base_amount", 
                "is_base_affected", 
                "analytic", 
                "hide_tax_exigibility", 
                "tax_exigibility", 
                "cash_basis_transition_account_id", 
                "is_used", 
                "repartition_lines_str", 
                "invoice_legal_notes", 
                "has_negative_factor", 
                "display_name", 
                "l10n_ec_code_base", 
                "l10n_ec_code_applied", 
                "l10n_ec_code_ats", 
                "code_base", 
                "write_date", 
                "create_date"
            };

        public HubAccountTax(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.tax";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int[] ids)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<AccountTax[]>?> GetAll(int[] ids)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountTax[]>>(args, _custom_args, kwargs);
        }
    }
}
