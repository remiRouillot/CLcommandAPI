using CLcommandAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLcommandAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MessageQueueController : Controller
    {

        private readonly MessageQueueService messageQueueService;

        public MessageQueueController(MessageQueueService messageQueueService)
        {
            this.messageQueueService = messageQueueService;
        }

        [HttpGet("GetMessageQueueInfo")]
        public IActionResult GetMessageQueueInfo(string queueLibrary = "QSYS", string queueName = "QSYSOPR", string filterType = "")
        {
            try
            {
                var results = messageQueueService.GetMessageQueueInfo(queueLibrary, queueName, filterType.ToUpper());
                if (results.Any())
                {
                    return Ok(results);
                }
                else
                {
                    return NotFound(new { message = results });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération des messages : {ex.Message}" });
            }
        }

        [HttpGet("GetInquiryMessages")]
        public IActionResult GetInquiryMessages()
        {
            try
            {
                var inquiryMessages = messageQueueService.GetInquiryMessages();
                if (inquiryMessages.Any())
                {
                    return Ok(inquiryMessages);
                }
                else
                {
                    return Ok(new { message = "Aucun message INQUIRY trouvé." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne du serveur : {ex.Message}" });
            }
        }



        [HttpPost("ReplyToInquiryMessage")]
        public IActionResult ReplyToInquiryMessage(string messageKey, string reply)
        {
            reply = reply.ToUpper(); 
            if (string.IsNullOrEmpty(messageKey) || string.IsNullOrEmpty(reply))
            {
                return BadRequest(new { message = "Clé du message et réponse requises." });
            }

            var validReplies = new HashSet<string> { "I", "C" }; // I pour Ignore, C pour Cancel
            if (!validReplies.Contains(reply))
            {
                return BadRequest(new { message = "Type de réponse non valide." });
            }

            try
            {
                messageQueueService.ReplyToInquiryMessage(messageKey, reply);
                return Ok(new { message = "Réponse au message envoyée avec succès." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne du serveur : {ex.Message}" });
            }
        }

        [HttpGet("GetHistoryLogInfo")]

        public IActionResult GetHistoryLogInfo(int daysAgo)
        {
            try
            {
                var results = messageQueueService.GetHistoryLogInfo(daysAgo);
                if (results != null)
                {
                    return Ok(results);
                }
                else
                {
                    return NotFound(new { message = $"Aucun historique de log trouvé." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la récupération de l'historique des logs : {ex.Message}" });
            }
        }

        [HttpPost("SendBreakMessage")]

        public IActionResult SendBreakMessage(string messageText, string messageQueue = "*ALLWS", string messageType = "*INFO")
        {
            try
            {
                var results = messageQueueService.SendBreakMessage(messageText, messageQueue, messageType);
                if (results != null)
                {
                    return Ok(results);
                }
                else
                {
                    return BadRequest(new { message = $"Impossible d'envoyer le message." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de l'envoi du messa : {ex.Message}" });
            }
        }

        [HttpGet("ExportMessagesToExcel")]

        public IActionResult ExportMessagesToExcel(string queueLibrary = "QSYS", string queueName = "QSYSOPR", string filterType = "")
        {
            try
            {
                var messages = messageQueueService.GetMessageQueueInfo(queueLibrary, queueName, filterType.ToUpper());
                
                string fileName = "MessagesIBMi"; // Base nom fichier

                if (!string.IsNullOrEmpty(filterType))
                {
                    fileName += $"_TYPE_{filterType.ToUpper()}"; 
                }

                fileName += ".xlsx";

                var excelFile = messageQueueService.ExportMessagesToExcel(messages);
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur lors de la création du fichier Excel : {ex.Message}" });
            }
        }
    }
}
