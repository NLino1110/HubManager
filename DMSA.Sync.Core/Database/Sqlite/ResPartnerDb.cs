using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ResPartnerDb : SqliteDbBase<res_partner>
    {
        private Dictionary<int, string> cachedChannels = new Dictionary<int, string>();

        public ResPartnerDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_res_partner__id ON res_partner(id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_res_partner__name ON res_partner(name)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_res_partner__type ON res_partner(_type)");
            });
        }

        public async Task PreloadInfoData()
        {
            if (cachedChannels.Count > 0)
                return;

            var itemsChannels = await Database.Table<product_pricelist>()
                .Where(x => x.active == true)
                .ToArrayAsync();

            cachedChannels = itemsChannels.ToDictionary(channel => channel.id, channel => channel.name);
        }

        private static AsyncTableQuery<res_partner> ApplySort(
            AsyncTableQuery<res_partner> q, int filter_sort)
        {
            // 1: sequence ASC, 2: code ASC, 3: name ASC; default: id ASC
            return filter_sort switch
            {
                1 => q.OrderBy(x => x.name),
                //2 => q.OrderBy(x => x.code),
                //3 => q.OrderBy(x => x.name),
                _ => q.OrderBy(x => x.id)
            };
        }

        private async Task<AsyncTableQuery<res_partner>> BuildQuery(
            string filter_code,
            string filter_vat,
            string filter_name,
            int filter_channel,
            int filter_days,
            int filter_status,
            int filter_sort,
            int filter_adic_commercial)
        {
            Init();

            var q = Database.Table<res_partner>();

            await PreloadInfoData();

            var idStr = filter_adic_commercial.ToString();

            var exact_str = $"[{idStr}]";
            var middle_str = $",{idStr},";
            var start_str = $"[{idStr},";
            var end_str = $",{idStr}]";

            //Excluimos los vendedores y filtramos cliente por vendedor
            q = q.Where(x =>
                    x.is_salesman == false &&
                    (
                        x._adic_comercial_id == filter_adic_commercial ||
                        (
                            x.adic_comercial_secundarios_ids_json != null &&
                            (                            
                            x.adic_comercial_secundarios_ids_json == exact_str ||
                            x.adic_comercial_secundarios_ids_json.Contains(middle_str) ||
                            x.adic_comercial_secundarios_ids_json.Contains(start_str) ||
                            x.adic_comercial_secundarios_ids_json.Contains(end_str)
                            )
                        )
                    )
                );

            //q = q.Where(x => x.is_salesman == false && x._adic_comercial_id == filter_adic_commercial);

            // --- 1) Filtro por code (prioridad máxima, como tu método actual) ---
            if (!string.IsNullOrWhiteSpace(filter_code))
            {
                var raw = filter_code.Trim();

                // Si es numérico: buscar por id exacto (fallback lo haces fuera)
                if (int.TryParse(raw, out var idCode))
                {
                    q = q.Where(x => x.id == idCode);
                    // OJO: no aplicamos más filtros aquí para mantener tu comportamiento original.
                    return ApplySort(q, filter_sort);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter_vat))
            {
                var raw = filter_vat.Trim();
                q = q.Where(x => x.vat_doc != null && x.vat_doc.Contains(raw));
                return ApplySort(q, filter_sort);                
            }

            // --- 2) Resto de filtros cuando NO hay filter_code ---
            if (!string.IsNullOrWhiteSpace(filter_name))
            {
                var nameTerm = filter_name.Trim().ToLowerInvariant();
                q = q.Where(x => x.name.ToLower().Contains(nameTerm));
            }

            if(filter_channel != 0)
            {
                q = q.Where(x => x._product_pricelist_id == filter_channel);
            }

            if (filter_status == 1)
                q = q.Where(x => x.misc_estado == "activo");
            else if(filter_status == 2)
                q = q.Where(x => x.misc_estado == "inactivo");

            //if (filter_new == 1)
            //    q = q.Where(x => x.is_new);

            //if (filter_stock == 1)
            //    q = q.Where(x => x.qty_available > 0);

            if(filter_days == 1)
                q = q.Where(x => x.adic_lunes);

            if (filter_days == 2)
                q = q.Where(x => x.adic_martes);

            if (filter_days == 3)
                q = q.Where(x => x.adic_miercoles);

            if (filter_days == 4)
                q = q.Where(x => x.adic_jueves);

            if (filter_days == 5)
                q = q.Where(x => x.adic_viernes);

            if (filter_days == 6)
                q = q.Where(x => x.adic_sabado);

            if (filter_days == 7)
                q = q.Where(x => x.adic_domingo);

            // --- 3) Orden ---
            q = ApplySort(q, filter_sort);

            return q;
        }

        private async Task<(string Sql, object[] Args)> BuildQuerySQL(
            string filter_code,
            string filter_vat,
            string filter_name,
            int filter_channel,
            int filter_days,
            int filter_status,
            int filter_sort,
            int filter_adic_commercial)
        {
            Init();

            await PreloadInfoData();

            var where = new List<string>();
            var args = new List<object>();

            var idStr = filter_adic_commercial.ToString();

            var exact_str = $"[{idStr}]";
            var middle_str = $",{idStr},";
            var start_str = $"[{idStr},";
            var end_str = $",{idStr}]";

            where.Add("(is_salesman = 0 AND (_adic_comercial_id = ? OR (adic_comercial_secundarios_ids_json IS NOT NULL AND (adic_comercial_secundarios_ids_json = ? OR adic_comercial_secundarios_ids_json LIKE ? OR adic_comercial_secundarios_ids_json LIKE ? OR adic_comercial_secundarios_ids_json LIKE ?))))");

            args.Add(filter_adic_commercial);
            args.Add(exact_str);
            args.Add($"%{middle_str}%");
            args.Add($"{start_str}%");
            args.Add($"%{end_str}");

            if (!string.IsNullOrWhiteSpace(filter_code))
            {
                var raw = filter_code.Trim();

                if (int.TryParse(raw, out var idCode))
                {
                    where.Add("id = ?");
                    args.Add(idCode);
                }
            }
            else if (!string.IsNullOrWhiteSpace(filter_vat))
            {
                where.Add("vat_doc IS NOT NULL AND vat_doc LIKE ? COLLATE NOCASE");
                args.Add($"%{filter_vat.Trim()}%");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(filter_name))
                {
                    var term = filter_name.Trim();

                    var term1 = term.Replace("ñ", "Ñ");
                    var term2 = term.Replace("Ñ", "ñ");

                    where.Add("(name LIKE ? COLLATE NOCASE OR name LIKE ? COLLATE NOCASE)");
                    args.Add($"%{term1}%");
                    args.Add($"%{term2}%");
                }

                if (filter_channel != 0)
                {
                    where.Add("_product_pricelist_id = ?");
                    args.Add(filter_channel);
                }

                if (filter_status == 1)
                    where.Add("misc_estado = 'activo'");
                else if (filter_status == 2)
                    where.Add("misc_estado = 'inactivo'");

                if (filter_days == 1) where.Add("adic_lunes = 1");
                if (filter_days == 2) where.Add("adic_martes = 1");
                if (filter_days == 3) where.Add("adic_miercoles = 1");
                if (filter_days == 4) where.Add("adic_jueves = 1");
                if (filter_days == 5) where.Add("adic_viernes = 1");
                if (filter_days == 6) where.Add("adic_sabado = 1");
                if (filter_days == 7) where.Add("adic_domingo = 1");
            }

            var sql = $"SELECT * FROM res_partner WHERE {string.Join(" AND ", where)}";
            sql = ApplySortSql(sql, filter_sort);

            return (sql, args.ToArray());
        }

        private string ApplySortSql(string sql, int filter_sort)
        {
            switch (filter_sort)
            {
                case 1: return sql + " ORDER BY name ASC";
                case 2: return sql + " ORDER BY name DESC";
                case 3: return sql + " ORDER BY id ASC";
                case 4: return sql + " ORDER BY id DESC";
                default: return sql;
            }
        }

        public async Task<(IList<res_partner> Items, int Total)> GetPagedAsync(
            string filter_code,
            string filter_vat,
            string filter_name,
            int filter_channel,
            int filter_days,
            int filter_status,
            int filter_sort,
            int filter_adic_commercial,
            int page, int pageSize)
        {            
            var q = await BuildQuery(filter_code, filter_vat, filter_name, filter_channel,filter_days, filter_status, filter_sort, filter_adic_commercial);

            // COUNT(*) en SQLite, sin traer datos
            var total = await q.CountAsync();

            // LIMIT/OFFSET en SQLite (Skip/Take sobre AsyncTableQuery)
            var offset = Math.Max(0, (page - 1) * pageSize);
            var items = await q.Skip(offset).Take(pageSize).ToListAsync();

            foreach (var p in items)
            {
                p.display_channel_name = cachedChannels.TryGetValue(p._product_pricelist_id, out var channelName) ? channelName : "";
            }

            return (items, total);
        }

        public async Task<(IList<res_partner> Items, int Total)> GetPagedSqlAsync(
            string filter_code,
            string filter_vat,
            string filter_name,
            int filter_channel,
            int filter_days,
            int filter_status,
            int filter_sort,
            int filter_adic_commercial,
            int page, int pageSize)
        {
            var (baseSql, args) = await BuildQuerySQL(
                filter_code,
                filter_vat,
                filter_name,
                filter_channel,
                filter_days,
                filter_status,
                filter_sort,
                filter_adic_commercial);

            var countSql = $"SELECT COUNT(*) FROM ({baseSql}) AS t";
            var total = await Database.ExecuteScalarAsync<int>(countSql, args);

            var orderedSql = ApplySortSql(baseSql, filter_sort);

            var offset = Math.Max(0, (page - 1) * pageSize);
            var pagedSql = $"{orderedSql} LIMIT {pageSize} OFFSET {offset}";

            var items = await Database.QueryAsync<res_partner>(pagedSql, args);

            foreach (var p in items)
            {
                p.display_channel_name = cachedChannels.TryGetValue(p._product_pricelist_id, out var channelName) ? channelName : "";
            }

            return (items, total);
        }

        public async Task<List<res_partner>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_partner>().Take(50).ToListAsync();            
        }

        public async Task<res_partner> GetItemsAsync(int company_id, int partner_id)
        {
            await Init();
            return await Database.Table<res_partner>().Where(x=> (x._company_id == company_id || x._company_id == 0) && 
            x.id == partner_id).FirstOrDefaultAsync();
        }

        public async Task<List<res_partner>> GetItemsBySearchAsync(int company_id, string TextSearch, int limit)
        {
            await Init();
            int findCode = 0;

            int.TryParse(TextSearch, out findCode);

            if (findCode > 0)
            {
                return await Database.Table<res_partner>().Where(y =>
                (y._company_id == company_id || y._company_id == 0) && (
                y.id == findCode )
                ).Take(limit).ToListAsync();
            }
            else
            {
                if(TextSearch.Length < 3)
                {
                    //Sin texto para buscar
                    //await Database.Table<res_partner>().Where(x => x.id == -1).ToListAsync();
                    await Database.Table<res_partner>().Take(0).ToListAsync();
                }

                return await Database.Table<res_partner>().Where(y =>
                y._parent_id == 0 &&
                (y._company_id == company_id || y._company_id == 0) && (
                y.id == findCode ||
                y.name.Contains(TextSearch) ||
                y.email.Contains(TextSearch) ||
                y.vat.Contains(TextSearch))
                ).Take(limit).ToListAsync();
            }
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<res_partner>> GetItemsBySearchAsyncWithChannel(int company_id, string TextSearch, int limit)
        {
            await Init();

            await PreloadInfoData();

            int findCode = 0;

            int.TryParse(TextSearch, out findCode);

            var q = Database.Table<res_partner>();

            if (findCode > 0)
            {
                q = Database.Table<res_partner>().Where(y =>
                (y._company_id == company_id || y._company_id == 0) && (
                y.id == findCode)
                );
            }
            else
            {
                if (TextSearch.Length < 3)
                {                    
                    await Database.Table<res_partner>().Take(0).ToListAsync();
                }

                q = Database.Table<res_partner>().Where(y =>
                (y._company_id == company_id || y._company_id == 0) && (
                y.id == findCode ||
                y.name.Contains(TextSearch) ||
                y.email.Contains(TextSearch) ||
                y.vat.Contains(TextSearch))
                );
            }            

            var items = await q.Take(limit).ToListAsync();

            foreach (var p in items)
            {
                p.display_channel_name = cachedChannels.TryGetValue(p._product_pricelist_id, out var channelName) ? channelName : "";
            }

            return items;
        }

        //public async Task<List<res_partner>> GetItemsByPartnerForPaymentAsync(res_partner res_Partner)
        //{
        //    await Init();
        //    return await Database.Table<account_move>().Where(x=>
        //    x._partner_id == res_Partner.id &&
        //    x.move_type == "out_invoice" && 
        //    x.amount_residual > 0).ToListAsync();
        //    //return Database.Table<account_journal>().ToList();
        //}
                
        public async Task ClearFullCache()
        {
            cachedChannels.Clear();
        }

        public async Task<res_partner> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_partner>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task RemoveOldDataAsync()
        {
            await Init();

            string sql = @"
               DELETE FROM res_partner WHERE datetime((write_date / 10000000) - 62135596800, 'unixepoch') <= '2026-04-21';
            ";

            await Database.ExecuteAsync(sql);
        }
    }
}
