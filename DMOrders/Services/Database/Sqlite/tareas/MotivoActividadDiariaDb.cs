using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class MotivoActividadDiariaDb : SqliteDbBase<MotivoActividadDiaria>
    {   
        public MotivoActividadDiariaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<MotivoActividadDiaria>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<MotivoActividadDiaria>().Where(x=>x.tipo== "ACT" && x.codsistema== "VEX").ToListAsync();
        }

        public async Task<MotivoActividadDiaria> GetItem(int id)
        {
            await Init();
            return await Database.Table<MotivoActividadDiaria>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
