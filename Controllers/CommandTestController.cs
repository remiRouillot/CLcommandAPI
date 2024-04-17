using CLcommandAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLcommandAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CommandTestController : Controller
    {
        private readonly ClcommandServiceAPi _clCommandServiceApi;

        public CommandTestController(ClcommandServiceAPi clCommanServiceApi)
        {
            _clCommandServiceApi = clCommanServiceApi;
        }

        [HttpGet("TestCommandDetails")]
        public async Task<IActionResult> TestCommandDetails(string commandName)
        {
            try
            {
                var commandDetails = await _clCommandServiceApi.GetCommandDetailsAsync(commandName.ToUpper(), "*LIBL");
                return Ok(commandDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des données de l'API QCCDMD : {ex.Message}" });
            }
        }
    }
}
