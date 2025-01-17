using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;

namespace AuthJwt.Mail
{
    [Route("[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        [HttpPost("successRegister")]
        public async Task<IActionResult> successRegister([FromBody] EmailRequest request)
        {
            try
            {
                //var smtpClient = new SmtpClient("smtp.mailtrap.io")
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("BikeVille.commercial@gmail.com", "othvgfyosjudjjrn"),
                    // Credentials = new NetworkCredential("9eee65065e91d5", "a9dd17c0e5f652"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("bikeville.commercial@gmail.com"),
                    Subject = request.Subject,
                    Body = request.Message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(request.ToEmail);

                await smtpClient.SendMailAsync(mailMessage);

                return Ok(new { message = "Email inviata con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("toBeAdmin")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("BikeVille.commercial@gmail.com", "othvgfyosjudjjrn"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("bikeville.commercial@gmail.com"),
                    Subject = "Richiesta di collaborazione",
                    Body = request.Message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add("bikeville.commercial@gmail.com");

                await smtpClient.SendMailAsync(mailMessage);

                return Ok(new { message = "Email inviata con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("recoverPassword")]
        public async Task<IActionResult> SendEmailrecoverPassword([FromBody] EmailRequest request)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("BikeVille.commercial@gmail.com", "othvgfyosjudjjrn"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("bikeville.commercial@gmail.com"),
                    Subject = "Recupero password",
                    Body = request.Message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(request.ToEmail);

                await smtpClient.SendMailAsync(mailMessage);

                return Ok(new { message = "Email inviata con successo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

