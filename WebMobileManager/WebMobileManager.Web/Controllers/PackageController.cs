using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Abstract.Server.Dto;
using Microsoft.AspNetCore.Mvc;
using WebMobileManager.Web.Components.Pages.Packages;
using WebMobileManager.Web.Services.Interfaces;
using WebMobileManager.Web.Services.Sqlite;

namespace WebMobileManager.Web.Controllers
{   

    [ApiController]
    [Route("api/[controller]")]
    public class PackageController : ControllerBase
    {
        private readonly PackageDb _packageDb;
        private readonly PackageFileDb _fileDb;
        private readonly IPackageService _packageService;

        public PackageController(
            PackageDb packageDb,
            PackageFileDb fileDb,
            IPackageService packageService)
        {
            _packageDb = packageDb;
            _fileDb = fileDb;
            _packageService = packageService;
        }

        // 1. Crear paquete
        [HttpPost("create")]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(
                    new
                    {
                        success_upload = false,
                        status = "error",
                        error = "Nombre inválido"
                    }                    
                    );

            var existing = await _packageDb.GetByName(dto.Name);

            if (existing != null)
                return BadRequest(
                    new
                    {
                        success_upload = false,
                        status = "error",
                        error = "El paquete ya existe"
                    }                    
                    );

            bool is_base = false;

            if (dto.is_base.HasValue) 
            {
                is_base = dto.is_base.Value;
            }

            var pkg = new Package
            {
                name = dto.Name,
                server = dto.server,
                database_name = dto.database_name,
                file_name = dto.file_name,
                file_type = dto.file_type,
                date_data_cutoff = dto.date_data_cutoff ?? DateTime.UtcNow,
                mobile_app_id = dto.mobile_app_id,
                user_frontend = dto.user_frontend,
                total_files_expected = dto.total_files_expected ?? 0,
                total_file_size_expected = dto.total_file_size_expected ?? 0,
                external_guid = dto.external_guid,
                created_at = DateTime.UtcNow,
                processing_state = "Pending",                
                total_files_uploaded = 0,
                success_upload = true,
                is_base = is_base,
            };

            await _packageDb.InsertAsync(pkg);

            return Ok(pkg);
        }

        // 2. Obtener paquete por nombre
        [HttpGet("{name}")]
        public async Task<IActionResult> GetPackage(string name)
        {
            var pkg = await _packageDb.GetByName(name);

            if (pkg == null)
                return NotFound();

            return Ok(pkg);
        }

        // 3. Listar paquetes
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var list = await _packageDb.GetAll();
        //    return Ok(list);
        //}

        [HttpPost("{packageName}/file")]
            public async Task<IActionResult> AddFile(
            string packageName,
            [FromForm] IFormFile file,
            [FromForm] CreatePackageFileDto dto)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new
                {
                    success_upload = false,
                    status = "error",
                    error = "Archivo vacío"
                });

            var pkg = await _packageDb.GetByName(packageName);

            if (pkg == null)
                return NotFound(new {
                    success_upload = false,
                    status = "error",
                    error = "Paquete no existe"
                });

            var folderPath = Path.Combine("wwwroot/uploads/zips", packageName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            //var filePath = Path.Combine(folderPath, dto.File.FileName);

            var filePath = Path.Combine(folderPath, dto.file_name);

            if (System.IO.File.Exists(filePath))
                return Conflict(new
                {
                    success_upload = false,
                    status = "error",
                    error = "Archivo ya existe"
                });

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = BuildFileUrl(packageName, dto.file_name);

            var fileRecord = new PackageFile
            {
                package_id = pkg.id,
                file_name = dto.file_name,
                file_type = dto.file_type,
                file_path = filePath,
                url = url,
                total_file_size_expected = dto.total_file_size_expected ?? (int)file.Length,
                success_upload = dto.success_upload ?? true,
                uploaded_at = DateTime.UtcNow,
                status = "Uploaded",
                error = dto.error
            };

            await _fileDb.InsertAsync(fileRecord);

            pkg.total_files_uploaded += 1;

            await _packageDb.UpdateAsync(pkg);

            return Ok(fileRecord);
        }

        // 5. Obtener archivos de un paquete
        [HttpGet("{packageName}/files")]
        public async Task<IActionResult> GetFiles(string packageName)
        {
            var pkg = await _packageDb.GetByName(packageName);

            if (pkg == null)
                return NotFound();

            var files = await _fileDb.GetByPackage(pkg.id);

            var result = files.Select(file => new PackageFileResponseDto
            {
                id = file.id,
                file_name = file.file_name,
                file_type = file.file_type,
                uploaded_at = file.uploaded_at,
                status = file.status,
                url = BuildFileUrl(packageName, file.file_name)
            });

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] PackageFilterDto filter)
        {
            var list = await _packageDb.GetAll(filter);
            return Ok(list);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var data = await _packageService.DownloadFullPackage(id);

            if (data == null)
                return NotFound();

            return File(data, "application/zip", $"package_{id}.zip");
        }

        private string BuildFileUrl(string packageName, string fileName)
        {
            return $"{Request.Scheme}://{Request.Host}/uploads/zips/{packageName}/{fileName}";
        }
    }
}