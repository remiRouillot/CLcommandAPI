using Microsoft.AspNetCore.Mvc;
using CLcommandAPI.Services;

namespace CLcommandAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : Controller
    {

        private readonly JobsService jobservice;

        public JobsController(JobsService jobService)
        {
            this.jobservice = jobService;
        }

        [HttpGet("GetActiveJobs")]
        public ActionResult GetActiveJobs(string selectedFields = "")
        {
            string[] fields = string.IsNullOrEmpty(selectedFields) ? new string[] { } : selectedFields.Split(',');

            try
            {
                var activeJobs = jobservice.GetActiveJobs(fields);
                return Ok(activeJobs);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des jobs actifs: {ex.Message}" });
            }
        }

        [HttpPost("HoldJob")]
        public ActionResult HoldJob(string jobName)
        {
            try
            {
                jobservice.HoldJob(jobName);

                return Ok(new  { message = $"Travail {jobName} mis en attente avec succès." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error =  $"Erreur lors de la mise en attente du travail : {ex.Message}" });
            }
        }

        [HttpPost("ReleaseJob")]
        public ActionResult ReleaseJob(string jobName)
        {
            try
            {
                jobservice.ReleaseJob(jobName);

                return Ok(new { message = $"Reprise du travail {jobName} avec succès." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error =  $"Erreur lors de la reprise du travail : {ex.Message}" });
            }
        }

        [HttpPost("EndJob")]
        public ActionResult EndJob(string jobName)
        {
            try
            {
                jobservice.EndJob(jobName);
                return Ok(new { message = $"job {jobName} stoppé avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Erreur lors de l'arrêt du job{jobName} : {ex.Message}"});
            }
        }


        [HttpGet("GetJobMessageByJobName")]
        public ActionResult GetJobMessageByJobName(string jobName)
        {
            try
            {
                var results = jobservice.GetJobMessageByJobName(jobName);
                
                if (results.Any())
                {
                    return Ok(results);
                }
                else
                {
                    return NotFound(new { message = $"Aucun message trouvé pour le job {jobName}." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des messages concernant le job {jobName}: {ex.Message}" });
            }
        }
    }
}
