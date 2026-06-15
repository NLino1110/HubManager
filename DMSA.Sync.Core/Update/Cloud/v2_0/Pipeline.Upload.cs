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
using static ApiManager.HubMnsaAttachment;

namespace DMSA.Sync.Core.Update.Cloud.v2_0
{
    public partial class Pipeline
    {        
        public async Task<(PackageResponseDto, bool)> UploadSqliteZip(string DbNameSqlite, Func<int, int, Task>? onProgress = null)
        {
            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                DbNameSqlite
            );

            Debug.WriteLine($"Comprimiendo base de datos: {dbPath}");
            string zipPath = await CompressDatabaseAsync(dbPath);

            byte[] zipBytes = await File.ReadAllBytesAsync(zipPath);

            Debug.WriteLine($"ZIP generado. Tamaño total: {zipBytes.Length / 1024 / 1024.0:F2} MB");

            var hub = new HubPackageClient(Constants.Session);

            string package_name = $"pk_{Constants.Session.AppCodeOdoo}_{DbNameSqlite}_{DateTime.Now:yyyyMMddHHmmss}";
            //package_name = "pk_01_prod1_macronegocios_20260614012613";

            var package = new CreatePackageDto()
            {
                Name = package_name,
                server = Constants.Session.odooConnection.Host,
                database_name = Constants.Session.odooConnection.DbName,
                file_name = Path.GetFileName(zipPath),
                file_type = "application/zip",
                date_data_cutoff = DateTime.UtcNow,
                mobile_app_id = Constants.Session.AppCodeOdoo,
                //total_files_expected = 0,
                //total_file_size_expected = 0,
                external_guid = Guid.NewGuid().ToString(),
                //user_frontend = "456789"
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

            // Partir el ZIP
            var parts = SplitFile(zipBytes, MAX_PART_SIZE).ToList();
            int totalParts = parts.Count;

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

        //public async Task<(string, bool, responseUpload)> UploadSqliteZipNonAttach(string dbNameSqlite)
        //{
        //    string dbPath = Path.Combine(
        //        FileSystem.AppDataDirectory,
        //        dbNameSqlite
        //    );

        //    string zipPath = string.Empty;
        //    string final_url = string.Empty;
        //    responseUpload file_upload_response = null;

        //    try
        //    {
        //        Debug.WriteLine($"Comprimiendo base de datos: {dbPath}");
        //        zipPath = await CompressDatabaseAsync(dbPath);

        //        byte[] zipBytes = await File.ReadAllBytesAsync(zipPath);

        //        Debug.WriteLine($"ZIP generado. Tamaño total: {zipBytes.Length / 1024 / 1024.0:F2} MB");

        //        HubMnsaAttachment hub = new HubMnsaAttachment(Constants.Session);

        //        int packageId = 1381;

        //        //var parts = SplitFile(zipBytes, MAX_PART_SIZE_LONG).ToList();
        //        //int totalParts = parts.Count;
        //        //Una sola parte para poder descargar
        //        var parts = new List<byte[]> { zipBytes };
        //        int totalParts = 1;

        //        string package_name = Constants.Session.AppCodeOdoo + "_app_package_" +
        //        dbNameSqlite + "_" +
        //        DateTime.Now.ToString("yyyyMMddHHmmss");

        //        for (int i = 0; i < totalParts; i++)
        //        {
        //            string partName = $"{packageId}_{dbNameSqlite}_part_{(i + 1):D6}.zip";

        //            file_upload_response = await hub.SendToExternalServer(parts[i], partName, package_name);
        //            if (file_upload_response == null || file_upload_response.url == String.Empty)
        //                return (final_url, false, file_upload_response);

        //            //if(file_upload_response.status_code != 200)
        //            //{
        //            //    Debug.WriteLine($"Upload error: {file_upload_response.message}");
        //            //    return (final_url, false);
        //            //}

        //            final_url = file_upload_response.url;
        //        }

        //        return (final_url, true, file_upload_response);
        //    }
        //    finally
        //    {

        //        try
        //        {
        //            if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
        //            {
        //                File.Delete(zipPath);
        //                Debug.WriteLine($"ZIP eliminado: {zipPath}");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine($"No se pudo eliminar el ZIP: {ex.Message}");
        //        }
        //    }
        //}

        //public async Task<(mnsa_attachment, bool)> UploadSqliteZipCustomMode2(string dbNameSqlite)
        //{
        //    string dbPath = Path.Combine(
        //        FileSystem.AppDataDirectory,
        //        dbNameSqlite
        //    );

        //    string zipPath = string.Empty;

        //    try
        //    {
        //        Debug.WriteLine($"Comprimiendo base de datos: {dbPath}");
        //        zipPath = await CompressDatabaseAsync(dbPath);

        //        byte[] zipBytes = await File.ReadAllBytesAsync(zipPath);

        //        Debug.WriteLine($"ZIP generado. Tamaño total: {zipBytes.Length / 1024 / 1024.0:F2} MB");

        //        HubMnsaAttachment hub = new HubMnsaAttachment(Constants.Session);

        //        mnsa_attachment mnsaAttachment = new mnsa_attachment()
        //        {
        //            server = Constants.Session.odooConnection.Host,
        //            database_name = Constants.Session.odooConnection.DbName,
        //            file_name = Path.GetFileName(zipPath),
        //            file_type = "application/zip",
        //            date_data_cutoff = DateTime.UtcNow,
        //            mobile_app_id = Constants.Session.AppMobileId,
        //        };

        //        var responseSend = await hub.CreatePackage(mnsaAttachment);
        //        int packageId = responseSend.result;

        //        var parts = SplitFile(zipBytes, MAX_PART_SIZE_LONG).ToList();
        //        int totalParts = parts.Count;

        //        string package_name = Constants.Session.AppCodeOdoo + "_app_package_" +
        //        dbNameSqlite + "_" +
        //        DateTime.Now.ToString("yyyyMMddHHmmss");

        //        for (int i = 0; i < totalParts; i++)
        //        {
        //            string partName = $"{packageId}_{dbNameSqlite}_part_{(i + 1):D6}.zip";

        //            var file_upload_response = await hub.SendAttachmentMode2(new mnsa_attachment_line()
        //            {
        //                name = partName,
        //                url = "",
        //                file_name = partName,
        //                file_bytes = parts[i],
        //                file_type = "application/zip",
        //                package_id = packageId
        //            }, dbNameSqlite, package_name);

        //            if (file_upload_response.result == 0)
        //                return (mnsaAttachment, false);

        //            await hub.LinkMode2(packageId, file_upload_response.result);
        //        }

        //        return (mnsaAttachment, true);
        //    }
        //    finally
        //    {

        //        try
        //        {
        //            if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
        //            {
        //                File.Delete(zipPath);
        //                Debug.WriteLine($"ZIP eliminado: {zipPath}");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine($"No se pudo eliminar el ZIP: {ex.Message}");
        //        }
        //    }
        //}
    }
}
