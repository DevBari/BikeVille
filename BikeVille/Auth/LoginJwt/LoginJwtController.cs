using BikeVille.Auth.AuthContext;
using BikeVille.CriptingDecripting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Per ulteriori informazioni su come abilitare le API Web per progetti vuoti, visita https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthJwt.Auth
{
    [Route("[controller]")]
    [ApiController]
    public class LoginJwtController : ControllerBase
    {
        // Classe di configurazione del JWT
        private JwtSettings _jwtSettings;
        private AdventureWorksLt2019usersInfoContext _context;

        // Iniezione delle dipendenze per i settings e il contesto del database
        public LoginJwtController(JwtSettings jwtSettings, AdventureWorksLt2019usersInfoContext context)
        {
            _jwtSettings = jwtSettings;
            _context = context;
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
        public IActionResult Login([FromBody] Credentials credentials)
        {
            // Controlla se l'utente esiste nel database
            var user = _context.Users.FirstOrDefault(u => u.EmailAddress.Equals(credentials.Email));

            if (user != null)
            {
                // Verifica se la password inserita corrisponde a quella salvata
                var passHash = SaltEncrypt.SaltDecryptPass(credentials.Password, user.PasswordSalt);
                if (passHash.Equals(user.PasswordHash))
                {
                    // Genera e restituisce il token JWT se la password è corretta
                    string token = GenerateJwtToken(credentials.Email, user.Role.Trim());
                    return Ok(new { token });
                }
                else
                {
                    // Restituisce un errore di autenticazione se la password è errata
                    return Unauthorized();
                }
            }
            else
            {
                // Restituisce un errore di autenticazione se l'utente non esiste
                return Unauthorized();
            }
        }
    }
}
