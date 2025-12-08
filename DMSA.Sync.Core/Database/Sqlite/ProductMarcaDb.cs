using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductMarcaDb : SqliteDbBase<product_marca>
    {
        public ProductMarcaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<product_marca>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<product_marca>().ToListAsync();
        }

        public async Task<List<product_marca>> GetItemsAsync(int[] topMarcas)
        {
            await Init();

            if (topMarcas == null || topMarcas.Length == 0)
                return await Database.Table<product_marca>().ToListAsync();

            var result = new List<product_marca>();

            foreach (var id in topMarcas)
            {
                var clave = id.ToString();

                // Busca solo una marca con esa clave_externa (o null si no existe)
                //var marca = await Database.Table<product_marca>()
                //    .Where(x => x.clave_externa == clave)
                //    .FirstOrDefaultAsync();

                var marca = await Database.Table<product_marca>()
                    .Where(x => x.id == id)
                    .FirstOrDefaultAsync();

                if (marca != null)
                    result.Add(marca);
            }

            return result;
        }

        public async Task<product_marca> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_marca>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
