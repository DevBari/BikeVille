using BikeVille.Entity.EntityContext;

namespace BikeVille.Logging
{
    /*
    In questa classe viene implementata l'interfaccia ILoggerProvider che si occupa di creare istanze 
    di DatabaseLogger.Questo  provider permette l’integrazione di DatabaseLogger 
    nel sistema di logging di ASP.NET Core

    */
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly Func<LogLevel, bool> _filter; //Filtro per i log
        private readonly AdventureWorksLt2019Context _context; //Contesto del database

        /*
        Il costruttore accetta una funzione di filtro e 
        il contesto del database, assegnando ai campi privati 
        corrispondenti. Questo permette di configurare 
        quali livelli di log verranno registrati e di 
        utilizzare il contesto del database per memorizzare i log.
        */
        public DatabaseLoggerProvider(Func<LogLevel, bool> filter,
                                      AdventureWorksLt2019Context context)
        {
            _filter = filter;//Una funzione che determina i livelli di log da filtrare.
            _context = context;//Contesto del database
        }
        /*
        Questo metodo crea e ritorna una nuova istanza di DatabaseLogger,
        passando il nome della categoria, il filtro e il contesto del database. Ogni volta che viene richiesto un 
        logger per una determinata categoria, viene 
        fornito un DatabaseLogger configurato appropriatamente
        */
        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(categoryName, _filter, _context);
        }
        /*Implementa la logica di pulizia delle risorse utilizzate dal provider. 
        È importante per rilasciare eventuali risorse non gestite
         quando il provider non è più necessario
        */
        public void Dispose()
        {
            // Eventuali operazioni di pulizia
        }
    }
}