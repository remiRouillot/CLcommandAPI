using Aumerial.Data.Nti;
using CLcommandAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLcommandAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {

        private readonly UserService userService;
        
        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("ListAndFilterUserByName")]

        public IActionResult ListAndFilterUserByName(string userName = "*All")
        {
            try
            {
            var result = userService.ListAndFilterUserByName(userName);
            
                if(result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(new { message = result });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des utilisateurs : {ex.Message}" });
            }
        }
    }
}
