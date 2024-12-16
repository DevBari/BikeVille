using AuthJwt.Mail;
using BikeVille.Auth.AuthContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BikeVille.Admin
{
    // Definizione della classe AdminController, che gestisce le operazioni amministrative.
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private AdventureWorksLt2019usersInfoContext _context;

        // Costruttore della classe che inizializza il contesto del database (AdventureWorksLt2019usersInfoContext).
        public AdminController(AdventureWorksLt2019usersInfoContext context)
        {
            _context = context;
        }

        // Endpoint che consente a un amministratore di promuovere un utente a ruolo ADMIN.
        // Solo gli utenti con il ruolo "ADMIN" possono accedere a questa operazione.
        [HttpGet("toBeAdmin/{email}")]
        [Authorize(Roles = "ADMIN")]  // Autorizzazione per gli utenti con il ruolo "ADMIN".
        public async Task<IActionResult> toBeAdmin(string email)
        {
            // Verifica se l'email è valida (non null).
            if (email != null)
            {
                // Controlla se esiste un utente con l'email fornita nel database.
                if (_context.Users.Any(x => x.EmailAddress == email))
                {
                    // Se l'utente esiste, gli viene assegnato il ruolo "ADMIN".
                    _context.Users.FirstOrDefault(x => x.EmailAddress == email).Role = "ADMIN";
                    // Salva le modifiche nel database.
                    await _context.SaveChangesAsync();
                    // Restituisce una risposta OK con il messaggio di successo.
                    return Ok("User is now an admin");
                }
                else
                {
                    // Se l'email non corrisponde a nessun utente, restituisce una risposta BadRequest.
                    return BadRequest("Email not found");
                }
            }
            else
            {
                // Se l'email è null, restituisce una risposta BadRequest con un messaggio appropriato.
                return BadRequest("Email null");
            }
        }
    }
}
