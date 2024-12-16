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

namespace BikeVille.Entity.ProductControllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public ProductsController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet("Index")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.ProductCategory).Include(p => p.ProductModel).ThenInclude(pm => pm.ProductModelProductDescriptions).ThenInclude(pmpd => pmpd.ProductDescription).Include(p => p.SalesOrderDetails).FirstOrDefaultAsync(p=>p.ProductId==id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet("addCart/{id}")]
        public async Task<ActionResult<Product>> GetProductForCart(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet("Filter/{name}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(string name)
        {
            var product = await _context.Products.Where(p => p.Name.ToLower().Contains(name.ToLower())).ToListAsync();

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDto productDto)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);


            if (id != product.ProductId)
            {
                return BadRequest();
            }


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
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Add")]
        public async Task<ActionResult<Product>> PostProduct(ProductDto productDto)
        {
            var product = new Product()
            {
                Name=productDto.Name,
                ProductNumber=productDto.ProductNumber,
                Color= productDto.Color,
                StandardCost=productDto.StandardCost,
                ListPrice=productDto.ListPrice,
                Size=productDto.Size,
                Weight=productDto.Weight,
                ProductCategoryId = productDto.ProductCategoryId,
                ProductModelId=productDto.ProductModelId,
                SellStartDate=productDto.SellStartDate,
                SellEndDate=productDto.SellEndDate,
                DiscontinuedDate=null,
                ThumbNailPhoto=null,
                ThumbnailPhotoFileName=null,
                Rowguid=new Guid(),
                ModifiedDate=DateTime.Now,
                ProductCategory=_context.ProductCategories.FirstOrDefault(c=>c.ProductCategoryId==productDto.ProductCategoryId),
                ProductModel=_context.ProductModels.FirstOrDefault(c=>c.ProductModelId==productDto.ProductModelId),
                
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Retrieve the product including related sales order details
            var product = await _context.Products
                .Include(p => p.SalesOrderDetails) // Ensure you load the related SalesOrderDetails
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Remove the related SalesOrderDetails first to prevent foreign key violation
            _context.SalesOrderDetails.RemoveRange(product.SalesOrderDetails);

            // Now, remove the product
            _context.Products.Remove(product);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //[HttpDelete("Delete/{id}")]
        //public async Task<IActionResult> DeleteProduct(int id)
        //{
        //    var product = await _context.Products.FindAsync(id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Products.Remove(product);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
