
using AuthJwt.Mail;
using BikeVille.Auth.AuthContext;
using BikeVille.Entity.EntityContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BikeVille.Admin
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AdventureWorksLt2019usersInfoContext _authContext;
        private readonly AdventureWorksLt2019Context _context;
        private readonly ILogger<AdminController> _logger;
        public AdminController(
            AdventureWorksLt2019usersInfoContext authContext,
            AdventureWorksLt2019Context context,
            ILogger<AdminController> logger)
        {
            _authContext = authContext;
            _context = context;
            _logger = logger;
        }

        [HttpGet("toBeAdmin/{email}")]
        public async Task<IActionResult> toBeAdmin([FromRoute][EmailAddress] string email)
        {
           if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Tentativo di promozione con email null o vuota.");
                return BadRequest("Email non può essere nulla o vuota.");
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);

            if (user == null)
            {
                _logger.LogWarning("Utente non trovato con email: {Email}.", email);
                return NotFound($"Utente con email {email} non trovato.");
            }

            user.Role = "ADMIN";
            _authContext.Users.Update(user);
            _logger.LogInformation("Aggiornamento ruolo utente: {UserId} a ADMIN.", user.UserId);

            var affectedRows = await _authContext.SaveChangesAsync();
            if (affectedRows == 0)
            {
                _logger.LogWarning("Nessuna riga modificata durante l'aggiornamento del ruolo per l'utente {UserId}.", user.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Impossibile aggiornare il ruolo dell'utente.");
            }

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cliente associato rimosso per l'utente {UserId} dopo promozione a ADMIN.", user.UserId);
            }

            _logger.LogInformation("Utente {UserId} promosso a ADMIN con successo.", user.UserId);
            return Ok($"L'utente {email} è ora un ADMIN.");
        }
    }
}