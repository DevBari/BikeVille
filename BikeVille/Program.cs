using LoginJwt.jwtSettings;
using BikeVille.Auth.AuthContext;
using BikeVille.Entity.EntityContext;
using BikeVille.Transition;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using BikeVille.Logging;
using BikeVille.Middleware; // Importa il namespace del middleware

namespace BikeVille
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            builder.Services.AddControllers().AddJsonOptions(o=>o.JsonSerializerOptions.ReferenceHandler=ReferenceHandler.Preserve);
            //Service Transition
            builder.Services.AddHostedService<TransitionService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AdventureWorksLt2019Context>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("BikeVilleDb")));
            //auth
            builder.Services.AddDbContext<AdventureWorksLt2019usersInfoContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("BikeVilleUsersDb")));
    
            //cors
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });



            //JwtConfiguration
            JwtSettings jwtSettings = new();
            builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
            _ = builder.Services.AddSingleton(jwtSettings);

            //AuthJwt
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero,

                };
            });
            builder.Services.Configure<GoogleAuthSettings>(options =>
                {
                    options.GoogleClientId = "467980910008-a1c9tm4dg1omhet8vckq8t77nr42feea.apps.googleusercontent.com";
                    options.Issuer = "https://accounts.google.com";
                    options.Audience = "467980910008-a1c9tm4dg1omhet8vckq8t77nr42feea.apps.googleusercontent.com";
                });



            //Role
            builder.Services.AddAuthorizationBuilder().AddPolicy("AdminPolicy", policy => policy.RequireRole("ADMIN"));


            // (D) Costruisci un provider temporaneo per recuperare AdventureWorksLt2019Context
            // (il contesto sul quale hai implementato DatabaseLogger)
            var tempProvider = builder.Services.BuildServiceProvider();
            var dbContext = tempProvider.GetRequiredService<AdventureWorksLt2019Context>();

            // (E) Configura il logging personalizzato
            //     Se vuoi mantenere i log su console, NON chiamare ClearProviders().
            //     Se vuoi sostituire i provider predefiniti, puoi fare:
            //
            // builder.Logging.ClearProviders(); 
            // builder.Logging.AddConsole();
            //
            // Poi aggiungi il tuo provider DatabaseLogger:
            builder.Logging.AddDatabaseLogger(
                filter: logLevel => logLevel >= LogLevel.Warning,  // Filtra i log che vuoi inserire in DB
                context: dbContext
            );
            
            var app = builder.Build();

            //Cors
            app.UseCors("AllowAll");

           // Use the error handling middleware
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }



            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();




            app.MapControllers();

            app.Run();
        }
    }
}
