using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Entity;
using BikeVille.Entity.EntityContext;
using Microsoft.AspNetCore.Authorization;

namespace BikeVille.Entity.ProductControllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ILogger<ProductCategoriesController> _logger;

        public ProductCategoriesController(AdventureWorksLt2019Context context, ILogger<ProductCategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ProductCategories
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetProductCategories()
        {
            try
            {
                var productCategories = await _context.ProductCategories
                    .Include(c => c.Products)
                    .Include(c => c.InverseParentProductCategory)
                        .ThenInclude(ip => ip.Products)
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} categorie di prodotto.", productCategories.Count);
                return Ok(productCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle categorie di prodotto.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero delle categorie di prodotto.");
            }
        }


        [HttpGet("IndexWhithOutProducts")]
       public async Task<ActionResult<IEnumerable<ProductCategory>>> GetProductCategoriesWhithOutProducts()
        {
            try
            {
                var productCategories = await _context.ProductCategories.ToListAsync();
                _logger.LogInformation("Recuperati {Count} categorie di prodotto senza prodotti associati.", productCategories.Count);
                return Ok(productCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle categorie di prodotto senza prodotti.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero delle categorie di prodotto.");
            }
        }

        // GET: api/ProductCategories/5
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<ProductCategory>> GetProductCategory(int id)
        {
            try
            {
                var productCategory = await _context.ProductCategories
                    .Include(c => c.Products)
                    .Include(c => c.InverseParentProductCategory)
                        .ThenInclude(ip => ip.Products)
                    .FirstOrDefaultAsync(c => c.ProductCategoryId == id);

                if (productCategory == null)
                {
                    _logger.LogWarning("Categoria di prodotto non trovata con ID {ProductCategoryId}.", id);
                    return NotFound("Categoria di prodotto non trovata.");
                }

                _logger.LogInformation("Recuperata categoria di prodotto con ID {ProductCategoryId}.", id);
                return Ok(productCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della categoria di prodotto con ID {ProductCategoryId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero della categoria di prodotto.");
            }
        }

        private bool ProductCategoryExists(int id)
        {
            return _context.ProductCategories.Any(e => e.ProductCategoryId == id);
        }
    }
}
