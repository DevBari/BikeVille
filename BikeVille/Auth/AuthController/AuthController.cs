using BikeVille.Auth.AuthContext;
using BikeVille.Entity.EntityContext;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AdventureWorksLt2019usersInfoContext _context;
    private const string CLIENT_ID = "467980910008-a1c9tm4dg1omhet8vckq8t77nr42feea.apps.googleusercontent.com"; // Inserisci il tuo CLIENT_ID di Google

    public AuthController(AdventureWorksLt2019usersInfoContext context)
    {
        _context = context;
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
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
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
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

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
