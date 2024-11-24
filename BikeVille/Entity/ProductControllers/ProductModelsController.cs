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

        public ProductModelsController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // GET: api/ProductModels
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetProductModels()
        {
            return await _context.ProductModels.Include(pm=>pm.Products).ToListAsync();
        }

        // GET: api/ProductModels/5
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<ProductModel>> GetProductModel(int id)
        {
            var productModel = await _context.ProductModels.Include(pm => pm.Products).FirstOrDefaultAsync(pm=>pm.ProductModelId==id);

            if (productModel == null)
            {
                return NotFound();
            }

            return productModel;
        }

        //// PUT: api/ProductModels/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutProductModel(int id, ProductModel productModel)
        //{
        //    if (id != productModel.ProductModelId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(productModel).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ProductModelExists(id))
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

        //// POST: api/ProductModels
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<ProductModel>> PostProductModel(ProductModel productModel)
        //{
        //    _context.ProductModels.Add(productModel);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetProductModel", new { id = productModel.ProductModelId }, productModel);
        //}

        //// DELETE: api/ProductModels/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteProductModel(int id)
        //{
        //    var productModel = await _context.ProductModels.FindAsync(id);
        //    if (productModel == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.ProductModels.Remove(productModel);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool ProductModelExists(int id)
        {
            return _context.ProductModels.Any(e => e.ProductModelId == id);
        }
    }
}
