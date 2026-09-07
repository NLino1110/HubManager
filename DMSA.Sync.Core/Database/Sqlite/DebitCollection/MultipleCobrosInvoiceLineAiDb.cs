using DMSA.Models.Odoo.DebitCollection;

namespace DMSA.Sync.Core.Database.Sqlite.DebitCollection
{
    public class MultipleCobrosInvoiceLineAiDb : SqliteDbBase<MultipleCobrosInvoiceLineAi>
    {
        public MultipleCobrosInvoiceLineAiDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        protected override async Task OnAfterInit()
        {
            await EnsureColumnAsync("is_nota_debito", "INTEGER");
        }

        private async Task EnsureColumnAsync(string columnName, string columnTypeSql)
        {
            var cols = await Database.QueryAsync<SqliteColumnInfo>(
                "PRAGMA table_info(multiple_cobros_invoice_line_ai)");

            if (cols != null && cols.Any(c =>
                    string.Equals(c.name, columnName, StringComparison.OrdinalIgnoreCase)))
                return;

            await Database.ExecuteAsync(
                $"ALTER TABLE multiple_cobros_invoice_line_ai ADD COLUMN {columnName} {columnTypeSql}");
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

        public async Task<int> DeleteItemOfParent(MultipleCobrosInvoiceLine parent)
        {
            await Init();
            int count = 0;
            var resultItems = (await Database.Table<MultipleCobrosInvoiceLineAi>().ToListAsync()).Where(i => i.multiple_cobros_invoice_line_id == parent.Id);
            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }
    }
}
