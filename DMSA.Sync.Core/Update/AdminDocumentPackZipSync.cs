using ApiManager;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;

namespace DMSA.Sync.Core.Update
{
    /// <summary>
    /// Primer sync del día (admin Mobile App 218):
    /// busca el ZIP del cron, lo descarga, descomprime e inserta en SQLite.
    /// No llama a action_request_document_pack (generación síncrona / timeout).
    /// </summary>
    public static class AdminDocumentPackZipSync
    {
        public const int SqliteBatchSize = 2000;

        public sealed class ImportResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public int PackId { get; set; }
            public int HeaderCount { get; set; }
            public int LineCount { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
        }

        public static async Task<ImportResult> TryImportAsync(
            AppSession appSession,
            Func<int, int, Task>? onProgress = null)
        {
            var result = new ImportResult();
            string? workDir = null;

            try
            {
                if (appSession?.odooConnection == null)
                {
                    result.Message = "Sin conexión Odoo.";
                    return result;
                }

                await Report(onProgress, 1, 5);

                var hub = new HubMnsaMobileDocumentPack(appSession);
                var pack = await hub.GetLatestDonePackAsync();
                if (pack == null || pack._attachment_id <= 0)
                {
                    result.Message = "No hay paquete ZIP done. Fallback HTTP.";
                    Debug.WriteLine("[AdminDocumentPackZip] " + result.Message);
                    return result;
                }

                result.PackId = pack.id;
                result.DateFrom = pack.date_from;
                result.DateTo = pack.date_to;

                workDir = Path.Combine(
                    FileSystem.CacheDirectory,
                    "mnsa_doc_pack_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(workDir);

                string zipPath = Path.Combine(workDir, "pack.zip");
                string extractDir = Path.Combine(workDir, "extract");
                Directory.CreateDirectory(extractDir);

                await Report(onProgress, 2, 5);
                Debug.WriteLine(
                    $"[AdminDocumentPackZip] descargando pack={pack.id} att={pack._attachment_id} {pack.date_from:yyyy-MM-dd}..{pack.date_to:yyyy-MM-dd}");

                await hub.DownloadAttachmentToFileAsync(pack._attachment_id, zipPath);
                if (!File.Exists(zipPath) || new FileInfo(zipPath).Length == 0)
                {
                    result.Message = "ZIP vacío o no descargado.";
                    return result;
                }

                await Report(onProgress, 3, 5);
                ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);

                string headerPath = Path.Combine(extractDir, "account_move.json");
                string linePath = Path.Combine(extractDir, "account_move_line.json");
                if (!File.Exists(headerPath) || !File.Exists(linePath))
                {
                    result.Message = "El ZIP no trae account_move.json y account_move_line.json.";
                    return result;
                }

                if (!TryValidateJsonFields(headerPath, AccountDocumentSyncFields.Header, "account_move", out string headerFieldError))
                {
                    result.Message = headerFieldError;
                    return result;
                }

                if (!TryValidateJsonFields(linePath, AccountDocumentSyncFields.Line, "account_move_line", out string lineFieldError))
                {
                    result.Message = lineFieldError;
                    return result;
                }

                var headerDb = new AccountMoveDb(appSession.odooConnection.DbNameSqlite);
                var lineDb = new AccountMoveLineDb(appSession.odooConnection.DbNameSqlite);

                await Report(onProgress, 4, 5);
                bool headerMapped = false;
                result.HeaderCount = await InsertJsonArrayInBatchesAsync<account_move>(
                    headerPath,
                    batch =>
                    {
                        if (!headerMapped)
                        {
                            if (!TryValidateMappedHeader(batch[0], out string mapError))
                                throw new InvalidOperationException(mapError);
                            headerMapped = true;
                        }

                        return headerDb.InsertBatchAsync(batch);
                    },
                    onProgress);

                await Report(onProgress, 5, 5);
                bool lineMapped = false;
                result.LineCount = await InsertJsonArrayInBatchesAsync<account_move_line>(
                    linePath,
                    batch =>
                    {
                        if (!lineMapped)
                        {
                            if (!TryValidateMappedLine(batch[0], out string mapError))
                                throw new InvalidOperationException(mapError);
                            lineMapped = true;
                        }

                        return lineDb.InsertBatchAsync(batch);
                    },
                    onProgress);

                result.Success = result.HeaderCount > 0;
                result.Message = result.Success
                    ? $"ZIP pack {pack.id}: {result.HeaderCount} cabeceras, {result.LineCount} líneas"
                    : "ZIP sin cabeceras.";

                Debug.WriteLine("[AdminDocumentPackZip] " + result.Message);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[AdminDocumentPackZip] " + ex);
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            finally
            {
                TryDeleteDir(workDir);
            }
        }

        static async Task<int> InsertJsonArrayInBatchesAsync<T>(
            string jsonPath,
            Func<T[], Task> insertBatch,
            Func<int, int, Task>? onProgress)
        {
            using var reader = new StreamReader(jsonPath);
            using var jsonReader = new JsonTextReader(reader);
            var serializer = new JsonSerializer();

            if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.StartArray)
                throw new InvalidOperationException("JSON esperado: arreglo [" + typeof(T).Name + "]");

            var batch = new List<T>(SqliteBatchSize);
            int total = 0;

            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.EndArray)
                    break;

