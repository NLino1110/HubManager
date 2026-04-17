using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubReceiptReceiptsLine : HubBase
    {
        string[] fields_array = {
            "id",
            "company_id",
            "receipt_receipts_id",
            "name",
            "number_seq",
            "payment_date",
            "withdrawal_date",
            "reconciled_date",
            "amount",
            "amount_in_words",
            "note",
            "number_check_text",
            "payment_id",
            "sale_user_id",
            "display_name",
            "create_date",
            "write_date",
            "state"
        };

        public HubReceiptReceiptsLine(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "receipt.receipts.line";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd") },                
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<ReceiptReceiptsLine[]>?> GetBySaleUser(int sale_user_id, DateTime dateIni, int limit, int index)
        { 
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] { "sale_user_id", "=", sale_user_id },
            };
            return await SearchRead<ApiResponseOdooRpcT<ReceiptReceiptsLine[]>>(args, _custom_args, kwargs, true);
        }
    }
}
