using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using SQLite;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountMoveDb : SqliteDbBase<account_move>
    {
        public AccountMoveDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        protected override async Task OnAfterInit()
        {
            await EnsureColumnAsync("pf_promised_amount", "REAL");
            await EnsureColumnAsync("is_nota_debito", "INTEGER");

            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_account_move_partner_company ON account_move(_partner_id, _company_id)");
            });
        }

        private async Task EnsureColumnAsync(string columnName, string columnTypeSql)
        {
            var cols = await Database.QueryAsync<SqliteColumnInfo>(
                "PRAGMA table_info(account_move)");

            if (cols != null && cols.Any(c =>
                    string.Equals(c.name, columnName, StringComparison.OrdinalIgnoreCase)))
                return;

            await Database.ExecuteAsync(
                $"ALTER TABLE account_move ADD COLUMN {columnName} {columnTypeSql}");
        }

        private string AccountMoveSyncDayKey =>
            $"account_move_sync_day_{DatabaseFilename}";

        public bool IsFirstAccountMoveSyncOfDay()
        {
            var lastSyncDay = Preferences.Get(AccountMoveSyncDayKey, string.Empty);
            return !string.Equals(
                lastSyncDay,
                DateTime.Now.ToString("yyyy-MM-dd"),
                StringComparison.Ordinal);
        }

        public void MarkAccountMoveSyncCompletedToday()
        {
            Preferences.Set(AccountMoveSyncDayKey, DateTime.Now.ToString("yyyy-MM-dd"));
        }

        /// <summary>Pruebas: quita el candado de “ya syncé hoy” para poder reintentar el ZIP.</summary>
        public void ClearAccountMoveSyncDay()
        {
            Preferences.Remove(AccountMoveSyncDayKey);
        }

        private sealed class SqliteColumnInfo
        {
            public int cid { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int notnull { get; set; }
            public string dflt_value { get; set; }
            public int pk { get; set; }
        }

        private sealed class PostdatedAmountRow
        {
            public int id { get; set; }
            public decimal pf_promised_amount { get; set; }
        }

        /// <summary>
        /// Columnas agregadas con ALTER TABLE pueden no mapearse bien en lecturas/escrituras ORM.
        /// Lee pf_promised_amount con SQL explícito y lo aplica al modelo en memoria.
        /// </summary>
        public async Task EnrichPostdatedAmountsAsync(IList<account_move> items)
        {
            if (items == null || items.Count == 0)
                return;

            await Init();

            var ids = items.Select(x => x.id).Distinct().ToList();
            if (ids.Count == 0)
                return;

            var placeholders = string.Join(",", ids.Select(_ => "?"));
            var rows = await Database.QueryAsync<PostdatedAmountRow>(
                $"SELECT id, COALESCE(pf_promised_amount, 0) AS pf_promised_amount FROM account_move WHERE id IN ({placeholders})",
                ids.Cast<object>().ToArray());

            var map = rows.ToDictionary(r => r.id, r => r.pf_promised_amount);
            foreach (var item in items)
            {
                if (map.TryGetValue(item.id, out var amount))
                    item.pf_promised_amount = amount;
            }
        }

        /// <summary>
        /// Garantiza persistir columnas agregadas vía ALTER (pf_promised_amount, is_nota_debito).
        /// </summary>
        public new async Task<int> InsertBatchAsync(IEnumerable<account_move> items)
        {
            await Init();

            var list = (items ?? Enumerable.Empty<account_move>()).ToList();
            if (list.Count == 0)
                return 0;

            await Database.InsertAllAsync(list, "OR REPLACE", true);

            await Database.RunInTransactionAsync(tran =>
            {
                foreach (var item in list)
                {
                    tran.Execute(
                        @"UPDATE account_move SET
                            pf_promised_amount = ?,
                            is_nota_debito = ?
                          WHERE id = ?",
                        item.pf_promised_amount,
                        item.is_nota_debito ? 1 : 0,
                        item.id);
                }
            });

            return 0;
        }

        public new async Task<account_move> GetItemAsync(Expression<Func<account_move, bool>> predicate)
        {
            var item = await base.GetItemAsync(predicate);
            if (item != null)
                await EnrichPostdatedAmountsAsync(new List<account_move> { item });

            return item;
        }
                
        public async Task<List<account_move>> GetItemsAsync(int company_id, int res_partner_id, string Search, int limit)
        {
            await Init();

            List<account_move> items;

            if (!Search.Equals(string.Empty) && Search != "")
            {
                items = await Database.Table<account_move>().Where(x =>
                    x._company_id == company_id &&
                    x._partner_id == res_partner_id &&
                    (x.move_type == "out_invoice" || x.move_type == "out_refund") &&
                    (x.name.Contains(Search) || x.docnum_mask.Contains(Search))
                ).
                OrderByDescending(o => o.invoice_date).
                ToListAsync();
            }
            else
            {
                items = await Database.Table<account_move>().Where(x =>
                    x._company_id == company_id &&
                    x._partner_id == res_partner_id &&
                    (x.move_type == "out_invoice" || x.move_type == "out_refund")
                ).
                OrderByDescending(o => o.invoice_date).
                ToListAsync();
            }

            return items
                .Where(AccountMoveDocumentDisplay.HasOpenBalanceForBalanceView)
                .Take(limit)
                .ToList();
        }

        public async Task<List<account_move>> GetItemsAsync(account_move_line[] lines, int company_id, int partner_id, int limit)
        {
            await Init();

            var previuResult = await Database.Table<account_move>().Where(i => i._company_id == company_id &&
            i._partner_id == partner_id).Take(limit).ToListAsync();

            return previuResult.Where(i =>
            lines.Any(p => p._move_id == i.id)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move>> GetItemsAsync(int company_id, int partner_id, int limit)
        {
            await Init();
            return await Database.Table<account_move>().Where(i => i._company_id == company_id &&
            i._partner_id == partner_id).Take(limit).ToListAsync();
        }

        public async Task<List<account_move>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<account_move>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move>> GetItemsByPartnerForPaymentAsync(res_partner res_Partner)
        {
            await Init();
            return await Database.Table<account_move>().Where(x=>
            x._partner_id == res_Partner.id &&
            x._company_id == res_Partner._company_id &&
            x.move_type == "out_invoice" &&
            x.amount_residual > 0)
            .OrderBy(x => x.invoice_date)
            .ThenBy(x => x.id)
            .ToListAsync();
        }

        // Cobros / TOT. FACT. PEND.: solo facturas y notas de débito (out_invoice) con saldo > 0.
        public async Task<List<account_move>> GetItemsWithBalanceByPartnerAsync(
            int company_id,
            int partner_id,
            int? limit = null)
        {
            await Init();

            var items = await Database.Table<account_move>().Where(x =>
                x._company_id == company_id &&
                x._partner_id == partner_id &&
                x.move_type == "out_invoice" &&
                x.amount_residual > 0)
                .ToListAsync();

            await EnrichPostdatedAmountsAsync(items);
            return ApplyBalanceLimit(items, limit);
        }

        // Ver saldos (PopupSelectInvoice): facturas, ND y NCRE (out_refund). advance solo se sincroniza, no se lista aquí.
        public async Task<List<account_move>> GetItemsWithBalanceByPartnerForBalanceViewAsync(
            int company_id,
            int partner_id,
            int? limit = null)
        {
            await Init();

            var items = await Database.Table<account_move>().Where(x =>
                x._company_id == company_id &&
                x._partner_id == partner_id)
                .ToListAsync();

            var filtered = items.Where(AccountMoveDocumentDisplay.HasOpenBalanceForBalanceView).ToList();
            await EnrichPostdatedAmountsAsync(filtered);
            return ApplyBalanceLimit(filtered, limit);
        }

        static List<account_move> ApplyBalanceLimit(List<account_move> items, int? limit)
        {
            IEnumerable<account_move> ordered = items
                .OrderBy(x => x.invoice_date)
                .ThenBy(x => x.id);

            if (limit.HasValue)
                return ordered.Take(limit.Value).ToList();

            return ordered.ToList();
        }

        public async Task<List<account_move>> GetItemsByPartnerAndCompany(res_partner res_Partner, res_company res_Company)
        {
            await Init();
            return await Database.Table<account_move>().Where(x =>
            x._partner_id == res_Partner.id &&
            x._company_id == res_Company.id &&
            x.move_type == "out_invoice" &&
            x.payment_state != "paid" &&
            (x.mcl_check_id == 0 || x.mcl_check_id == null ) &&
            x.amount_residual > 0).ToListAsync();            
        }

        //public async Task<List<account_move>> GetItemsByPartnerAsync(res_partner res_Partner)
        //{
        //    await Init();
        //    return await Database.Table<account_move>().Where(x =>
        //    x._partner_id == res_Partner.id &&
        //    x.move_type == "out_invoice" &&
        //    x.amount_residual > 0).ToListAsync();
        //    //return Database.Table<account_journal>().ToList();
        //}


        public async Task<account_move> GetByNameItem(string name_doc)
        {
            await Init();
            return await Database.Table<account_move>().Where(i => i.name == name_doc).FirstOrDefaultAsync();
        }

        // Reintento cabecera: MAX(invoice_date) local (orden Odoo invoice_date asc, id asc).
        public async Task<DateTime?> GetMaxInvoiceDateOrNullAsync()
        {
            await Init();

            try
            {
                return await Database.ExecuteScalarAsync<DateTime?>(
                    "SELECT MAX(invoice_date) FROM account_move");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMaxInvoiceDateOrNullAsync: {ex.Message}");
                return null;
            }
        }

        // IDs de account_move local en rango invoice_date (cabeceras ya sincronizadas).
        public async Task<int[]> GetIdsByInvoiceDateRangeAsync(DateTime dateFrom, DateTime dateTo)
        {
            await Init();

            var from = dateFrom.Date;
            var to = dateTo.Date;

            var items = await Database.Table<account_move>()
                .Where(x => x.invoice_date >= from && x.invoice_date <= to)
                .OrderBy(x => x.id)
                .ToListAsync();

            return items.Select(x => x.id).ToArray();
        }

        // Distinct partner_sale_id en account_move (vendedores referenciados en facturas).
        public async Task<int[]> GetDistinctPartnerSaleIdsAsync(
            DateTime? invoiceDateFrom = null,
            DateTime? invoiceDateTo = null)
        {
            await Init();

            var items = await Database.Table<account_move>().ToListAsync();

            if (invoiceDateFrom.HasValue && invoiceDateTo.HasValue)
            {
                var from = invoiceDateFrom.Value.Date;
                var to = invoiceDateTo.Value.Date;
                items = items
                    .Where(x => x.invoice_date >= from && x.invoice_date <= to)
                    .ToList();
            }

            return items
                .Where(x => x._partner_sale_id > 0)
                .Select(x => x._partner_sale_id)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }
    }
}
