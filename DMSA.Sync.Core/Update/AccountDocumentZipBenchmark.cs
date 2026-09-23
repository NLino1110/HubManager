using ApiManager;
using ApiManagerOdoo.Accounting;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace DMSA.Sync.Core.Update
{
    /// <summary>
    /// Benchmark cabecera/detalle: Odoo → GZIP → descarga HTTP → descompresión → SQLite.
    /// Activar con EnableAccountMoveLineZipBenchmarkSync en ServerPuller.
    /// </summary>
    public static class AccountDocumentZipBenchmark
    {
        // Solo benchmark (no afecta sync en línea ni DbLimitDefault global).
        const int BenchmarkOdooPageSize = 2500;
        const int BenchmarkSqliteBatchSize = 2500;

        public sealed class BenchmarkResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
            public int RecordCount { get; set; }
            public long ZipBytes { get; set; }
            public TimeSpan PrepOdooFetch { get; set; }
            public TimeSpan PrepZipCreate { get; set; }
            public TimeSpan PrepUpload { get; set; }
            public TimeSpan Download { get; set; }
            public TimeSpan Decompress { get; set; }
            public TimeSpan DbInsert { get; set; }

            public string FormatSummary()
            {
                if (!Success)
                    return $" [GZIP {Label}: ERROR — {Message}]";

                return $" [GZIP {Label} benchmark — " +
                    $"{RecordCount} registros, {ZipBytes / 1024.0 / 1024.0:F2} MB | " +
                    $"prep Odoo {FormatSpan(PrepOdooFetch)}, gzip {FormatSpan(PrepZipCreate)}, upload {FormatSpan(PrepUpload)} | " +
                    $"descarga {FormatSpan(Download)}, descomprimir {FormatSpan(Decompress)}, SQLite {FormatSpan(DbInsert)}]";
            }

            static string FormatSpan(TimeSpan span) =>
                $"{span.Minutes:D2}m {span.Seconds:D2}s {span.Milliseconds:D3}ms";
        }

        sealed class AccountMoveZipPayload
        {
            [JsonProperty("account_move")]
            public account_move[] Items { get; set; } = Array.Empty<account_move>();
        }

        sealed class AccountMoveLineZipPayload
        {
            [JsonProperty("account_move_line")]
            public account_move_line[] Items { get; set; } = Array.Empty<account_move_line>();
        }

        public static Task<BenchmarkResult> RunHeaderAsync(
            AppSession appSession,
            DateTime dateFrom,
            DateTime dateTo,
            Func<string, int, int, Task>? onProgress = null)
        {
            return RunAsync(
                appSession,
                dateFrom,
                dateTo,
                label: "cabecera",
                workDirPrefix: "am_gzip_benchmark_",
                uploadStemPrefix: "am_benchmark_",
                jsonFileName: "account_move.json",
                onProgress,
                fetchAsync: FetchHeadersFromOdooAsync,
                insertAsync: InsertHeadersAsync);
        }

        public static Task<BenchmarkResult> RunDetailAsync(
            AppSession appSession,
            DateTime dateFrom,
            DateTime dateTo,
            Func<string, int, int, Task>? onProgress = null)
        {
            return RunAsync(
                appSession,
                dateFrom,
                dateTo,
                label: "detalle",
                workDirPrefix: "aml_gzip_benchmark_",
                uploadStemPrefix: "aml_benchmark_",
                jsonFileName: "account_move_line.json",
                onProgress,
                fetchAsync: FetchLinesFromOdooAsync,
                insertAsync: InsertLinesAsync);
        }

        delegate Task<int> FetchDelegate(
            AppSession appSession,
            DateTime dateFrom,
            DateTime dateTo,
            string workDir,
            Func<string, int, int, Task>? onProgress);

        delegate Task<int> InsertDelegate(
            AppSession appSession,
            string jsonPath,
            Func<string, int, int, Task>? onProgress);

        static async Task<BenchmarkResult> RunAsync(
            AppSession appSession,
            DateTime dateFrom,
            DateTime dateTo,
            string label,
            string workDirPrefix,
            string uploadStemPrefix,
            string jsonFileName,
            Func<string, int, int, Task>? onProgress,
            FetchDelegate fetchAsync,
            InsertDelegate insertAsync)
        {
            var result = new BenchmarkResult { Label = label };
            string? workDir = null;
            string? archivePath = null;

            try
            {
                if (appSession?.odooConnection == null)
                {
                    result.Message = "Sin conexión Odoo activa.";
                    return result;
                }

                workDir = Path.Combine(FileSystem.CacheDirectory, workDirPrefix + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(workDir);

                var fetchWatch = Stopwatch.StartNew();
                int recordCount = await fetchAsync(appSession, dateFrom, dateTo, workDir, onProgress);
                fetchWatch.Stop();
                result.PrepOdooFetch = fetchWatch.Elapsed;
                result.RecordCount = recordCount;

                if (recordCount == 0)
                {
                    result.Message = "Odoo no devolvió registros para el rango seleccionado.";
                    return result;
                }

                var gzipWatch = Stopwatch.StartNew();
                archivePath = await CreateGzipArchiveAsync(workDir, jsonFileName);
                gzipWatch.Stop();
                result.PrepZipCreate = gzipWatch.Elapsed;
                result.ZipBytes = new FileInfo(archivePath).Length;

                var uploadWatch = Stopwatch.StartNew();
                string? downloadUrl = await UploadArchiveAsync(
                    appSession, archivePath, dateFrom, dateTo, uploadStemPrefix);
                uploadWatch.Stop();
                result.PrepUpload = uploadWatch.Elapsed;

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    result.Message = "No se pudo subir el GZIP al servidor de recursos (HostDump).";
                    return result;
                }

                await ReportProgress(onProgress, $"Descargando GZIP ({label})", 0, 1);
                var downloadWatch = Stopwatch.StartNew();
                var hub = new HubMnsaAttachment(appSession);
                byte[] downloadedBytes = await hub.DownloadFileMode2Async(downloadUrl);
                downloadWatch.Stop();
                result.Download = downloadWatch.Elapsed;

                if (downloadedBytes == null || downloadedBytes.Length == 0)
                {
                    result.Message = "La descarga GZIP no devolvió datos.";
                    return result;
                }

                result.ZipBytes = downloadedBytes.Length;

                await ReportProgress(onProgress, $"Descomprimiendo GZIP ({label})", 0, 1);
                var decompressWatch = Stopwatch.StartNew();
                string jsonPath = Path.Combine(workDir, "payload.json");
                await DecompressGzipToFileAsync(downloadedBytes, jsonPath);
                decompressWatch.Stop();
                result.Decompress = decompressWatch.Elapsed;

                await ReportProgress(onProgress, $"Insertando en SQLite ({label})", 0, 1);
                var insertWatch = Stopwatch.StartNew();
                int inserted = await insertAsync(appSession, jsonPath, onProgress);
                insertWatch.Stop();
                result.DbInsert = insertWatch.Elapsed;
                result.RecordCount = inserted;
                result.Success = true;
                result.Message = "OK";
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountDocumentZipBenchmark:{label}] {ex}");
                result.Message = ex.Message;
                return result;
            }
            finally
            {
                CleanupWorkDir(workDir);
            }
        }

        static void CleanupWorkDir(string? workDir)
        {
            try
            {
                if (workDir == null)
                    return;

                if (Directory.Exists(workDir))
                    Directory.Delete(workDir, true);

                // HostDump exige extensión .zip aunque el contenido sea GZIP.
                string archivePath = workDir + ".zip";
                if (File.Exists(archivePath))
                    File.Delete(archivePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountDocumentZipBenchmark] cleanup: {ex.Message}");
            }
        }

        static async Task<int> FetchHeadersFromOdooAsync(
            AppSession appSession,
            DateTime dateFrom,
            DateTime dateTo,
            string workDir,
            Func<string, int, int, Task>? onProgress)
        {
            var headers = new List<account_move>();
            var hub = new HubAccountMove(appSession);
            int limit = BenchmarkOdooPageSize;

            // PRUEBA: sin filtro por partner_id (solo rango invoice_date).
            // var partnerDb = new ResPartnerDb(appSession.odooConnection.DbNameSqlite);
            // int commercialPartnerId = appSession.CurrentUserFront.partner_id;
            // int[] partnerIds = await partnerDb.GetAllPartnerIdsByAdicComercialAsync(commercialPartnerId);
            int[] partnerIds = Array.Empty<int>();

            var headerCount = await hub.GetHeaderCountByInvoiceDateRange(dateFrom.Date, dateTo.Date, partnerIds);
            if (headerCount == null || headerCount.result == 0)
                return 0;

            int totalPages = (int)Math.Ceiling((double)headerCount.result / limit);

            for (int page = 0; page <= totalPages; page++)
            {
                var response = await hub.GetAccountMovesByInvoiceDateRange(
                    dateFrom.Date, dateTo.Date, limit, page, partnerIds);

                if (response?.result != null && response.result.Length > 0)
                    headers.AddRange(response.result);

                await ReportProgress(onProgress, "Preparando GZIP cabecera (Odoo)", page + 1, Math.Max(totalPages, 1));

                if (page >= 600)
                    break;
            }

            string jsonPath = Path.Combine(workDir, "account_move.json");
            var payload = new AccountMoveZipPayload { Items = headers.ToArray() };
            await File.WriteAllTextAsync(jsonPath, JsonConvert.SerializeObject(payload), Encoding.UTF8);
            return headers.Count;
        }

        static async Task<int> FetchLinesFromOdooAsync(
            AppSession appSession,
            DateTime dateFrom,
            DateTime dateTo,
            string workDir,
            Func<string, int, int, Task>? onProgress)
        {
            var lines = new List<account_move_line>();
            var hub = new HubAccountMoveLine(appSession);
            int limit = BenchmarkOdooPageSize;

            // PRUEBA: sin filtro por move_id local (solo rango invoice_date del move).
            // var moveDb = new AccountMoveDb(appSession.odooConnection.DbNameSqlite);
            // int[] moveIds = await moveDb.GetIdsByInvoiceDateRangeAsync(dateFrom.Date, dateTo.Date);
            int[] moveIds = Array.Empty<int>();

            var lineCount = await hub.GetDetailCountByInvoiceDateRange(dateFrom.Date, dateTo.Date, moveIds);
            if (lineCount == null || lineCount.result == 0)
                return 0;

            int totalPages = (int)Math.Ceiling((double)lineCount.result / limit);

            for (int page = 0; page <= totalPages; page++)
            {
                var response = await hub.GetAccountMoveLinesByInvoiceDateRange(
                    dateFrom.Date, dateTo.Date, limit, page, moveIds);

                if (response?.result != null && response.result.Length > 0)
                    lines.AddRange(response.result);

                await ReportProgress(onProgress, "Preparando GZIP detalle (Odoo)", page + 1, Math.Max(totalPages, 1));

                if (page >= 600)
                    break;
            }

            string jsonPath = Path.Combine(workDir, "account_move_line.json");
            var payload = new AccountMoveLineZipPayload { Items = lines.ToArray() };
            await File.WriteAllTextAsync(jsonPath, JsonConvert.SerializeObject(payload), Encoding.UTF8);
            return lines.Count;
        }

        static async Task<int> InsertHeadersAsync(
            AppSession appSession,
            string jsonPath,
            Func<string, int, int, Task>? onProgress)
        {
            string jsonContent = await File.ReadAllTextAsync(jsonPath);
            var payload = JsonConvert.DeserializeObject<AccountMoveZipPayload>(jsonContent);

            if (payload?.Items == null || payload.Items.Length == 0)
                throw new InvalidOperationException("El JSON descomprimido no contiene cabeceras.");

            var database = new AccountMoveDb(appSession.odooConnection.DbNameSqlite);

            for (int offset = 0; offset < payload.Items.Length; offset += BenchmarkSqliteBatchSize)
            {
                var batch = payload.Items.Skip(offset).Take(BenchmarkSqliteBatchSize).ToArray();
                await database.InsertBatchAsync(batch);

                int page = (offset / BenchmarkSqliteBatchSize) + 1;
                int totalPages = (int)Math.Ceiling((double)payload.Items.Length / BenchmarkSqliteBatchSize);
                await ReportProgress(onProgress, "Insertando cabeceras en SQLite", page, totalPages);
            }

            return payload.Items.Length;
        }

        static async Task<int> InsertLinesAsync(
            AppSession appSession,
            string jsonPath,
            Func<string, int, int, Task>? onProgress)
        {
            string jsonContent = await File.ReadAllTextAsync(jsonPath);
            var payload = JsonConvert.DeserializeObject<AccountMoveLineZipPayload>(jsonContent);

            if (payload?.Items == null || payload.Items.Length == 0)
                throw new InvalidOperationException("El JSON descomprimido no contiene líneas.");

            var database = new AccountMoveLineDb(appSession.odooConnection.DbNameSqlite);

            for (int offset = 0; offset < payload.Items.Length; offset += BenchmarkSqliteBatchSize)
            {
                var batch = payload.Items.Skip(offset).Take(BenchmarkSqliteBatchSize).ToArray();
                await database.InsertBatchAsync(batch);

                int page = (offset / BenchmarkSqliteBatchSize) + 1;
                int totalPages = (int)Math.Ceiling((double)payload.Items.Length / BenchmarkSqliteBatchSize);
                await ReportProgress(onProgress, "Insertando detalle en SQLite", page, totalPages);
            }

            return payload.Items.Length;
        }

        /// <summary>
        /// Comprime JSON con GZIP. El archivo usa extensión .zip por restricción de HostDump.
        /// </summary>
        static async Task<string> CreateGzipArchiveAsync(string workDir, string jsonFileName)
        {
            string jsonPath = Path.Combine(workDir, jsonFileName);
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("No se encontró JSON para comprimir.", jsonFileName);

            string archivePath = workDir + ".zip";
            if (File.Exists(archivePath))
                File.Delete(archivePath);

            await using (var input = File.OpenRead(jsonPath))
            await using (var output = File.Create(archivePath))
            await using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                await input.CopyToAsync(gzip);
            }

            return archivePath;
        }

        static async Task DecompressGzipToFileAsync(byte[] gzipBytes, string jsonOutputPath)
        {
            await using var input = new MemoryStream(gzipBytes);
            await using var gzip = new GZipStream(input, CompressionMode.Decompress);
            await using var output = File.Create(jsonOutputPath);
            await gzip.CopyToAsync(output);
        }

        static async Task<string?> UploadArchiveAsync(
            AppSession appSession,
            string archivePath,
            DateTime dateFrom,
            DateTime dateTo,
            string uploadStemPrefix)
        {
            byte[] archiveBytes = await File.ReadAllBytesAsync(archivePath);
            var hub = new HubMnsaAttachment(appSession);

            string fileStem = $"{uploadStemPrefix}{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}_{DateTime.Now:HHmmss}";
            string packageName = $"{appSession.AppCodeOdoo}_{uploadStemPrefix}{fileStem}";

            var upload = await hub.SendToExternalServer(archiveBytes, fileStem, packageName);
            if (upload == null || upload.status_code < 200 || upload.status_code >= 300)
                return null;

            return upload.url;
        }

        static Task ReportProgress(Func<string, int, int, Task>? onProgress, string stage, int current, int total)
        {
            if (onProgress == null)
                return Task.CompletedTask;

            return onProgress(stage, current, total);
        }
    }
}
