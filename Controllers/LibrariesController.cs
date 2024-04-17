using CLcommandAPI.Services;
using Microsoft.AspNetCore.Mvc;


namespace CLcommandAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class LibrariesController : Controller
    {

        private readonly LibrariesService librariesService;

        public LibrariesController(LibrariesService librariesService)
        {
            this.librariesService = librariesService;
        }


        [HttpPost("CreateLibrary")]
        public ActionResult CreateLibrary(string libraryName)
        {
            try
            {
                var result = librariesService.CreateLibrary(libraryName);
                // Si resultat librariesService Succès
                if (result.EndsWith("créée avec succès."))
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
                return StatusCode(500, new { message = $"Erreur lors de la création de la bibliothèque : {ex.Message}" });
            }
        }



        [HttpDelete("DeleteLibrary")]
        public ActionResult DeleteLibrary(string libraryName)
        {
            try
            {
                var result = librariesService.DeleteLibrary(libraryName);
                if (result.Contains("supprimée avec succès"))
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
                return StatusCode(500, new { message = $"Erreur lors de la suppression de la bibliothèque : {ex.Message}" });
            }
        }

        [HttpGet("GetLibraryName")]

        public IActionResult GetLibraryName()
        {
            try
            {
                var librariesName = librariesService.GetLibraryName();
                return Ok(librariesName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des noms de bibliothèques : {ex.Message}" });
            }
        }



        [HttpGet("ListAndFilterObjectsOrLibraries")]
        public IActionResult ListAndFilterObjectsOrLibraries(string libraryName = "*All", string selectedFields = "")
        {
            // Transformation champs sélectionnés en tableau
            string[] fields = string.IsNullOrEmpty(selectedFields) ? new string[] { } : selectedFields.Split(',');

            try
            {
                var listDetails = librariesService.ListAndFilterLibraryObjectsFields(libraryName, fields);
                return Ok(listDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la liste et du filtrage des items: {ex.Message}" });
            }
        }
    }
}
