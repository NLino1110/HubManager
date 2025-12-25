using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Native.Inventory;
using DMSA.Models.Odoo.Specials;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class MnsaAttachmentDb : SqliteDbBase<mnsa_attachment>
    {
        public MnsaAttachmentDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            Task.Run(async () =>
            {
                await InitializeAsync();
            });
        }

        public async Task InitializeAsync()
        {
            await Init();
        }

        public async Task<mnsa_attachment> GetLastUpdate()
        {
            await Init();
            var q = Database.Table<mnsa_attachment>();
            q.OrderByDescending(x => x.date_data_cutoff);
            return await q.FirstOrDefaultAsync();
        }
    }
}
