namespace DMSA.Sync.Core.Database.Sqlite
{
    public class AppParameterDb : SqliteDbBase<DMSA.Models.Odoo.Native.stock_quant>
    {
        public AppParameterDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
    }
}
