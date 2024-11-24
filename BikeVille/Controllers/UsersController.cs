using BikeVille.Auth.AuthContext; // Assicurati di importare il namespace corretto
using BikeVille.Entity.EntityContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BikeVille.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly AdventureWorksLt2019usersInfoContext _authContext;

        public UsersController(AdventureWorksLt2019usersInfoContext authContext)
        {
            _authContext = authContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authContext.Users.ToListAsync();
            return Ok(users);
        }
    }
}
