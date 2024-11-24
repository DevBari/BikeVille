using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Auth;
using BikeVille.Entity.EntityContext;
using BikeVille.Auth.AuthContext;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using BikeVille.CriptingDecripting;

namespace BikeVille.Auth.AuthController
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AdventureWorksLt2019usersInfoContext _authContext;

        public UsersController(AdventureWorksLt2019usersInfoContext authContext)
        {
            _authContext = authContext;
        }

        // GET: api/Users
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Recupera gli utenti dal contesto e mappali nel DTO UserJ
            var users = await _authContext.Users.ToListAsync();
            var usersJ = users.Select(u => new User
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                EmailAddress = u.EmailAddress,
                Phone = u.Phone,
                Role = u.Role,
                Title = u.Title,
                Suffix = u.Suffix
            }).ToList();

            return Ok(usersJ);  // Restituisci gli utenti mappati come UserJ
        }

        // GET: api/Users/5
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Mappa l'utente alla versione DTO UserJ
            var userJ = new User
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailAddress = user.EmailAddress,
                Phone = user.Phone,
                Role = user.Role,
                Title = user.Title,
                Suffix = user.Suffix
            };

            return Ok(userJ);
        }

        // PUT: api/Users/5
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutUser(int id, User userJ)
        {
            if (id != userJ.UserId)
            {
                return BadRequest();
            }

            // Mappa il DTO UserJ all'entità User nel contesto
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Aggiorna le proprietà dell'utente
            user.FirstName = userJ.FirstName;
            user.LastName = userJ.LastName;
            user.EmailAddress = userJ.EmailAddress;
            user.Phone = userJ.Phone;
            user.Role = userJ.Role;
            user.Title = userJ.Title;
            user.Suffix = userJ.Suffix;

            _authContext.Entry(user).State = EntityState.Modified;

            try
            {
                await _authContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost("Add")]
        public async Task<ActionResult<User>> PostUser(UserDto userDto)
        {
            // Crea l'oggetto User utilizzando il DTO passato
            KeyValuePair<string, string> passHashSalt = SaltEncrypt.SaltEncryptPass(userDto.Password);

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
                Rowguid = Guid.NewGuid()
            };

            _authContext.Users.Add(user);
            await _authContext.SaveChangesAsync();

            // Restituisce l'utente creato come UserJ
            var userJ = new User
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailAddress = user.EmailAddress,
                Phone = user.Phone,
                Role = user.Role,
                Title = user.Title,
                Suffix = user.Suffix
            };

            return CreatedAtAction("GetUser", new { id = user.UserId }, userJ);
        }

        // DELETE: api/Users/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _authContext.Users.Remove(user);
            await _authContext.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _authContext.Users.Any(e => e.UserId == id);
        }
    }
}
