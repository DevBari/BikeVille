using BikeVille.Auth;
using BikeVille.Auth.AuthContext;
using BikeVille.CriptingDecripting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthJwt.Auth
{
    [Route("[controller]")]
    [ApiController]
    public class LoginJwtController : ControllerBase
    {
        //classe setting jwt
        private JwtSettings _jwtSettings;
        private AdventureWorksLt2019usersInfoContext _context;

        //dependency injection dei settings
        public LoginJwtController(JwtSettings jwtSettings,AdventureWorksLt2019usersInfoContext context)
        {
            _jwtSettings = jwtSettings;
            _context = context;
        }

        private string GenerateJwtToken(string email,string role)
        {
            var secretKey=_jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key=Encoding.ASCII.GetBytes(secretKey);


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
           
            return tokenHandler.WriteToken(token);
        }


        [HttpPost("Login")]
        public IActionResult Login([FromBody] Credentials credentials)
        {
            if(_context.Users.FirstOrDefault(u => u.EmailAddress.Equals(credentials.Email)) != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.EmailAddress.Equals(credentials.Email));
                var passHash=SaltEncrypt.SaltDecryptPass(credentials.Password, user.PasswordSalt);
                if(passHash.Equals(user.PasswordHash))
                {
                    string token = GenerateJwtToken(credentials.Email, user.Role.Trim());
                    return Ok(new { token });
                }
                else
                {

                    return Unauthorized();

                }


            }
            else
            {
                return Unauthorized();
            }
           
        }



       
    }
}
