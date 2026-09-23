using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using System.Linq;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountMoveLineDb : SqliteDbBase<account_move_line>
    {
        public AccountMoveLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        protected override async Task OnAfterInit()
        {
            await EnsureColumnAsync("_analitica_id", "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync("quantity_available_base", "REAL NOT NULL DEFAULT 0");

            await Database.RunInTransactionAsync(tran =>
            {

                tran.Execute(
                "CREATE INDEX IF NOT EXISTS idx_account_move_line__move_id ON account_move_line(_move_id)"
            );

                tran.Execute(
                    "CREATE INDEX IF NOT EXISTS idx_account_move_line__product_id ON account_move_line(_product_id)"
                );

                tran.Execute(
                    "CREATE INDEX IF NOT EXISTS idx_account_move_line__account_id ON account_move_line(_account_id)"
                );

                tran.Execute(
                    "CREATE INDEX IF NOT EXISTS idx_account_move_line__create_date ON account_move_line(create_date)"
                );

                tran.Execute(
                    "CREATE INDEX IF NOT EXISTS idx_account_move_line__write_date ON account_move_line(write_date)"
                );

                tran.Execute(
                    "CREATE INDEX IF NOT EXISTS idx_account_move_line__display_type ON account_move_line(display_type)"
                );

                tran.Execute(
                    "CREATE INDEX IF NOT EXISTS idx_account_move_line__move_id__display_type ON account_move_line(_move_id, display_type)"
                );
            });
        }

        public async Task<List<account_move_line>> GetItemsAsync(
            string name, 
            string display_type, 
            account_move[] account_Move_Parents, 
            int limit)
        {
            await Init();
            var resultTmp = await Database.Table<account_move_line>().Where(i=>i.display_type == display_type &&
            i.name.ToLower().Contains(name.ToLower())
            ).ToListAsync();

            return resultTmp.Where(i=>
            account_Move_Parents.Any(p => p.id == i._move_id)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move_line>> GetItemsAsync(
            int productId,
            account_move[] account_Move_Parents,
            int limit)
        {
            await Init();
            var resultTmp = await Database.Table<account_move_line>().
                Where(i => i._product_id == productId).ToListAsync();

            return resultTmp.Where(i =>
            account_Move_Parents.Any(p => p.id == i._move_id)
            ).Take(limit).ToList();
        }

        public async Task<List<account_move_line>> GetItemsByParentAsync(int move_id)
        {
            await Init();
            return await Database.Table<account_move_line>().Where(x=>x._move_id == move_id).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move_line_view>> GetByTopLines(res_partner resPartner)
        {
            await Init();

            string sql = @"
        SELECT 
            aml.*,
            pp.code AS product_code,
            pp.name AS product_name,
            am.docnum_mask AS docnum_mask
        FROM account_move_line aml
        JOIN account_move am
            ON am.id = aml._move_id
        JOIN product_product pp
            ON pp.id = aml._product_id
        JOIN (
                SELECT
                    aml2._product_id,
                    COUNT(*) AS usage_count,
                    MAX(am2.create_date) AS last_date
                FROM account_move_line aml2
                JOIN account_move am2
                    ON am2.id = aml2._move_id
                WHERE am2._partner_id = ? and
                aml2.quantity_available > 0
                GROUP BY aml2._product_id
                ORDER BY usage_count DESC, last_date DESC
                LIMIT 10
        ) top_products
            ON top_products._product_id = aml._product_id and
            aml.quantity_available > 0
        WHERE am._partner_id = ?
        GROUP BY aml._product_id
        ORDER BY am.create_date ASC
    ";

            return await Database.QueryAsync<account_move_line_view>(
                sql,
                resPartner.id,
                resPartner.id
            );
        }

        public async Task<List<account_move_line_view>> GetByProductName(string search, res_partner resPartner)
        {
            await Init();

            string sql = @"
        SELECT 
        aml.*,
        pp.code AS product_code,
        pp.name AS product_name,
        am.docnum_mask AS docnum_mask
    FROM account_move_line aml
    JOIN account_move am
        ON am.id = aml._move_id
    JOIN product_product pp 
        ON pp.id = aml._product_id
    WHERE
        (
            pp.name LIKE ?
            OR pp.code LIKE ?
            OR pp.id = ?
        )
        AND am._partner_id = ?
    ";

            int productId = 0;
            int.TryParse(search, out productId);

            return await Database.QueryAsync<account_move_line_view>(
                sql,
                $"%{search}%",
                $"%{search}%",
                productId,
                resPartner.id
            );
        }

        public async Task<account_move_line> GetItem(int id)
        {
            await Init();
            return await Database.Table<account_move_line>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertBatchAsync(account_move_line[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        private async Task EnsureColumnAsync(string columnName, string columnTypeSql)
        {
            var cols = await Database.QueryAsync<SqliteColumnInfo>(
                "PRAGMA table_info(account_move_line)");

            if (cols != null && cols.Any(c =>
                    string.Equals(c.name, columnName, StringComparison.OrdinalIgnoreCase)))
                return;

            await Database.ExecuteAsync(
                $"ALTER TABLE account_move_line ADD COLUMN {columnName} {columnTypeSql}");
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
    }
}
