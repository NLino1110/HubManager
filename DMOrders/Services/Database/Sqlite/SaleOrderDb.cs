using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMOrders.Services.Database.Sqlite
{
    public class SaleOrderDb
    {
        SQLiteAsyncConnection Database;

        public SaleOrderDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<sale_order>().ToListAsync()).Count;
        }
                
        public async Task<List<sale_order>> GetItemsAsync(int company_id)
        {
            await Init();
            return await Database.Table<sale_order>()
                .Where(x => x._company_id == company_id)
                .ToListAsync();
        }

        public async Task<List<sale_order>> GetItemsAsync(int partner_id, int company_id)
        {
            await Init();
            return await Database.Table<sale_order>()
                .Where(x => x._partner_id == partner_id && x._company_id == company_id)
                .ToListAsync();
        }

        public async Task<sale_order> GetItem(int id)
        {
            await Init();
            return await Database.Table<sale_order>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(sale_order item)
        {
            await Init();
            return await Database.InsertAsync(item);
        }

        public async Task<int> InsertBatchAsync(sale_order[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();
            return await Database.DeleteAllAsync<sale_order>();
        }

        public async Task<int> UpdateAsync(sale_order item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> DeleteRecursive(sale_order parent)
        {
            await Init();
            int count = 0;

            var resultItemsMove = (await Database.Table<sale_order>().ToListAsync()).Where(i => i.id == parent.id);

            foreach (var moveItem in resultItemsMove)
            {
                var resultItems = (await Database.Table<sale_order_line>().ToListAsync()).Where(i => i._order_id == moveItem.id);

                //Elimina detalles
                foreach (var item in resultItems)
                {
                    count++;
                    await Database.DeleteAsync(item);
                }

                //Eliminar movimientos
                await Database.DeleteAsync(moveItem);
            }

            //Elimina cabecera
            await Database.DeleteAsync(parent);
            return count;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<sale_order>();
        }    
    }
}
