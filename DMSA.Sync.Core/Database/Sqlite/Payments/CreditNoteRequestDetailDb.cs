using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class CreditNoteRequestDetailDb : SqliteDbBase<credit_note_request_detail>
    {
        public CreditNoteRequestDetailDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

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
