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
using Google.Apis.Auth;
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
        private const string CLIENT_ID = "467980910008-a1c9tm4dg1omhet8vckq8t77nr42feea.apps.googleusercontent.com"; // Inserisci il tuo CLIENT_ID di Google

        // Costruttore che inietta il contesto del database
        public UsersController(AdventureWorksLt2019usersInfoContext authContext, AdventureWorksLt2019Context context)
        {
            _authContext = authContext;
            _context = context;

        }

        // GET: api/Users
        // Restituisce una lista di tutti gli utenti
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _authContext.Users.ToListAsync();
        }

        // GET: api/Users/5
        // Restituisce i dettagli di un singolo utente tramite ID
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(); // Se l'utente non esiste, restituisce 404
            }

            return user;
        }

        // GET: api/Users/UserPass/{password}/{id}
        // Verifica se la password fornita corrisponde all'utente con ID specificato
        [HttpGet("UserPass/{password}/{id}")]
        public async Task<ActionResult<bool>> GetUserPass(int id, string password)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (SaltEncrypt.SaltDecryptPass(password, user.PasswordSalt) == user.PasswordHash)
            {
                return true; // La password è corretta
            }
            return false; // La password non è corretta
        }

        // GET: api/Users/AuthUser/{emailAddress}
        // Restituisce l'utente corrispondente all'indirizzo email
        [HttpGet("AuthUser/{emailAddress}")]
        public async Task<ActionResult<User>> GetUser(string emailAddress)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.EmailAddress.Equals(emailAddress));

            if (user == null)
            {
                return NotFound(); // Se l'utente non esiste, restituisce 404
            }

            return user;
        }

        // PUT: api/Users/5
        // Aggiorna i dettagli di un utente specificato dall'ID
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest(); // Se l'ID non corrisponde, restituisce un errore
            }

            _authContext.Entry(user).State = EntityState.Modified;

            try
            {
                await _authContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(); // Se l'utente non esiste, restituisce 404
                }
                else
                {
                    throw; // Rilancia l'errore se c'è un problema di concorrenza
                }
            }

            return NoContent(); // Restituisce una risposta vuota di successo
        }

        // PUT: api/Users/UpdatePass
        // Permette di aggiornare la password di un utente
        [HttpPut("UpdatePass")]
        public async Task<IActionResult> PutUpdatePass([FromBody] ChangePassRequest changePassRequest)
        {
            var user = await _authContext.Users.FindAsync(changePassRequest.Id);
            KeyValuePair<string, string> passHashSalt = SaltEncrypt.SaltEncryptPass(changePassRequest.Password);

            if (user == null)
            {
                return BadRequest(); // Se l'utente non esiste, restituisce un errore
            }

            user.PasswordHash = passHashSalt.Key; // Imposta il nuovo hash della password
            user.PasswordSalt = passHashSalt.Value; // Imposta il nuovo salt della password
            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();

            return NoContent(); // Restituisce una risposta vuota di successo
        }

        // POST: api/Users/Add
        // Aggiunge un nuovo utente
        [HttpPost("Add")]
        public async Task<ActionResult<User>> PostUser(UserDto userDto)
        {
            KeyValuePair<string, string> passHashSalt = SaltEncrypt.SaltEncryptPass(userDto.Password);

            var user = new User()
            {
                FirstName = userDto.FirstName,
                MiddleName = userDto.MiddleName,
                LastName = userDto.LastName,
                Suffix = userDto.Suffix,
                EmailAddress = userDto.EmailAddress,
                Phone = userDto.Phone,
                PasswordHash = passHashSalt.Key, // Imposta l'hash della password
                PasswordSalt = passHashSalt.Value, // Imposta il salt della password
                Role = userDto.Role,
                Rowguid = Guid.NewGuid(), // Crea un nuovo identificatore GUID
            };
            _authContext.Users.Add(user); // Aggiunge il nuovo utente al contesto
            await _authContext.SaveChangesAsync();
            var customer = new Customer()
            {
                NameStyle = true, // Impostazione del formato del nome, ad esempio western style
                FirstName = userDto.FirstName,
                MiddleName = userDto.MiddleName,
                LastName = userDto.LastName,
                Suffix = userDto.Suffix,
                EmailAddress = userDto.EmailAddress,
                Phone = userDto.Phone,
                PasswordHash = passHashSalt.Key, // Usa lo stesso hash della password
                PasswordSalt = passHashSalt.Value, // Usa lo stesso salt della password
                Rowguid = Guid.NewGuid(), // GUID unico per il customer
                ModifiedDate = DateTime.UtcNow, // Data di ultima modifica
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();


            return CreatedAtAction("GetUser", new { id = user.UserId }, user); // Restituisce l'utente creato con il codice 201
        }

        // DELETE: api/Users/5
        // Elimina un utente specificato dall'ID
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(); // Se l'utente non esiste, restituisce 404
            }

            _authContext.Users.Remove(user); // Rimuove l'utente dal contesto
            await _authContext.SaveChangesAsync();

            return NoContent(); // Restituisce una risposta vuota di successo
        }

        // Metodo privato per verificare se l'utente esiste nel database
        private bool UserExists(int id)
        {
            return _authContext.Users.Any(e => e.UserId == id);
        }


        [HttpPost("registerGoogleUser")]
        public async Task<IActionResult> RegisterUser([FromBody] string idTokenString)
        {
            try
            {
                // Verifica il token ID di Google
                var payload = await GoogleJsonWebSignature.ValidateAsync(idTokenString, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { CLIENT_ID }
                });

                if (payload != null)
                {
                    // Estrai le informazioni dell'utente dal payload
                    string email = payload.Email;
                    string firstName = payload.GivenName;
                    string lastName = payload.FamilyName;

                    // Controlla se l'utente esiste già nel database
                    var existingUser = await _authContext.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
                    if (existingUser != null)
                    {
                        return BadRequest("User already exists.");
                    }

                    // Crea una password hash generata casualmente per l'utente
                    string password = GenerateRandomPassword(); // Puoi generare una password casuale
                    var (passwordHash, passwordSalt) = CreatePasswordHash(password);

                    // Crea un nuovo utente
                    var newUser = new User
                    {
                        FirstName = firstName,
                        MiddleName = "",
                        LastName = lastName,
                        Suffix = "",
                        EmailAddress = email,
                        Phone = "",
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        Role = "User", // Assegna il ruolo dell'utente
                        Rowguid = Guid.NewGuid()// Puoi generare un GUID per l'utente
                    };

                    // Aggiungi il nuovo utente al database
                    _authContext.Users.Add(newUser);
                    await _authContext.SaveChangesAsync();

                    return Ok("User registered successfully.");
                }
                else
                {
                    return Unauthorized("Invalid ID token.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Metodo per generare una password casuale
        private string GenerateRandomPassword()
        {
            var random = new Random();
            var password = Path.GetRandomFileName().Replace(".", ""); // Usa un nome di file come password casuale
            return password;
        }

        // Metodo per creare un hash della password
        private (string passwordHash, string passwordSalt) CreatePasswordHash(string password)
        {
            // Usare HMACSHA512 per generare un hash
            using (var hmac = new HMACSHA512())
            {
                // La salt viene estratta dalla chiave segreta di HMAC (10 byte)
                var passwordSalt = hmac.Key.Take(10).ToArray();  // Limita la salt a 10 byte

                // Crea l'hash della password
                var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Converti la salt in Base64 e limita la lunghezza a 10 caratteri
                var passwordSaltBase64 = Convert.ToBase64String(passwordSalt);

                if (passwordSaltBase64.Length > 10)
                {
                    passwordSaltBase64 = passwordSaltBase64.Substring(0, 10);  // Limita la salt a 10 caratteri
                }

                // Limita l'hash a 128 bit (16 byte) e convertilo in Base64
                var passwordHashBase64 = Convert.ToBase64String(passwordHash.Take(16).ToArray());

                // Restituisci l'hash e la salt
                return (passwordHashBase64, passwordSaltBase64);
            }
        }
    }
}
