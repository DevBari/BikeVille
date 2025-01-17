using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Auth;
using BikeVille.Auth.AuthContext;
using BikeVille.Entity.EntityContext;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using BikeVille.CriptingDecripting;
using System.Text;

using BikeVille.Entity;

namespace BikeVille.Auth.AuthController
{
    // Controller per gestire le operazioni sugli utenti
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AdventureWorksLt2019usersInfoContext _authContext;
        private readonly AdventureWorksLt2019Context _context;
        private readonly ILogger<UsersController> _logger;
       

        // Costruttore che inietta  entrambi i contesti del database sia user che customer
        public UsersController(AdventureWorksLt2019usersInfoContext authContext, AdventureWorksLt2019Context context, ILogger<UsersController> logger)
        {
            _authContext = authContext;
            _context = context;
            _logger = logger;

        }

        // GET: api/Users
        // Restituisce una lista di tutti gli utenti
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _authContext.Users.ToListAsync();
            return Ok(users);
        }

        // GET: api/Users/5
        // Restituisce i dettagli di un singolo utente tramite ID
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);

            if (user == null)
            {
                _logger.LogWarning("Utente non trovato con ID {UserId}.", id);
                return NotFound("Utente non trovato.");
            }

            return Ok(user);
        }

        // GET: api/Users/UserPass/{password}/{id}
        // Verifica se la password fornita corrisponde all'utente con ID specificato
        [HttpGet("UserPass/{password}/{id}")]
        public async Task<ActionResult<bool>> GetUserPass(int id, string password)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Verifica password fallita: utente non trovato con ID {UserId}.", id);
                return NotFound("Utente non trovato.");
            }

            bool isValid = SaltEncrypt.SaltDecryptPass(password, user.PasswordSalt) == user.PasswordHash;
            return Ok(isValid);
        }

        // GET: api/Users/AuthUser/{emailAddress}
        // Restituisce l'utente corrispondente all'indirizzo email
        [HttpGet("AuthUser/{emailAddress}")]
         public async Task<ActionResult<User>> GetUser(string emailAddress)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.EmailAddress.Equals(emailAddress));

            if (user == null)
            {
                _logger.LogWarning("Utente non trovato con Email {EmailAddress}.", emailAddress);
                return NotFound("Utente non trovato.");
            }

            return Ok(user);
        }


        // PUT: api/Users/5
        // Aggiorna i dettagli di un utente specificato dall'ID
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] User user)
        {
            if (id != user.UserId)
            {
                _logger.LogWarning("Tentativo di aggiornamento fallito per l'utente {Email}. ID non corrispondente.", user.EmailAddress);
                return BadRequest("ID non corrispondente.");
            }

            var existingUser = await _authContext.Users.FindAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning("Tentativo di aggiornamento fallito: utente non trovato con email {Email}.", user.EmailAddress);
                return NotFound("Utente non trovato.");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == id);
            if (customer == null)
            {
                _logger.LogWarning("Tentativo di aggiornamento fallito: cliente non trovato per l'utente {Email}.", user.EmailAddress);
                return NotFound("Cliente non trovato.");
            }

            // Aggiorna i dati dell'utente
            existingUser.FirstName = user.FirstName;
            existingUser.MiddleName = user.MiddleName;
            existingUser.LastName = user.LastName;
            existingUser.Suffix = user.Suffix;
            existingUser.EmailAddress = user.EmailAddress;
            existingUser.Phone = user.Phone;
            existingUser.Role = user.Role;

            // Aggiorna anche il cliente con i dati dell'utente
            customer.Title = user.Title;
            customer.FirstName = user.FirstName;
            customer.MiddleName = user.MiddleName;
            customer.LastName = user.LastName;
            customer.Suffix = user.Suffix;
            customer.EmailAddress = user.EmailAddress;
            customer.Phone = user.Phone;

            // Marca l'utente e il cliente come modificati
            _authContext.Entry(existingUser).State = EntityState.Modified;
            _context.Entry(customer).State = EntityState.Modified;

            await _authContext.SaveChangesAsync();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Aggiornato con successo utente {Email}.", user.EmailAddress);
            return NoContent();
        }


        // Metodo privato per verificare se l'utente esiste
        private bool UserExistsU(int id)
        {
            return _authContext.Users.Any(e => e.UserId == id);
        }


        // PUT: api/Users/UpdatePass
        // Permette di aggiornare la password di un utente
        [HttpPut("UpdatePass")]
        public async Task<IActionResult> PutUpdatePass([FromBody] ChangePassRequest changePassRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authContext.Users.FindAsync(changePassRequest.Id);
            if (user == null)
            {
                _logger.LogWarning("Tentativo di aggiornamento password fallito: utente non trovato con ID {UserId}.", changePassRequest.Id);
                return NotFound("Utente non trovato.");
            }

            var passHashSalt = SaltEncrypt.SaltEncryptPass(changePassRequest.Password);
            user.PasswordHash = passHashSalt.Key;
            user.PasswordSalt = passHashSalt.Value;

            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();

            _logger.LogInformation("Password aggiornata per utente {Email}.", user.EmailAddress);
            return NoContent();
        }


        // POST: api/Users/Add
        // Aggiunge un nuovo utente
        [HttpPost("Add")]
        public async Task<ActionResult<User>> PostUser([FromBody] UserDto userDto)
        {
            // Verifica se nello stato del modello ci sono eventuali errori
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Controlla se esiste già un utente con la stessa email
            var existingUser = await _authContext.Users
                .FirstOrDefaultAsync(u => u.EmailAddress == userDto.EmailAddress);

            if (existingUser != null)
            {
                _logger.LogWarning("Tentativo di creazione fallito: un utente con la mail {Email} esiste già.", userDto.EmailAddress);
                // 2. Restituisce un 409 Conflict se la mail esiste già
                return Conflict("Esiste già un utente con questa email.");
            }

            // 3. Crea l'hash e il salt della password
            var passHashSalt = SaltEncrypt.SaltEncryptPass(userDto.Password);

            // 4. Crea l'oggetto User
            var user = new User()
            {
                FirstName = userDto.FirstName,
                MiddleName = userDto.MiddleName,
                LastName = userDto.LastName,
                Suffix = userDto.Suffix,
                EmailAddress = userDto.EmailAddress,
                Phone = userDto.Phone,
                PasswordHash = passHashSalt.Key,
                PasswordSalt = passHashSalt.Value,
                Role = userDto.Role,
                Rowguid = Guid.NewGuid(),
            };

            // 5. Aggiunge l'utente al database
            _authContext.Users.Add(user);
            await _authContext.SaveChangesAsync();

            // 6. Crea e salva un record in tabella Customers
            var customer = new Customer()
            {
                NameStyle = true,
                FirstName = userDto.FirstName,
                MiddleName = userDto.MiddleName,
                LastName = userDto.LastName,
                Suffix = userDto.Suffix,
                EmailAddress = "",
                Phone = "",
                PasswordHash = "",
                PasswordSalt = "",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow,
                UserID = user.UserId
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Creato nuovo utente {Email} e relativo cliente.", user.EmailAddress);
            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }
        // DELETE: api/Users/5
        // Elimina un utente specificato dall'ID
        [HttpDelete("Delete/{id}")]
         public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Tentativo di eliminazione fallito: utente non trovato con ID {UserId}.", id);
                return NotFound("Utente non trovato.");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _logger.LogInformation("Rimosso cliente associato per l'utente {Email}.", user.EmailAddress);
            }

            _authContext.Users.Remove(user);
            await _authContext.SaveChangesAsync();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Eliminato con successo utente {Email}.", user.EmailAddress);
            return NoContent();
        }
    }
}
