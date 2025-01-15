using Microsoft.Extensions.Logging;
using BikeVille.Auth.AuthContext; // o dove si trova il tuo AdventureWorksLt2019usersInfoContext
using BikeVille.Entity;           // o dove si trova la classe 'ErrorLog', se la usi
using System;
using BikeVille.Entity.EntityContext;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace BikeVille.Logging
{
    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<LogLevel, bool> _filter;
        private readonly AdventureWorksLt2019Context _context;
        
        public DatabaseLogger(string categoryName,
                              Func<LogLevel, bool> filter,
                              AdventureWorksLt2019Context context)
        {
            _categoryName = categoryName;
            _filter = filter;
            _context = context;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter == null || _filter(logLevel);
        }

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception? exception,
                                Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            
            // Estrai l'email dallo state
            var email = ExtractEmailFromState(state);
            // Crea l'entità ErrorLog (o la tua tabella personalizzata)
            // Assumendo che tu abbia una classe 'ErrorLog' nel tuo Entity/DbContext.
            var errorLog = new ErrorLog
            {
                ErrorTime = DateTime.UtcNow,
                UserName = email,              // O recupera info in modo diverso
                ErrorNumber = (int)logLevel,
                ErrorSeverity = (int)logLevel,
                ErrorState = eventId.Id,
                ErrorProcedure = _categoryName,
                ErrorLine = 0,
                ErrorMessage = message + (exception != null ? $" EX: {exception.Message}" : "")
            };

            _context.ErrorLogs.Add(errorLog);
            _context.SaveChanges();
        }

        private string ExtractEmailFromState<TState>(TState state)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
            {
                var emailPair = keyValuePairs.FirstOrDefault(kv => kv.Key == "Email");
                if (!emailPair.Equals(default(KeyValuePair<string, object>)) && emailPair.Value != null)
                {
                    return emailPair.Value.ToString() ?? "System";
                }
            }
            return "System"; // Valore predefinito se l'email non è trovata
        }
    }
}