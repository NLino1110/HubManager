using ApiManagerOdoo.Specials;
using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Abstract.Server.Dto;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update.Cloud.v2_0
{
    public partial class Pipeline
    {        
        public async Task<(PackageResponseDto, bool)> UploadSqliteZip(DatabaseStruct ds, Func<int, int, Task>? onProgress = null)
        {
            var DbNameSqlite = ds.Name;
            var DbName = ds.OriginalDBName;

            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                DbNameSqlite
            );

            Debug.WriteLine($"Comprimiendo base de datos: {dbPath}");
            string zipPath = await CompressDatabaseAsync(dbPath);

            byte[] zipBytes = await File.ReadAllBytesAsync(zipPath);

            Debug.WriteLine($"ZIP generado. Tamaño total: {zipBytes.Length / 1024 / 1024.0:F2} MB");

            // Partir el ZIP
            var parts = SplitFile(zipBytes, MAX_PART_SIZE).ToList();
            int totalParts = parts.Count;

            var hub = new HubPackageClient(Constants.Session);

            string package_name = $"pk_{Constants.Session.AppCodeOdoo}_{DbNameSqlite}_{DateTime.Now:yyyyMMddHHmmss}";
            //package_name = "pk_01_prod1_macronegocios_20260614012613";

            var package = new CreatePackageDto()
            {
                Name = package_name,
                server = ds.Host,
                database_name = DbName,
                file_name = Path.GetFileName(zipPath),
                file_type = "application/zip",
                date_data_cutoff = DateTime.UtcNow,
                mobile_app_id = Constants.Session.AppCodeOdoo,
                //user_frontend = Constants.Session.CurrentUserFront.username,
                total_files_expected = totalParts,
                //total_file_size_expected = 0,
                external_guid = Guid.NewGuid().ToString(),                
            };

            // Crear paquete lógico (mnsa.package)
            var responseSend = await hub.CreatePackage(package);

            //if(string.IsNullOrEmpty( responseSend.error))
            //{
            //    responseSend.success_upload = true;
            //}

            if (!responseSend.success_upload)
            {                
                return (responseSend, false);
            }
            //bool packageId = responseSend;

            Debug.WriteLine($"Paquete creado con ID: {responseSend.id}");

            

            Debug.WriteLine($"Archivo dividido en {totalParts} partes");

            // Subir cada parte como ir.attachment
            for (int i = 0; i < totalParts; i++)
            {
                string partName = $"{DbNameSqlite}.part{i + 1:D2}.zip";

                Debug.WriteLine($"Subiendo {partName} ({i + 1}/{totalParts})");
                int part_size = parts[i].Length;

                var file_upload_response = await hub.UploadFileBytes(package_name,
                    parts[i],                
                    partName,                    
                    "application/zip",
                    part_size
                );                

                if (!file_upload_response)
                {
                    Debug.WriteLine($"Hubo un error al enviar el archivo: {file_upload_response}");
                    responseSend.error = $"Hubo un error al enviar el archivo: {file_upload_response}";
                    responseSend.success_upload = false;
                    return (responseSend, false);
                }

                Debug.WriteLine($"Parte {partName} subida");


                if (onProgress != null)
                    await onProgress(i + 1, totalParts);
            }

            Debug.WriteLine("✅ Todas las partes enviadas correctamente");

            try
            {
                if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                    Debug.WriteLine($"ZIP eliminado: {zipPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"No se pudo eliminar el ZIP: {ex.Message}");
            }

            return (responseSend, true);
        }
    }
}
