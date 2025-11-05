using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Hubs;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace LiveShoppingCart_RealTime.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public ProductsController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                TempData["SweetAlertMessage"] = "Product not found!";
                TempData["SweetAlertType"] = "error";
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                TempData["SweetAlertMessage"] = "Product not found!";
                TempData["SweetAlertType"] = "error";
                return NotFound();
            }
            ViewBag.ProductId = product.Id;

            return View(product);
        }

        //[Authorize(Roles ="Admin")]
        public IActionResult Create() => View();

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                TempData["SweetAlertMessage"] = "Fill all requiered Values of Product";
                TempData["SweetAlertType"] = "error";
                return BadRequest(ModelState);
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SweetAlertMessage"] = "Product created successfully!";
            TempData["SweetAlertType"] = "success";

            //await _hubContext.Clients.All.SendAsync("CartUpdated", new
            //{
            //    Message = $"{User.Identity.Name} removed {item.Product?.Name ?? "an item"} from cart",
            //    ProductId = item.Product.Id,
            //    NewStock = item.Product.Stock
            //});

            //await _hubContext.Clients.All.SendAsync("New Product Added", $"{User.Identity.Name} added {product.Name} to cart");

            await _hubContext.Clients.All.SendAsync("NewProductAdded", new
            {
                Message = $"Newly added {product.Name} in Products List Available",
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    price = product.Price,
                    stock = product.Stock
                }
            });

            return RedirectToAction("Index");
        }

        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["SweetAlertMessage"] = "Product not found!";
                TempData["SweetAlertType"] = "error";
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["SweetAlertMessage"] = "Product not found!";
                TempData["SweetAlertType"] = "error";
                return NotFound();
            }
            return View(product);
        }

        //[HttpPost, Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product updatedProduct)
        {
            if (!ModelState.IsValid)
                return View(updatedProduct);

            try
            {
                _context.Products.Update(updatedProduct);
                await _context.SaveChangesAsync();

                TempData["SweetAlertMessage"] = "Product updated successfully!";
                TempData["SweetAlertType"] = "success";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(updatedProduct.Id))
                {
                    TempData["SweetAlertMessage"] = "Product not found!";
                    TempData["SweetAlertType"] = "error";
                    return NotFound();
                }
                else
                    throw;
            }

            return RedirectToAction("Index");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }


        //[HttpPost, Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int Productid)
        {
            var product = await _context.Products.FindAsync(Productid);
            if (product == null)
            {
                TempData["SweetAlertMessage"] = "Product not found!";
                TempData["SweetAlertType"] = "error";
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SweetAlertMessage"] = "Product deleted successfully!";
            TempData["SweetAlertType"] = "success";

            return RedirectToAction("Index");
        }
    }
}
