using Microsoft.Extensions.Logging;
using BikeVille.Auth.AuthContext;
using BikeVille.Entity.EntityContext;

namespace BikeVille.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly Func<LogLevel, bool> _filter;
        private readonly AdventureWorksLt2019Context _context;

        public DatabaseLoggerProvider(Func<LogLevel, bool> filter,
                                      AdventureWorksLt2019Context context)
        {
            _filter = filter;
            _context = context;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(categoryName, _filter, _context);
        }

        public void Dispose()
        {
            // Eventuali operazioni di pulizia
        }
    }
}