                if (jsonReader.TokenType != JsonToken.StartObject)
                    continue;

                var item = serializer.Deserialize<T>(jsonReader);
                if (item == null)
                    continue;

                batch.Add(item);
                if (batch.Count < SqliteBatchSize)
                    continue;

                var chunk = batch.ToArray();
                batch.Clear();
                await insertBatch(chunk);
                total += chunk.Length;
                await Report(onProgress, total, Math.Max(total, 1));
            }

            if (batch.Count > 0)
            {
                var chunk = batch.ToArray();
                await insertBatch(chunk);
                total += chunk.Length;
                await Report(onProgress, total, Math.Max(total, 1));
            }

            return total;
        }

        static bool TryValidateJsonFields(
            string jsonPath,
            IReadOnlyList<string> expected,
            string label,
            out string error)
        {
            error = string.Empty;
            using var reader = new StreamReader(jsonPath);
            using var jsonReader = new JsonTextReader(reader);
            if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.StartArray)
            {
                error = $"ZIP {label}: no es un arreglo JSON.";
                return false;
            }

            if (!jsonReader.Read() || jsonReader.TokenType == JsonToken.EndArray)
            {
                error = $"ZIP {label}: arreglo vacío.";
                return false;
            }

            var first = JObject.Load(jsonReader);
            var missing = expected.Where(f => !first.ContainsKey(f)).ToArray();
            if (missing.Length == 0)
                return true;

            error = $"ZIP {label} no trae los campos del sync HTTP: {string.Join(", ", missing)}. " +
                    "Actualice mnsa_mobile y regenere el paquete.";
            Debug.WriteLine("[AdminDocumentPackZip] " + error);
            return false;
        }

        static bool TryValidateMappedHeader(account_move row, out string error)
        {
            error = string.Empty;
            if (row == null || row.id <= 0)
            {
                error = "ZIP cabecera: id no se mapeó.";
                return false;
            }

            if (row.invoice_date == default)
            {
                error = $"ZIP cabecera id={row.id}: invoice_date llegó vacío.";
                return false;
            }

            if (row.partner_id == null || row.partner_id.Type == JTokenType.Null)
            {
                error = $"ZIP cabecera id={row.id}: partner_id no se mapeó.";
                return false;
            }

            return true;
        }

        static bool TryValidateMappedLine(account_move_line row, out string error)
        {
            error = string.Empty;
            if (row == null || row.id <= 0)
            {
                error = "ZIP detalle: id no se mapeó.";
                return false;
            }

            if (row._move_id <= 0)
            {
                error = $"ZIP detalle id={row.id}: move_id llegó vacío.";
                return false;
            }

            return true;
        }

        static Task Report(Func<int, int, Task>? onProgress, int current, int total)
        {
            if (onProgress == null)
                return Task.CompletedTask;
            return onProgress(current, Math.Max(total, 1));
        }

        static void TryDeleteDir(string? dir)
        {
            try
            {
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[AdminDocumentPackZip] cleanup: " + ex.Message);
            }
        }
    }
}
