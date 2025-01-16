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
                    Credentials = new NetworkCredential("adc1d4cf235e16", "325f87715bb449"),
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
                var smtpClient = new SmtpClient("smtp.mailtrap.io")
                {
                    Port = 2525,
                    Credentials = new NetworkCredential("adc1d4cf235e16", "325f87715bb449"),
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
                var smtpClient = new SmtpClient("smtp.mailtrap.io")
                {
                    Port = 2525,
                    Credentials = new NetworkCredential("adc1d4cf235e16", "325f87715bb449"),
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

