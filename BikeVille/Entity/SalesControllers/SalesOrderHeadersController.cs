using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Entity;
using BikeVille.Entity.EntityContext;
using BikeVille.Auth;
using BikeVille.Auth.AuthContext;

namespace BikeVille.Entity.SalesControllers
{


    [Route("[controller]")]
    [ApiController]
    public class SalesOrderHeadersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly AdventureWorksLt2019usersInfoContext _authContext;

        public SalesOrderHeadersController(AdventureWorksLt2019Context context,AdventureWorksLt2019usersInfoContext authContext)
        {
            _context = context;
            _authContext = authContext;
        }

        // GET: api/SalesOrderHeaders
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<SalesOrderHeader>>> GetSalesOrderHeaders()
        {
            return await _context.SalesOrderHeaders.ToListAsync();
        }

        // GET: api/SalesOrderHeaders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesOrderHeader>> GetSalesOrderHeader(int id)
        {
            var salesOrderHeader = await _context.SalesOrderHeaders.FindAsync(id);

            if (salesOrderHeader == null)
            {
                return NotFound();
            }

            return salesOrderHeader;
        }

        // PUT: api/SalesOrderHeaders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("Update/{id}")]
        //public async Task<IActionResult> PutSalesOrderHeader(int id, SalesOrderHeader salesOrderHeader)
        //{
        //    if (id != salesOrderHeader.SalesOrderId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(salesOrderHeader).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!SalesOrderHeaderExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/SalesOrderHeaders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Add")]
        public async Task<ActionResult<SalesOrderHeader>> PostSalesOrderHeader(SalesOrderHeaderDto salesOrderHeader)
        {


            // Funzione per generare il codice casuale
            static string GeneratePurchaseOrder()
            {
                Random random = new Random();

                // Genera 2 lettere maiuscole casuali
                char lettera1 = (char)random.Next('A', 'Z' + 1);
                char lettera2 = (char)random.Next('A', 'Z' + 1);

                // Genera un numero casuale a 5 cifre (tra 10000 e 99999)
                int numero1 = random.Next(10000, 100000);

                // Genera un numero casuale a 8 cifre (tra 10000000 e 99999999)
                long numero2 = random.Next(10000000, 100000000);

                // Combina le lettere e i numeri in un codice
                string codice = $"{lettera1}{lettera2}{numero1}{numero2}";
                return codice;
            }

            // Funzione per generare il codice casuale
            static string GenerateSalesOrderNumber()
            {
                Random random = new();
                // Genera 2 lettere maiuscole casuali
                char lettera1 = (char)random.Next('A', 'Z' + 1);
                char lettera2 = (char)random.Next('A', 'Z' + 1);

                // Genera 5 numeri casuali
                int numero = random.Next(10000, 100000); // Questo genera un numero tra 10000 e 99999

                // Combina le lettere e i numeri in un codice
                string codice = $"{lettera1}{lettera2}{numero}";
                return codice;
            }





            Customer customer = new(); 

            if(salesOrderHeader.user.Role == "CUSTOMER")
            {
                customer = await _context.Customers.FirstOrDefaultAsync(c=>c.Rowguid== salesOrderHeader.user.Rowguid);
                salesOrderHeader.SalesOrderHeader.CustomerId = customer.CustomerId;

            }
            else if(salesOrderHeader.user.Role =="USER")
            {
                User user = await _authContext.Users.FirstOrDefaultAsync(u => u.Rowguid == salesOrderHeader.user.Rowguid);
                user.Role = "CUSTOMER"; 

                var Addedcustomer =new Customer()
                {
                    NameStyle =false,
                    Title= salesOrderHeader.user.Title,
                    FirstName= salesOrderHeader.user.FirstName,
                    LastName= salesOrderHeader.user.LastName,
                    Suffix= salesOrderHeader.user.Suffix,
                    CompanyName= salesOrderHeader.CompanyName,
                    SalesPerson=null,
                    EmailAddress=null,
                    Phone=null,
                    PasswordHash="",
                    PasswordSalt="",
                    Rowguid= salesOrderHeader.user.Rowguid,
                    ModifiedDate=DateTime.Now,

                };


                _context.Customers.Add(Addedcustomer);
                await _context.SaveChangesAsync();
                await _authContext.SaveChangesAsync();

                customer = await _context.Customers.FirstOrDefaultAsync(c=>c.Rowguid== salesOrderHeader.user.Rowguid);
                salesOrderHeader.SalesOrderHeader.CustomerId = customer.CustomerId;
            }


            _context.SalesOrderHeaders.Add(salesOrderHeader.SalesOrderHeader);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSalesOrderHeader", new { id = salesOrderHeader.SalesOrderHeader.SalesOrderId }, salesOrderHeader);
        }

        // DELETE: api/SalesOrderHeaders/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteSalesOrderHeader(int id)
        //{
        //    var salesOrderHeader = await _context.SalesOrderHeaders.FindAsync(id);
        //    if (salesOrderHeader == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.SalesOrderHeaders.Remove(salesOrderHeader);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool SalesOrderHeaderExists(int id)
        {
            return _context.SalesOrderHeaders.Any(e => e.SalesOrderId == id);
        }
    }
}
