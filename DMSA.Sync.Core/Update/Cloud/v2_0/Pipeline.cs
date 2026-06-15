using ApiManager;
using ApiManagerOdoo.Specials;
using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Abstract.Server.Dto;
using DMSA.Models.Odoo.Specials;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Database.Sqlite.Special;
using System.Diagnostics;
using System.IO.Compression;

namespace DMSA.Sync.Core.Update.Cloud.v2_0
{
    public partial class Pipeline
    {
        public async Task<string> CompressDatabaseAsync(string dbPath)
        {
            if (!File.Exists(dbPath))
                throw new FileNotFoundException("No se encontró la base de datos", dbPath);

            string tempCopyPath = dbPath + ".tmp";
            string zipPath = dbPath + ".zip";

            // Eliminar restos previos
            if (File.Exists(tempCopyPath))
                File.Delete(tempCopyPath);

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // 1️⃣ Copiar archivo SIN bloquear el original
            using (var source = new FileStream(
                dbPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            using (var dest = new FileStream(
                tempCopyPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                await source.CopyToAsync(dest);
            }

            // 2️⃣ Comprimir la copia
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(
                    tempCopyPath,
                    Path.GetFileName(dbPath),
                    CompressionLevel.SmallestSize
                );
            }

            // 3️⃣ Limpiar temporal
            File.Delete(tempCopyPath);

            return zipPath;
        }

        //Archivos con un tamaño maximo de 3.14 MB
        const int MAX_PART_SIZE = (int)(3.14 * 1024 * 1024);
        const int MAX_PART_SIZE_LONG = (int)(6.6 * 1024 * 1024);


        public static IEnumerable<byte[]> SplitFile(byte[] fileBytes, int chunkSize)
        {
            int offset = 0;

            while (offset < fileBytes.Length)
            {
                int size = Math.Min(chunkSize, fileBytes.Length - offset);
                byte[] chunk = new byte[size];
                Buffer.BlockCopy(fileBytes, offset, chunk, 0, size);
                offset += size;
                yield return chunk;
            }
        }

        public async Task<Package> NewestZipPack()
        {
            string packName = Constants.Session.odooConnection.DbNameSqlite + ".zip";

            return await NewestZipPack(packName);
        }

        public async Task<Package> NewestZipPack(string packName)
        {
            var packageClient = new HubPackageClient(Constants.Session);
                        
            var top5List = await packageClient.GetPackages(packName);

            if (top5List != null && top5List.Count > 0)
            {
                return top5List[0];
            }

            return null;
        }

        public bool ZipContainsFile(string zipPath, string fileName)
        {
            using var zip = ZipFile.OpenRead(zipPath);

            return zip.Entries.Any(e =>
                string.Equals(e.Name, fileName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> ExistAttachRecord()
        {
            var mnsaAttachmentDb = new PackageDb(Constants.Session.odooConnection.DbNameSqlite);
            var record = await mnsaAttachmentDb.GetLastUpdate();
            if(record != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> RequiredNewUpload(string dbNameSqlite)
        {
            if (!dbNameSqlite.Contains(".zip"))
            {
                dbNameSqlite = dbNameSqlite + ".zip";
            }

            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

            var topList = await hubMnsaAttachment.GetLastestByFileName(Constants.Session.CurrentUserFront.log_fec_acceso, dbNameSqlite);

            if (topList?.result == null || topList.result.Length == 0)
                return true;

            var item = topList.result[0];

            if (!item.date_data_cutoff.HasValue)
                return true;

            DateTime cutoff = item.date_data_cutoff.Value.Date;
            DateTime now = Constants.Session.CurrentUserFront.log_fec_acceso.Date;

            double daysDiff = Math.Abs((now - cutoff).TotalDays);

            return daysDiff >= 1;
        }

        public async Task<bool> RequiredNewUploadBasePack(string dbNameSqlite)
        {
            if (!dbNameSqlite.Contains(".zip"))
            {
                dbNameSqlite = dbNameSqlite + ".zip";
            }

            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

            var topList = await hubMnsaAttachment.GetLastestByFileName(Constants.Session.CurrentUserFront.log_fec_acceso, dbNameSqlite);

            if (topList?.result == null || topList.result.Length == 0)
                return true;

            //var item = topList.result[0];

            //if (!item.date_data_cutoff.HasValue)
            //    return true;

            return false;
        }

        public async Task<bool> ExistAttachRecordCustom(string dbNameSqlite, string dbPath)
        {
            var packageDb = new PackageDb(dbNameSqlite);
            var record = await packageDb.GetLastUpdate(dbPath);
            return record != null;
        }

        public async Task<bool> InsertAttachRecordCustom(string dbNameSqlite, Package package)
        {
            var packageDb = new PackageDb(dbNameSqlite);
            await packageDb.InsertAsync(package);
            return true;
        }

        //public async Task<bool> InsertAttachRecord(mnsa_attachment mnsa_Attachment)
        //{
        //    MnsaAttachmentDb mnsaAttachmentDb = new MnsaAttachmentDb(Constants.Session.odooConnection.DbNameSqlite);
        //    var record = await mnsaAttachmentDb.InsertAsync(mnsa_Attachment);            
        //    return true;
        //}
    }
}
