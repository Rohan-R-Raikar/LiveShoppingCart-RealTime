using LiveShoppingCart_RealTime.Authorization;
using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Hubs;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace LiveShoppingCart_RealTime.Controllers
{
    [Route("Products")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<ProductController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: /Products
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Successfully fetched Products Data");
                var products = await _context.Products.Include(p => p.Category).ToListAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while fetching products");
                return View();
            }
        }

        // GET: /Products/Create
        [HttpGet("Create")]
        [HasPermission("CanAddProduct")]
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation($"{nameof(Create)}");
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Creating products");
                return View();
            }
        }

        // POST: /Products/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [HasPermission("CanAddProduct")]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (imageFile != null)
                    {
                        var uploadDir = Path.Combine(_env.WebRootPath, "images/products");
                        Directory.CreateDirectory(uploadDir);

                        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        product.ImageUrl = "/images/products/" + fileName;
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    TempData["SweetAlertMessage"] = "Product Added Successfully!";
                    TempData["SweetAlertType"] = "success";
                    return RedirectToAction(nameof(Index));
                }

                

                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error Ocured while adding product");
                return View(product);
            }
        }

        // GET: /Products/Edit/5
        [HttpGet("Edit/{id}")]
        [HasPermission("CanEditProduct")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Product Could not found");
                return NotFound();
            }
        }

        // POST: /Products/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [HasPermission("CanEditProduct")]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            try
            {
                if (id != product.Id) return NotFound();

                if (ModelState.IsValid)
                {
                    if (imageFile != null)
                    {
                        var uploadDir = Path.Combine(_env.WebRootPath, "images/products");
                        Directory.CreateDirectory(uploadDir);

                        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        product.ImageUrl = "/images/products/" + fileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                TempData["SweetAlertMessage"] = "Product updated successfully!";
                TempData["SweetAlertType"] = "success";

                _logger.LogInformation("Product Updated Successfully");

                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Ocured while Updating Product");
                return View(product);
            }    
        }

        // GET: /Products/Delete/5
        [HttpGet("Delete/{id}")]
        [HasPermission("CanDeleteProduct")]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) 
                { 
                    _logger.LogError("Product could not found");
                    return NotFound(); 
                }
                var product = await _context.Products.Include(p => p.Category)
                                                     .FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogError("Product is Null");
                    return NotFound();
                }
                _logger.LogInformation("Redirecting to Delete");
                return View(product);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"Error Occured");
                return View(ex);
            }
        }

        // POST: /Products/Delete/5
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        [HasPermission("CanDeleteProduct")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }

                TempData["SweetAlertMessage"] = "Product Deleted successfully!";
                TempData["SweetAlertType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error Occured while deleting product");
                return View(ex);
            }
        }
    }

}
