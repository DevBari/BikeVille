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

        // Costruttore che inizializza il contesto del database
        public CustomersController(AdventureWorksLt2019Context context, AdventureWorksLt2019usersInfoContext authContext)
        {
            _context = context;
            _authContext = authContext;

        }

        // GET: api/Customers
        // Recupera tutti i clienti con i relativi ordini e indirizzi
        
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers
                .Include(c => c.SalesOrderHeaders)  // Include gli ordini di vendita del cliente
                .ThenInclude(soh => soh.SalesOrderDetails)  // Include i dettagli degli ordini di vendita
                .Include(c => c.CustomerAddresses)  // Include gli indirizzi del cliente
                .ThenInclude(ca => ca.Address)  // Include gli indirizzi fisici
                .ToListAsync();
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

            // Se il cliente non esiste, restituisce un errore 404 (Non trovato)
            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // PUT: api/Customers/5
        // Permette di aggiornare le informazioni di un cliente
        // Per evitare attacchi di overposting, è necessario specificare solo le proprietà consentite
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            // Verifica se l'ID del cliente passato nella richiesta corrisponde a quello dell'oggetto
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;  // Imposta lo stato dell'entità come modificato

            try
            {
                await _context.SaveChangesAsync();  // Salva le modifiche nel database
            }
            catch (DbUpdateConcurrencyException)
            {
                // Gestisce l'errore se si verifica un conflitto durante l'aggiornamento
                if (!CustomerExists(id))
                {
                    return NotFound();  // Se il cliente non esiste più, restituisce 404
                }
                else
                {
                    throw;
                }
            }

            return NoContent();  // Restituisce un 204 No Content se l'aggiornamento è riuscito
        }
        [HttpDelete("Delete/{id}")]
        
public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.CustomerAddresses) // Include gli indirizzi associati al cliente
                .FirstOrDefaultAsync(c => c.UserID == id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (customer != null)
                {
                    // Elimina gli indirizzi del cliente, se presenti
                    if (customer.CustomerAddresses != null && customer.CustomerAddresses.Any())
                    {
                        _context.CustomerAddresses.RemoveRange(customer.CustomerAddresses);
                    }

                    // Elimina il cliente
                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                }

                // Elimina l'utente
                _authContext.Users.Remove(user);
                await _authContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log dell'errore (se hai configurato un logger)
                Console.Error.WriteLine($"Error while deleting user with ID {id}: {ex.Message}");
                throw;
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
