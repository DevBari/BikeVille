using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Entity;
using BikeVille.Entity.EntityContext;
using BikeVille.Auth.AuthContext;

namespace BikeVille.Entity.CustomerControllers
{
    // Definizione del controller per la gestione dei clienti
    [Route("[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly AdventureWorksLt2019usersInfoContext _authContext;
        private readonly ILogger<CustomersController> _logger;

        // Costruttore che inizializza il contesto del database
        public CustomersController(
            AdventureWorksLt2019Context context,
            AdventureWorksLt2019usersInfoContext authContext,
            ILogger<CustomersController> logger) // Iniezione del logger
        {
            _context = context;
            _authContext = authContext;
            _logger = logger;
        }


        // GET: api/Customers
        // Recupera tutti i clienti con i relativi ordini e indirizzi
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers
                   .Include(c => c.SalesOrderHeaders)
                       .ThenInclude(soh => soh.SalesOrderDetails)
                   .Include(c => c.CustomerAddresses)
                       .ThenInclude(ca => ca.Address)
                   .ToListAsync();

            _logger.LogInformation("Recuperati {Count} clienti.", customers.Count);
            return Ok(customers);
        }


        // GET: api/Customers/5
        // Recupera un cliente specifico tramite il suo ID, includendo gli ordini e gli indirizzi
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.SalesOrderHeaders)
                    .ThenInclude(soh => soh.SalesOrderDetails)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                _logger.LogWarning("Cliente non trovato con ID {CustomerId}.", id);
                return NotFound("Cliente non trovato.");
            }

            _logger.LogInformation("Recuperato cliente con ID {CustomerId}.", id);
            return Ok(customer);
        }

        // PUT: api/Customers/5
        // Permette di aggiornare le informazioni di un cliente
        // Per evitare attacchi di overposting, è necessario specificare solo le proprietà consentite
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                _logger.LogWarning("Tentativo di aggiornamento fallito per cliente ID {CustomerId}. ID non corrispondente.", id);
                return BadRequest("ID non corrispondente.");
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cliente ID {CustomerId} aggiornato con successo.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CustomerExists(id))
                {
                    _logger.LogWarning("Cliente non trovato durante l'aggiornamento con ID {CustomerId}.", id);
                    return NotFound("Cliente non trovato.");
                }
                else
                {
                    _logger.LogError(ex, "Errore di concorrenza durante l'aggiornamento del cliente ID {CustomerId}.", id);
                    throw;
                }
            }

            return NoContent();
        }
        
        // DELETE: api/Customers/Delete/5
        /// <summary>
        /// Elimina un utente specifico e il cliente associato, se non ha ordini.
        /// </summary>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Tentativo di eliminazione fallito: utente non trovato con ID {UserId}.", id);
                return NotFound("Utente non trovato.");
            }

            var customer = await _context.Customers
                .Include(c => c.CustomerAddresses)
                .Include(c => c.SalesOrderHeaders) // Include gli ordini associati al cliente
                .FirstOrDefaultAsync(c => c.UserID == id);

            if (customer != null && customer.SalesOrderHeaders.Any())
            {
                _logger.LogWarning("Tentativo di eliminazione fallito: il cliente ID {CustomerId} ha ordini associati.", customer.CustomerId);
                return BadRequest("Impossibile eliminare l'utente perché ha ordini associati.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (customer != null)
                {
                    // Elimina gli indirizzi del cliente, se presenti
                    if (customer.CustomerAddresses != null && customer.CustomerAddresses.Any())
                    {
                        _context.CustomerAddresses.RemoveRange(customer.CustomerAddresses);
                        _logger.LogInformation("Rimossi indirizzi associati per il cliente ID {CustomerId}.", customer.CustomerId);
                    }

                    // Elimina il cliente
                    _context.Customers.Remove(customer);
                    _logger.LogInformation("Cliente associato all'utente {EmailAddress} rimosso.", user.EmailAddress);
                    await _context.SaveChangesAsync();
                }

                // Elimina l'utente
                _authContext.Users.Remove(user);
                await _authContext.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Utente {EmailAddress} eliminato con successo.", user.EmailAddress);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Errore durante l'eliminazione dell'utente con ID {UserId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante l'eliminazione dell'utente.");
            }

            return NoContent();
        }
        
        
        // Metodo privato che verifica se un cliente con l'ID specificato esiste nel database
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
