using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Entity.EntityContext;

using BikeVille.Auth;
using BikeVille.Auth.AuthContext;
using Microsoft.Extensions.Logging;

namespace BikeVille.Entity.SalesControllers
{
    [Route("[controller]")]
    [ApiController]
    public class SalesOrderHeadersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly AdventureWorksLt2019usersInfoContext _authContext;
        private readonly ILogger<SalesOrderHeadersController> _logger;
        private object shipMethod;

        public SalesOrderHeadersController(AdventureWorksLt2019Context context, AdventureWorksLt2019usersInfoContext authContext, ILogger<SalesOrderHeadersController> logger)
        {
            _context = context;
            _authContext = authContext;
            _logger = logger;
        }

        // GET: api/SalesOrderHeaders
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<SalesOrderHeader>>> GetSalesOrderHeaders()
        {
            var headers = await _context.SalesOrderHeaders
    .Select(h => new SalesOrderHeader
    {
        SalesOrderId = h.SalesOrderId,
        RevisionNumber = (byte)h.RevisionNumber, // Cast esplicito
        Status = (byte)h.Status, // Cast esplicito
        OrderDate = h.OrderDate,
        DueDate = h.DueDate,
        ShipDate = h.ShipDate,
        OnlineOrderFlag = h.OnlineOrderFlag,
        SalesOrderNumber = h.SalesOrderNumber,
        SubTotal = h.SubTotal,
        TaxAmt = h.TaxAmt,
        Freight = h.Freight,
        TotalDue = h.TotalDue,
        BillToAddressId = h.BillToAddressId,
        CreditCardApprovalCode = h.CreditCardApprovalCode,
        Customer = new Customer
        {
            CustomerId = h.Customer.CustomerId,
            NameStyle = h.Customer.NameStyle,
            Title = h.Customer.Title,
            FirstName = h.Customer.FirstName,
            MiddleName = h.Customer.MiddleName,
            LastName = h.Customer.LastName,
            CompanyName = h.Customer.CompanyName,
            SalesPerson = h.Customer.SalesPerson,
            ModifiedDate = h.Customer.ModifiedDate
        },
        SalesOrderDetails = h.SalesOrderDetails.Select(d => new SalesOrderDetail
        {
            SalesOrderId = d.SalesOrderId,
            SalesOrderDetailId = d.SalesOrderDetailId,
            OrderQty = d.OrderQty,
            ProductId = d.ProductId,
            UnitPrice = d.UnitPrice,
            UnitPriceDiscount = d.UnitPriceDiscount,
            LineTotal = d.LineTotal,
            Product = new Product
            {
                ProductId = d.Product.ProductId,
                Name = d.Product.Name,
                ProductNumber = d.Product.ProductNumber,
                Color = d.Product.Color,
                StandardCost = d.Product.StandardCost,
                ListPrice = d.Product.ListPrice,
                Size = d.Product.Size,
                Weight = d.Product.Weight,
                SellStartDate = d.Product.SellStartDate,
                DiscontinuedDate = d.Product.DiscontinuedDate
            }
        }).ToList(),
        ShipToAddressId = h.ShipToAddressId,
        BillToAddress = h.BillToAddress != null
            ? new Address
            {
                AddressId = h.BillToAddress.AddressId,
                AddressLine1 = h.BillToAddress.AddressLine1,
                AddressLine2 = h.BillToAddress.AddressLine2,
                City = h.BillToAddress.City,
                StateProvince = h.BillToAddress.StateProvince,
                CountryRegion = h.BillToAddress.CountryRegion,
                PostalCode = h.BillToAddress.PostalCode,
                ModifiedDate = h.BillToAddress.ModifiedDate
            }
            : null,
        ShipToAddress = h.ShipToAddress != null
            ? new Address
            {
                AddressId = h.ShipToAddress.AddressId,
                AddressLine1 = h.ShipToAddress.AddressLine1,
                AddressLine2 = h.ShipToAddress.AddressLine2,
                City = h.ShipToAddress.City,
                StateProvince = h.ShipToAddress.StateProvince,
                CountryRegion = h.ShipToAddress.CountryRegion,
                PostalCode = h.ShipToAddress.PostalCode,
                ModifiedDate = h.ShipToAddress.ModifiedDate
            }
            : null,
        Comment = h.Comment,
        Rowguid = h.Rowguid,
        ModifiedDate = h.ModifiedDate
    })
    .ToListAsync();

            return headers;
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

        // POST: api/SalesOrderHeaders
        [HttpPost("Add")]
        public async Task<IActionResult> CreateSalesOrderHeader(SalesOrderHeaderRequest salesOrderHeaderRequest)
        {
            // Validazione condizionale
            if (salesOrderHeaderRequest.CustomerId == null && salesOrderHeaderRequest.UserId == null)
            {
                return BadRequest("Either CustomerId or UserId must be provided.");
            }

            // Procedi con la creazione dell'ordine
            var salesOrderHeader = new SalesOrderHeader
            {
                CustomerId = salesOrderHeaderRequest.CustomerId ?? 0,
                UserId = salesOrderHeaderRequest.UserId ?? 0,
                SalesOrderNumber = salesOrderHeaderRequest.SalesOrderNumber,
                RevisionNumber = salesOrderHeaderRequest.RevisionNumber,
                Status = salesOrderHeaderRequest.Status,
                OrderDate = salesOrderHeaderRequest.OrderDate,
                DueDate = salesOrderHeaderRequest.DueDate,
                ShipDate = salesOrderHeaderRequest.ShipDate,
                OnlineOrderFlag = salesOrderHeaderRequest.OnlineOrderFlag,
                SubTotal = salesOrderHeaderRequest.SubTotal,
                TaxAmt = salesOrderHeaderRequest.TaxAmt,
                Freight = salesOrderHeaderRequest.Freight,
                TotalDue = salesOrderHeaderRequest.TotalDue,
                BillToAddressId = salesOrderHeaderRequest.BillToAddressId,
                ShipToAddressId = salesOrderHeaderRequest.ShipToAddressId,
                CreditCardApprovalCode = salesOrderHeaderRequest.CreditCardApprovalCode,
                Comment = salesOrderHeaderRequest.Comment,
                ShipMethod = salesOrderHeaderRequest.shipMethod,
                PurchaseOrderNumber = salesOrderHeaderRequest.PurchaseOrderNumber,
                AccountNumber = salesOrderHeaderRequest.AccountNumber
            };

            try
            {
                // Aggiungi l'ordine al contesto e salva
                _context.SalesOrderHeaders.Add(salesOrderHeader);
                await _context.SaveChangesAsync();


                foreach (var detail in salesOrderHeaderRequest.SalesOrderDetails)
                {
                    var salesOrderDetail = new SalesOrderDetail
                    {
                        SalesOrderId = salesOrderHeader.SalesOrderId,
                        OrderQty = (short)detail.OrderQty,
                        ProductId = detail.ProductId,
                        UnitPrice = detail.UnitPrice,
                        UnitPriceDiscount = detail.UnitPriceDiscount,
                        LineTotal = detail.OrderQty * detail.UnitPrice * (1 - detail.UnitPriceDiscount),

                    };
                    _context.SalesOrderDetails.Add(salesOrderDetail);
                }

                await _context.SaveChangesAsync();

                // Cambia il ruolo dell'utente se necessario
                if (salesOrderHeaderRequest.UserId != null)
                {
                    var user = await _authContext.Users.FindAsync(salesOrderHeaderRequest.UserId);
                    if (user != null && user.Role == "USER      ")
                    {
                        user.Role = "CUSTOMER"; // Modifica direttamente il ruolo
                        Console.WriteLine($"State before SaveChanges: {_authContext.Entry(user).State}");
                        _authContext.Entry(user).State = EntityState.Modified; // Forza il tracciamento
                        var result = await _authContext.SaveChangesAsync();
                        Console.WriteLine($"Numero di record aggiornati: {result}");
                        await _authContext.SaveChangesAsync(); // Salva le modifiche
                        Console.WriteLine($"Role updated successfully for User {user.EmailAddress}");
                        Console.WriteLine($"State after SaveChanges: {_authContext.Entry(user).State}");
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                // Logga l'errore e mostra un messaggio informativo
                Console.WriteLine($"Error saving order: {ex.InnerException?.Message}");
                return StatusCode(500, "An error occurred while saving the order.");
            }

            return CreatedAtAction("GetSalesOrderHeader", new { id = salesOrderHeader.SalesOrderId }, salesOrderHeader);
        }
    }
}
