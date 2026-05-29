using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubPaymentCheckInvoiceLink : HubBase
    {
        string[] fields_array = {
            "id",
            "display_name",
            "state",
            "create_date",
            "write_date"
        };

        public HubPaymentCheckInvoiceLink(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "payment.check.invoice.link";
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
    }
}
