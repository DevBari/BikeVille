using BikeVille.Auth.AuthContext;
using BikeVille.CriptingDecripting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using LoginJwt.jwtSettings;
// Per ulteriori informazioni su come abilitare le API Web per progetti vuoti, visita https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoginJwt.Auth
{
    [Route("[controller]")]
    [ApiController]
    public class LoginJwtController : ControllerBase
    {
        // Classe di configurazione del JWT
        private JwtSettings _jwtSettings;
        private AdventureWorksLt2019usersInfoContext _context;
       private readonly ILogger<LoginJwtController> _logger;

        // Iniezione delle dipendenze per i settings e il contesto del database
        public LoginJwtController(JwtSettings jwtSettings, AdventureWorksLt2019usersInfoContext context,  ILogger<LoginJwtController> logger)
        {
            _jwtSettings = jwtSettings;
            _context = context;
            _logger = logger;
        }

        // Metodo per generare un token JWT
        private string GenerateJwtToken(string email, string role)
        {
            var secretKey = _jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Definizione del token con i relativi claim
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Restituisce il token come stringa
            return tokenHandler.WriteToken(token);
        }

        // Endpoint POST per il login
        [HttpPost("Login")]
       public async Task<IActionResult> Login([FromBody] Credentials credentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailAddress.Equals(credentials.Email));

            if (user == null)
            {
                _logger.LogWarning("Tentativo di accesso fallito: utente non trovato con email {Email}.", credentials.Email);
                return Unauthorized("Utente non trovato.");
            }

            var passHash = SaltEncrypt.SaltDecryptPass(credentials.Password, user.PasswordSalt);
            if (!passHash.Equals(user.PasswordHash))
            {
                _logger.LogWarning("Tentativo di accesso fallito per l'utente {Email}: password errata.", credentials.Email);
                return Unauthorized("Password errata.");
            }

            var token = GenerateJwtToken(credentials.Email, user.Role.Trim());
            _logger.LogInformation("Generato token JWT per l'utente {Email}.", credentials.Email);
            return Ok(new { token });
        }
    }
}

