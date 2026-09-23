using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json;
using RestSharp;
using static System.Net.Mime.MediaTypeNames;

namespace ApiManagerOdoo.Accounting
{
    public class HubAccountMoveLine : HubBase
    {
        // Misma lista que el ZIP Odoo (AccountDocumentSyncFields.Line).
        string[] fields_array => AccountDocumentSyncFields.Line;

        public HubAccountMoveLine(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move.line";
        }

        static object[] BuildLineWriteDateDomain(DateTime dateIni, bool firstSyncOfDay)
        {
            string writeDate = $"{dateIni.Year}-{dateIni.Month:00}-{dateIni.Day:00} 00:00:00";
            string writeOp = firstSyncOfDay ? "<=" : ">=";

            return new object[] {
                new object[] { "write_date", writeOp, writeDate },
                new object[] { "invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain("move_id.move_type"))
            .ToArray();
        }

        public async Task<ApiResponseOdooRpc?> GetDetailCount(DateTime dateIni, bool firstSyncOfDay = true)
        {
            object[] args = new object[] { };
            object[] _custom_args = BuildLineWriteDateDomain(dateIni, firstSyncOfDay);
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetDetailCountByInvoiceDateRange(
            DateTime dateFrom, DateTime dateTo, int[] moveIds, DateTime? writeDateFrom = null)
        {
            object[] args = new object[] { };
            object[] _custom_args = BuildLineDomainByInvoiceDateRange(dateFrom.Date, dateTo.Date, moveIds, writeDateFrom);
            return await GetCount(args, _custom_args);
        }

        // moveIds: IDs de account_move local; filtra líneas por move_id IN + rango invoice_date del move.
        // writeDateFrom: reintento detalle — write_date >= última línea local (orden write_date asc).
        private static object[] BuildLineDomainByInvoiceDateRange(
            DateTime dateFrom, DateTime dateTo, int[] moveIds, DateTime? writeDateFrom = null)
        {
            var domain = new object[] {
                new object[] { "move_id.invoice_date", ">=", dateFrom.ToString("yyyy-MM-dd") },
                new object[] { "move_id.invoice_date", "<=", dateTo.ToString("yyyy-MM-dd") },
                new object[] { "move_id.invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain("move_id.move_type"));

            // Vendedor: move_id IN cabeceras locales. Admin: moveIds vacío → sin este filtro.
            if (moveIds != null && moveIds.Length > 0)
            {
                domain = domain.Concat(new object[]
                {
                    new object[] { "move_id", "in", moveIds }
                });
            }

            if (writeDateFrom.HasValue)
            {
                domain = domain.Concat(new object[]
                {
                    new object[] { "write_date", ">=", writeDateFrom.Value.ToString("yyyy-MM-dd HH:mm:ss") }
                });
            }

            return domain.ToArray();
        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByInvoiceDateRange(
            DateTime dateFrom, DateTime dateTo, int limit, int index, int[] moveIds, DateTime? writeDateFrom = null)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = BuildLineDomainByInvoiceDateRange(dateFrom.Date, dateTo.Date, moveIds, writeDateFrom);
            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);
        }
        
        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLines(DateTime dateIni)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "date", ">=", $"{dateIni.ToString("yyyy-MM-dd")}" },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain("move_id.move_type"))
            .ToArray();

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLines(
            DateTime dateIni,
            int limit,
            int index,
            bool firstSyncOfDay = true)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = BuildLineWriteDateDomain(dateIni, firstSyncOfDay);

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByMove(int move_id)
        {
            var kwargs = new
            {                
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "move_id", "=", move_id },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);

        }

        public async Task<ApiResponseOdooRpc?> GetCountByIds(int[] ids)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLineByIds(int[] ids, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "id", "in", ids },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByMove(int move_id)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "move_id", "=", move_id }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByMove(int move_id, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "move_id", "=", move_id },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);

        }
    }
}
