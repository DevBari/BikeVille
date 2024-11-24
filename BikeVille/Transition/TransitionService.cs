using BikeVille.Auth.AuthContext;
using BikeVille.Entity.EntityContext;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BikeVille.Auth;

namespace BikeVille.Transition
{
    public class TransitionService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransitionService> _logger;
        private readonly IConfiguration _configuration;

        public TransitionService(IServiceScopeFactory scopeFactory, ILogger<TransitionService> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Transition service starting...");

            // Crea un nuovo scope per risolvere i DbContext come servizi scoped
            using (var scope = _scopeFactory.CreateScope())
            {
                // Risolvi i DbContext dal nuovo scope
                var authContext = scope.ServiceProvider.GetRequiredService<AdventureWorksLt2019usersInfoContext>();
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorksLt2019Context>();
                if (_configuration.GetValue<string>("Transition") == "False")
                {
                  // Avvia la transizione dei dati
                  await TransitionUserCreation(authContext, context);
                   
                  await context.Database.ExecuteSqlRawAsync(
                       "UPDATE [SalesLT].[Customer] SET EmailAddress = NULL, Phone = NULL, PasswordHash ='', PasswordSalt ='' WHERE CustomerID IS NOT NULL"
                  );
                    
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Transition service stopping...");
            return Task.CompletedTask;
        }

        private async Task TransitionUserCreation(AdventureWorksLt2019usersInfoContext authContext, AdventureWorksLt2019Context context)
        {
            foreach (var c in context.Customers)
            {
                var user = new User()
                {
                    Title = c.Title,
                    FirstName = c.FirstName,
                    MiddleName = c.MiddleName,
                    LastName = c.LastName,
                    Suffix = c.Suffix,
                    EmailAddress = c.EmailAddress,
                    Phone = c.Phone,
                    PasswordHash = c.PasswordHash,
                    PasswordSalt = c.PasswordSalt,
                    Role = "CUSTOMER",
                    Rowguid = c.Rowguid,
                };

                authContext.Users.Add(user);
 
                await authContext.SaveChangesAsync();
                
            }
            


            _logger.LogInformation("Transition completed.");
        }

        

    }
}
