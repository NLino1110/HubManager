using CommunityToolkit.Maui.Alerts;
using DMSA.Sync.Core.Database.Sqlite;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Sync.Core.Update.Cloud
{
    public partial class Pipeline    
    {
        public async Task<bool> DownloadFromFileMode2(string dbPath, FileInfo existTmp, string dbPathCentral)
        {
            if(existTmp != null)
            {
                string tempZipPath = existTmp.FullName;

                bool exists = ZipContainsFile(tempZipPath, dbPath);

                if (exists)
                {
                    await SqliteConnectionManager.CloseAllAsync();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    string extractPath = FileSystem.AppDataDirectory;
                    ZipFile.ExtractToDirectory(tempZipPath, extractPath, true);

                    await Task.Delay(1000);
                }
                
                File.Delete(tempZipPath);

                return true;
            }

            if(!dbPath.Contains(".zip"))
            {
                dbPath = dbPath + ".zip";
            }

            Pipeline pipeline = new Pipeline();

            bool packageReady = await pipeline.ExistAttachRecordCustom(dbPathCentral, dbPath);

            if (!packageReady)
            {
                var packFound = await pipeline.NewestZipPack(dbPath);

                if (packFound != null)
                {
                    await SqliteDbBase<object>.CloseDatabaseAsync();

                    if (await pipeline.DownloadSqliteZipCustomMode2(dbPath,true))
                    {
                        await pipeline.InsertAttachRecordCustom(dbPathCentral, packFound);
                    }
                    else
                    {
                        await Toast.Make("Hubo un error al descargar/descomprimir archivo.").Show();
                    }

                    await Toast.Make("Actualización rápida terminada").Show();
                }
            }

            return true;
        }

        public async Task<bool> UploadToFileMode2(string dbPath, string centralDB)
        {
            Pipeline pipeline = new Pipeline();
            bool requiredNewUpload = await pipeline.RequiredNewUploadBasePack(dbPath);
            if (requiredNewUpload)
            {
                (var attachData, bool successUpload) = await pipeline.UploadSqliteZipCustomMode2(dbPath);

                if (successUpload)
                {
                    if (!await pipeline.ExistAttachRecordCustom(centralDB, dbPath))
                        await pipeline.InsertAttachRecordCustom(centralDB, attachData);
                }
            }

            return true;
        }

        public async Task<bool> UploadToFileNoAttach(string dbPath, string centralDB)
        {            
            (var attachData, bool successUpload) = await UploadSqliteZipNonAttach(dbPath);

            if (successUpload)
            {
                    
            }            

            return true;
        }
    }
}
