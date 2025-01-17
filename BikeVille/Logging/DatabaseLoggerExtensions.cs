using BikeVille.Entity.EntityContext;



namespace BikeVille.Logging
{
    /*

     Queste classi(DatabaseLogger,DatabaseLoggerExtensions,DatabaseLoggerProvider) creano un sistema di logging
     personalizzato che intercetta i messaggi di 
     log (ILogger) e li salva nel database tramite la 
     tua tabella (ad esempio, ErrorLog).

    */
    public static class DatabaseLoggerExtensions
    {
        public static ILoggingBuilder AddDatabaseLogger(this ILoggingBuilder builder,
                                                        Func<LogLevel, bool> filter,
                                                        AdventureWorksLt2019Context context)
        {
            builder.AddProvider(new DatabaseLoggerProvider(filter, context));
            return builder;
        }
    }
}