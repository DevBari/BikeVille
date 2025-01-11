
using AuthJwt.Mail;
using BikeVille.Auth.AuthContext;
using BikeVille.Entity.EntityContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BikeVille.Admin
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AdventureWorksLt2019usersInfoContext _context;
        private readonly AdventureWorksLt2019Context _contextC;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdventureWorksLt2019usersInfoContext context, ILogger<AdminController> logger, AdventureWorksLt2019Context contextC)
        {
            _context = context;
            _logger = logger;
            _contextC = contextC;
        }

        [HttpGet("toBeAdmin/{email}")]
        // [Authorize(Roles = "ADMIN")] // Rimuovi temporaneamente per test
        public async Task<IActionResult> toBeAdmin(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is null or empty");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            var customer = await _contextC.Customers.FirstOrDefaultAsync(c => c.EmailAddress == email);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            try
            {
                user.Role = "ADMIN";
                _context.Entry(user).State = EntityState.Modified;

                _logger.LogInformation($"Updating user: {user.UserId}, Role: {user.Role}");

                var affectedRows = await _context.SaveChangesAsync();
                if (affectedRows == 0)
                {
                    _logger.LogWarning("No rows were affected when updating the user role.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update user role.");
                }

                if (customer != null)
                {
                    _contextC.Customers.Remove(customer);
                    await _contextC.SaveChangesAsync();
                }

                var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
                _logger.LogInformation($"Updated user: {updatedUser.UserId}, Role: {updatedUser.Role}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }

            return Ok($"User {email} is now an admin");
        }
    }
}