using BikeVille.Entity;
using BikeVille.Entity.EntityContext;

namespace BikeVille.Logging
{
    /// <summary>
    /// La classe <c>DatabaseLogger</c> implementa l'interfaccia <c>ILogger</c> per registrare i messaggi di log direttamente nel database.
    /// </summary>
    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<LogLevel, bool> _filter;
        private readonly AdventureWorksLt2019Context _context;
        /*
        Nel costruttore viene passato (iniettato) il nome della categoria(categoryName),
         la funzione di filtro dei livelli di log (filter )e il contesto del database(context).
        */

        public DatabaseLogger(string categoryName,
                             Func<LogLevel, bool> filter,
                             AdventureWorksLt2019Context context)
        {
            _categoryName = categoryName;
            _filter = filter;
            _context = context;
        }
        /// <summary>
        /// Il metodo BeginScope<TState> fa parte dell'interfaccia ILogger di Microsoft.Extensions.
        ///Logging e viene utilizzato per creare uno scope di logging. Uno scope consente di raggruppare 
        ///una serie di messaggi di log all'interno di un contesto specifico, migliorando la tracciabilità 
        ///e la leggibilità dei log.
        /// </summary>

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

            // Estrae l'email dallo state
            var email = ExtractEmailFromState(state);
            // Estrazione del numero di riga dall'eccezione, se presente
            int errorLine = 0;
            if (exception != null && exception.StackTrace != null)
            {
                var stackTrace = new System.Diagnostics.StackTrace(exception, true);
                var frame = stackTrace.GetFrame(0);
                if (frame != null)
                {
                    errorLine = frame.GetFileLineNumber();
                }

            }

            // Crea l'entità ErrorLog 
            var errorLog = new ErrorLog
            {
                ErrorTime = DateTime.UtcNow,         //viene associato la data e l'ora del messaggio di log
                UserName = email,                   //viene associata l'email dell'utente al messaggio di log
                ErrorNumber = (int)logLevel,       //viene associato il livello di log al messaggio di log
                ErrorSeverity = (int)logLevel,    //viene associato il livello di log al messaggio di log
                ErrorState = eventId.Id,         //viene associato l'ID dell'evento al messaggio di log
                ErrorProcedure = _categoryName, //viene associato il nome della categoria al messaggio di log
                ErrorLine = errorLine,         //viene associato il numero di riga al messaggio di log
                ErrorMessage = message + (exception != null ? $" EX: {exception.Message}" : "")  // viene associato il messaggio di log e l'eccezione, se presente
            };

            _context.ErrorLogs.Add(errorLog);
            _context.SaveChanges();
        }

        private string ExtractEmailFromState<TState>(TState state)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
            {
                var emailPair = keyValuePairs.FirstOrDefault(kv => kv.Key == "Email");  // Cerca l'email nel state
                if (!emailPair.Equals(default(KeyValuePair<string, object>)) && emailPair.Value != null) // Se l'email è trovata
                {
                    return emailPair.Value.ToString() ?? "System";
                }
            }
            return "System"; // Valore predefinito se l'email non è trovata
        }
    }
}