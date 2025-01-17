using Microsoft.AspNetCore.Http; //Fornisce gli oggetti necessari per gestire le richieste HTTP.
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BikeVille.Middleware
{
/*
La classe ErrorHandlingMiddleware gestisce gli errori non 
gestiti all'interno della pipeline delle richieste di ASP.NET Core.
 
*/
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next; //che rappresenta il middleware successivo nella pipeline.
        private readonly ILogger<ErrorHandlingMiddleware> _logger;//Istanza di ILogger<ErrorHandlingMiddleware> utilizzata per registrare gli errori.
    /*Il costruttore accetta due parametri:

    next: Il middleware successivo da eseguire.
    logger: L'istanza di logger per registrare eventuali eccezioni*/
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
/*
Il metodo InvokeAsync è il metodo principale della classe ErrorHandlingMiddleware.
Esegue il middleware successivo nella pipeline utilizzando _next(context).
Avvolge l'esecuzione in un blocco try-catch 
per intercettare eventuali eccezioni non gestite.
In caso di eccezione, registra l'errore usando 
_logger.LogError e invoca HandleExceptionAsync per gestire la risposta
*/
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }
        /*
        Il metodo HandleExceptionAsync gestisce le eccezioni non gestite.
        Imposta il tipo di contenuto della risposta a application/json.
        Imposta il codice di stato HTTP a 500 Internal Server Error.
        Crea un oggetto di risposta con un messaggio di errore generico.
        Scrive l'oggetto di risposta nel corpo della risposta in formato JSON.
        */
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = new { message = "An error occurred while processing your request." };
            return context.Response.WriteAsJsonAsync(result);
        }
    }
}