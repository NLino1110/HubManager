using Microsoft.AspNetCore.Mvc;

namespace WebMobileManager.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        //[HttpPost("image")]
        //public async Task<IActionResult> UploadImage(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("Archivo vacío");

        //    var allowedTypes = new[] { "image/jpeg", "image/png" };
        //    if (!allowedTypes.Contains(file.ContentType))
        //        return BadRequest("Solo se permiten imágenes JPG o PNG");

        //    var folderPath = Path.Combine(_env.WebRootPath, "uploads");
        //    if (!Directory.Exists(folderPath))
        //        Directory.CreateDirectory(folderPath);

        //    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        //    var filePath = Path.Combine(folderPath, fileName);

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

        //    return Ok(new { url });
        //}

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] string sku,
            [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            const string VALID_API_KEY = "t.0.0.r.1381";

            if (apiKey != VALID_API_KEY)
                return Unauthorized("No autorizado");

            if (file == null || file.Length == 0)
                return BadRequest("Archivo vacío");

            if (string.IsNullOrWhiteSpace(sku))
                return BadRequest("Debe enviar el SKU");

            //var allowedTypes = new[] { "image/jpeg", "image/png" };
            //if (!allowedTypes.Contains(file.ContentType))
            //    return BadRequest("Solo se permiten imágenes JPG o PNG");

            //var extension = Path.GetExtension(file.FileName).ToLower();

            //if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            //    return BadRequest("Extensión inválida");

            var allowedTypes = new[] { "image/jpeg" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Solo se permiten imágenes JPG (extensión .jpeg)");

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".jpeg")
                return BadRequest("Extensión inválida, solo extensión .jpeg permitida");

            var folderPath = Path.Combine(_env.WebRootPath, "uploads/images");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var safeSku = new string(sku.Where(char.IsLetterOrDigit).ToArray());
            var fileName = $"{safeSku}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{Request.Scheme}://{Request.Host}/uploads/images/{fileName}";

            return Ok(new { url });
        }

        [HttpPost("zip_old")]
        public async Task<IActionResult> UploadZip_old(
            IFormFile file,
            [FromForm] string fileName,
            [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            const string VALID_API_KEY = "t.0.0.r.1381";

            if (apiKey != VALID_API_KEY)
                return Unauthorized("No autorizado");

            if (file == null || file.Length == 0)
                return BadRequest("Archivo vacío");

            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Debe enviar el nombre del archivo");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".zip")
                return BadRequest("Extensión inválida");

            var folderPath = Path.Combine(_env.WebRootPath, "uploads/zips");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        
            var safeName = new string(nameWithoutExt
                .Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-')
                .ToArray());
                        
            if (string.IsNullOrWhiteSpace(safeName))
                return BadRequest("Nombre de archivo inválido");

            var finalName = $"{safeName}.zip";
            var filePath = Path.Combine(folderPath, finalName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{Request.Scheme}://{Request.Host}/uploads/zips/{finalName}";

            return Ok(new { url });
        }


        [HttpPost("zip")]
        public async Task<IActionResult> UploadZip(
            IFormFile file,
            [FromForm] string fileName,
            [FromForm] string packageName,
            [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            const string VALID_API_KEY = "t.0.0.r.1381";

            if (apiKey != VALID_API_KEY)
                return Unauthorized("No autorizado");

            if (file == null || file.Length == 0)
                return BadRequest("Archivo vacío");

            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Debe enviar el nombre del archivo");

            if (string.IsNullOrWhiteSpace(packageName))
                return BadRequest("Debe enviar el nombre del paquete");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".zip")
                return BadRequest("Extensión inválida");

            var folderPath = Path.Combine(_env.WebRootPath, "uploads/zips", packageName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            var safeName = new string(nameWithoutExt
                .Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-')
                .ToArray());

            if (string.IsNullOrWhiteSpace(safeName))
                return BadRequest("Nombre de archivo inválido");

            var finalName = $"{safeName}.zip";
            var filePath = Path.Combine(folderPath, finalName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{Request.Scheme}://{Request.Host}/uploads/zips/{packageName}/{finalName}";

            return Ok(new { url });
        }
    }
}
