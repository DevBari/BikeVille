using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

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
                var smtpClient = new SmtpClient("smtp.mailtrap.io")
                {
                    Port = 2525,
                    Credentials = new NetworkCredential("a9b05977a60d8a", "1b1c4f40d81cec"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("testemaildan98202@gmail.com"),
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
                var smtpClient = new SmtpClient("smtp.mailtrap.io")
                {
                    Port = 2525,
                    Credentials = new NetworkCredential("a9b05977a60d8a", "1b1c4f40d81cec"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("testemaildan98202@gmail.com"),
                    Subject = "Richiesta di collaborazione",
                    Body = request.Message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add("testemaildan98202@gmail.com");

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
                var smtpClient = new SmtpClient("smtp.mailtrap.io")
                {
                    Port = 2525,
                    Credentials = new NetworkCredential("a9b05977a60d8a", "1b1c4f40d81cec"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("testemaildan98202@gmail.com"),
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

