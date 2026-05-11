using Microsoft.AspNetCore.Mvc;

namespace WebMobileManager.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet("checkonline")]
        public async Task<ActionResult> CheckOnline()
        {
            var result = new
            {
                responseCode = 200,
                success = true,
                message = "Everything ok",
                data = new[]
                {
                    new {
                        status = "online",
                        descripcion = "Builder Server is running"
                    }
                }
            };

            return Ok(result);
        }
    }
}
