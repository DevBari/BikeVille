using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Entity;
using BikeVille.Entity.EntityContext;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace BikeVille.Entity.ProductControllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(AdventureWorksLt2019Context context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Products
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                _logger.LogInformation("Recuperati {Count} prodotti.", products.Count);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei prodotti.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero dei prodotti.");
            }
        }

        // GET: api/Products/5
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.ProductModel)
                        .ThenInclude(pm => pm.ProductModelProductDescriptions)
                            .ThenInclude(pmpd => pmpd.ProductDescription)
                    .Include(p => p.SalesOrderDetails)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Prodotto non trovato con ID {ProductId}.", id);
                    return NotFound("Prodotto non trovato.");
                }

                _logger.LogInformation("Recuperato prodotto con ID {ProductId}.", id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del prodotto con ID {ProductId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero del prodotto.");
            }
        }


        [HttpGet("addCart/{id}")]
        public async Task<ActionResult<Product>> GetProductForCart(int id)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Prodotto non trovato per il carrello con ID {ProductId}.", id);
                    return NotFound("Prodotto non trovato.");
                }

                _logger.LogInformation("Recuperato prodotto per il carrello con ID {ProductId}.", id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del prodotto per il carrello con ID {ProductId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero del prodotto per il carrello.");
            }
        }

        [HttpGet("Filter/{name}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(string name)
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                    .ToListAsync();

                if (products == null || !products.Any())
                {
                    _logger.LogWarning("Nessun prodotto trovato con nome contenente '{Name}'.", name);
                    return NotFound("Nessun prodotto trovato.");
                }

                _logger.LogInformation("Recuperati {Count} prodotti con nome contenente '{Name}'.", products.Count, name);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il filtro dei prodotti per nome '{Name}'.", name);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il filtro dei prodotti.");
            }
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDto productDto)
        {
            if (id != productDto.ProductId)
            {
                _logger.LogWarning("Tentativo di aggiornamento fallito per prodotto ID {ProductId}. ID non corrispondente.", id);
                return BadRequest("ID non corrispondente.");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                _logger.LogWarning("Prodotto non trovato con ID {ProductId}.", id);
                return NotFound("Prodotto non trovato.");
            }

            // Aggiorna le proprietà desiderate
            product.Name = productDto.Name;
            product.ProductNumber = productDto.ProductNumber;
            product.Color = productDto.Color;
            product.StandardCost = productDto.StandardCost;
            product.ListPrice = productDto.ListPrice;
            product.Size = productDto.Size;
            product.Weight = productDto.Weight;
            product.ProductCategoryId = productDto.ProductCategoryId;
            product.ProductModelId = productDto.ProductModelId;
            product.SellStartDate = productDto.SellStartDate;
            product.SellEndDate = productDto.SellEndDate;

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Prodotto ID {ProductId} aggiornato con successo.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(id))
                {
                    _logger.LogWarning("Tentativo di aggiornamento fallito: prodotto non trovato con ID {ProductId}.", id);
                    return NotFound("Prodotto non trovato.");
                }
                else
                {
                    _logger.LogError(ex, "Errore di concorrenza durante l'aggiornamento del prodotto ID {ProductId}.", id);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Errore di concorrenza durante l'aggiornamento del prodotto.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generale durante l'aggiornamento del prodotto ID {ProductId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante l'aggiornamento del prodotto.");
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Add")]
           public async Task<ActionResult<Product>> PostProduct(ProductDto productDto)
        {
            try
            {
                var productCategory = await _context.ProductCategories.FirstOrDefaultAsync(c => c.ProductCategoryId == productDto.ProductCategoryId);
                var productModel = await _context.ProductModels.FirstOrDefaultAsync(c => c.ProductModelId == productDto.ProductModelId);

                if (productCategory == null)
                {
                    _logger.LogWarning("Categoria di prodotto non trovata con ID {ProductCategoryId}.", productDto.ProductCategoryId);
                    return BadRequest("Categoria di prodotto non valida.");
                }

                if (productModel == null)
                {
                    _logger.LogWarning("Modello di prodotto non trovato con ID {ProductModelId}.", productDto.ProductModelId);
                    return BadRequest("Modello di prodotto non valido.");
                }

                var product = new Product()
                {
                    Name = productDto.Name,
                    ProductNumber = productDto.ProductNumber,
                    Color = productDto.Color,
                    StandardCost = productDto.StandardCost,
                    ListPrice = productDto.ListPrice,
                    Size = productDto.Size,
                    Weight = productDto.Weight,
                    ProductCategoryId = productDto.ProductCategoryId,
                    ProductModelId = productDto.ProductModelId,
                    SellStartDate = productDto.SellStartDate,
                    SellEndDate = productDto.SellEndDate,
                    DiscontinuedDate = null,
                    ThumbNailPhoto = null,
                    ThumbnailPhotoFileName = null,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.Now,
                    ProductCategory = productCategory,
                    ProductModel = productModel,
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Prodotto aggiunto con ID {ProductId}.", product.ProductId);
                return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiunta del prodotto.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante l'aggiunta del prodotto.");
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.SalesOrderDetails)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Tentativo di eliminazione fallito: prodotto non trovato con ID {ProductId}.", id);
                    return NotFound("Prodotto non trovato.");
                }

                // Rimuove i dettagli degli ordini di vendita associati per prevenire violazioni di chiavi esterne
                if (product.SalesOrderDetails != null && product.SalesOrderDetails.Any())
                {
                    _context.SalesOrderDetails.RemoveRange(product.SalesOrderDetails);
                    _logger.LogInformation("Rimossi SalesOrderDetails associati per prodotto ID {ProductId}.", id);
                }

                // Rimuove il prodotto
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Prodotto eliminato con successo con ID {ProductId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del prodotto con ID {ProductId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante l'eliminazione del prodotto.");
            }
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}