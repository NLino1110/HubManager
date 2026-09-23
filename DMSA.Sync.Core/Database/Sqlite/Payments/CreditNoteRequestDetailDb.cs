using DMSA.Models.Odoo.Accounting;
using System.Linq;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class CreditNoteRequestDetailDb : SqliteDbBase<credit_note_request_detail>
    {
        public CreditNoteRequestDetailDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        protected override async Task OnAfterInit()
        {
            await EnsureColumnAsync("client_permanently_closing", "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync("_uom_id", "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync("invoice_line_uom_id", "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync("quantity_available_invoice", "REAL NOT NULL DEFAULT 0");
            await EnsureColumnAsync("quantity_available_base", "REAL NOT NULL DEFAULT 0");
            await EnsureColumnAsync("quantity_available_by_uom", "REAL NOT NULL DEFAULT 0");
            await EnsureColumnAsync("discount", "REAL NOT NULL DEFAULT 0");
            await EnsureColumnAsync("analitica_id", "INTEGER NOT NULL DEFAULT 0");
        }

        private async Task EnsureColumnAsync(string columnName, string columnTypeSql)
        {
            var cols = await Database.QueryAsync<SqliteColumnInfo>(
                "PRAGMA table_info(credit_note_request_detail)");

            if (cols != null && cols.Any(c =>
                    string.Equals(c.name, columnName, StringComparison.OrdinalIgnoreCase)))
                return;

            await Database.ExecuteAsync(
                $"ALTER TABLE credit_note_request_detail ADD COLUMN {columnName} {columnTypeSql}");
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

        public async Task<List<credit_note_request_detail>> GetItemsAsync(credit_note_request parent)
        {
            await Init();            
            return (await Database.Table<credit_note_request_detail>().ToListAsync()).Where(pl => pl.parent_id == parent.id).ToList();
        }

        public async Task<int> DeleteItemOfParent(credit_note_request parent)
        {
            await Init();
            int count = 0;
            //await Init();
            var resultItems = (await Database.Table<credit_note_request_detail>().ToListAsync()).Where(i => i.parent_id == parent.id);
            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }

        public async Task<List<credit_note_request_detail>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<credit_note_request_detail>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<int> DeleteByParentAsync(int parent_id)
        {
            await Init();
            return await Database.Table<credit_note_request_detail>().DeleteAsync(x => x.parent_id == parent_id);
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<credit_note_request_detail>> GetItemsByParentAsync(int parent_id)
        {
            await Init();
            return await Database.Table<credit_note_request_detail>().Where(x=>x.parent_id == parent_id).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        //public async Task<credit_note_request_detail> GetItem(int id)
        //{
        //    await Init();
        //    return await Database.Table<credit_note_request_detail>().Where(i => i.line_id == id).FirstOrDefaultAsync();
        //}

        public async Task<int> InsertBatchAsync(credit_note_request_detail[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }
    }
}
