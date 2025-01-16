using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Entity;
using BikeVille.Entity.EntityContext;

namespace BikeVille.Entity.ProductControllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductModelsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ILogger<ProductModelsController> _logger;

        public ProductModelsController(AdventureWorksLt2019Context context, ILogger<ProductModelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ProductModels
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetProductModels()
        { 
            try
            {
                var productModels = await _context.ProductModels
                    .Include(pm => pm.Products)
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} modelli di prodotto.", productModels.Count);
                return Ok(productModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei modelli di prodotto.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero dei modelli di prodotto.");
            }
        }

        // GET: api/ProductModels/5
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<ProductModel>> GetProductModel(int id)
        {
            try
            {
                var productModel = await _context.ProductModels
                    .Include(pm => pm.Products)
                    .FirstOrDefaultAsync(pm => pm.ProductModelId == id);

                if (productModel == null)
                {
                    _logger.LogWarning("Modello di prodotto non trovato con ID {ProductModelId}.", id);
                    return NotFound("Modello di prodotto non trovato.");
                }

                _logger.LogInformation("Recuperato modello di prodotto con ID {ProductModelId}.", id);
                return Ok(productModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del modello di prodotto con ID {ProductModelId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore del server durante il recupero del modello di prodotto.");
            }
        }

        private bool ProductModelExists(int id)
        {
            return _context.ProductModels.Any(e => e.ProductModelId == id);
        }
    }
}
