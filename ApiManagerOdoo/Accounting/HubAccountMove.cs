using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManagerOdoo.Accounting
{
    public class HubAccountMove : HubBase
    {  
        // Misma lista que el ZIP Odoo (AccountDocumentSyncFields.Header).
        string[] fields_array => AccountDocumentSyncFields.Header;
        public HubAccountMove(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/connect/get_account_move";
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move";
        }

        private static object[] BuildHeaderDomain(int year, int month, int day, bool firstSyncOfDay)
        {
            string writeDate = $"{year}-{month:00}-{day:00} 00:00:00";
            string writeOp = firstSyncOfDay ? "<=" : ">=";

            return new object[] {
                new object[] { "write_date", writeOp, writeDate },
                new object[] { "state", "=", "posted" },
                new object[] { "invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain())
            .ToArray();
        }

        private static object[] BuildHeaderDomainByInvoiceDateRange(DateTime dateFrom, DateTime dateTo, int[] partnerIds)
        {
            var domain = new object[] {
                new object[] { "invoice_date", ">=", dateFrom.ToString("yyyy-MM-dd") },
                new object[] { "invoice_date", "<=", dateTo.ToString("yyyy-MM-dd") },
                new object[] { "state", "=", "posted" },
                new object[] { "invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain());

            // Vendedor: partner_id IN cartera local. Admin: partnerIds vacío → sin este filtro.
            if (partnerIds != null && partnerIds.Length > 0)
            {
                domain = domain.Concat(new object[]
                {
                    new object[] { "partner_id", "in", partnerIds }
                });
            }

            return domain.ToArray();
        }

        // partnerIds: IDs de res_partner local; filtra account.move por cartera del comercial.
        public async Task<ApiResponseOdooRpc?> GetHeaderCountByInvoiceDateRange(
            DateTime dateFrom, DateTime dateTo, int[] partnerIds)
        {
            object[] args = new object[] { };
            object[] _custom_args = BuildHeaderDomainByInvoiceDateRange(dateFrom.Date, dateTo.Date, partnerIds);
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByInvoiceDateRange(
            DateTime dateFrom, DateTime dateTo, int limit, int index, int[] partnerIds)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "invoice_date asc, id asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = BuildHeaderDomainByInvoiceDateRange(dateFrom.Date, dateTo.Date, partnerIds);
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetHeaderCount(int year, int month, int day, bool firstSyncOfDay = true)
        {
            object[] args = new object[] { };
            object[] _custom_args = BuildHeaderDomain(year, month, day, firstSyncOfDay);
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMoves(
            DateTime dateIni, int limit, int index, bool firstSyncOfDay = true)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = BuildHeaderDomain(
                dateIni.Year, dateIni.Month, dateIni.Day, firstSyncOfDay);
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMove(int[] ids)
        {
            var kwargs = new
            {                
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "id", "in", ids }
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetHeaderCountByDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            //string _custom_args = $"[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByDate(int year, int month, int day, int limit, int index)
        {
            //string fields = "fields=['id','name','partner_id','invoice_date','invoice_date_due','payment_state','move_type','journal_id','amount_residual','amount_untaxed_signed','amount_total_signed','amount_total','amount_tax','l10n_latam_document_type_id','invoice_user_id','company_id','team_id','invoice_line_ids','reversed_entry_id','ref','refund_invoice_ids','printer_id']";
            
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },                
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpc?> GetHeaderCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            //string _custom_args = $"[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await GetCount(args, _custom_args);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByWriteDate(int year, int month, int day, int limit, int index)
        {   
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByWriteDate_dl(int year, int month, int day, int limit, int index)
        {
            string[] fields_array = {
            "id",
            "write_date"
            };

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);

        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetByCreateDate_dl(int year, int month, int day, int limit, int index)
        {
            string[] fields_array = {
            "id",
            "create_date",
            "write_date"
            };

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        // Sync por cliente y masiva: out_invoice, out_refund, advance.
        public async Task<ApiResponseOdooRpc?> GetCountByResPartner(int res_partner)
        {            

            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                 new object[] { "partner_id", "=", res_partner },
                new object[] { "state", "=", "posted" },
                new object[] { "invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain())
            .ToArray();
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByResPartner(int res_partner, int limit, int index)
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
                new object[] { "partner_id", "=", res_partner },
                new object[] { "state", "=", "posted" },
                new object[] { "invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain())
            .ToArray();
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

    }
}
