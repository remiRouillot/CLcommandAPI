using Microsoft.AspNetCore.Mvc;
using CLcommandAPI.Services;

namespace CLcommandAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class SystemController : Controller
    {
        private readonly SystemService systemService;

        public SystemController(SystemService systemService)
        {
            this.systemService = systemService;
        }

        [HttpGet("GetInformationSystem")]
        public IActionResult GetInformationSystem()
        {
            try
            {
                var results = systemService.GetInformationSystem();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des informations systemes : {ex.Message}" });
            }

        }
    }
}
