using Microsoft.AspNetCore.Mvc;
using CLcommandAPI.Services;
using Aumerial.Data.Nti;

namespace CLcommandAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class JournalController : Controller
    {
        private readonly JournalService journalService;

        public JournalController(JournalService journalService)
        {
            this.journalService = journalService;
        }

        [HttpGet("GetAllJournals")]

        public IActionResult GetAllJournals()
        {
            try
            {
                var result = journalService.GetAllJournals();
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(new { message = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des journaux : {ex.Message}" });
            }
        }


        [HttpGet("ListAndFilterJournal")]
        public IActionResult ListAndFilterJournal(string journalName, string journalLib)
        {
            try
            {
                var result = journalService.ListAndFilterByJournalName(journalName, journalLib);

                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(new { message = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des journaux : {ex.Message}" });
            }
        }
    }

}
