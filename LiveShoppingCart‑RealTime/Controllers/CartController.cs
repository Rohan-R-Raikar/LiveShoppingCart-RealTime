using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Hubs;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;

namespace LiveShoppingCart_RealTime.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public CartController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var items = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["SweetAlertMessage"] = "Product not found!";
                TempData["SweetAlertType"] = "error";
                return NotFound();
            }

            if (product.Stock <= 0)
            {
                TempData["SweetAlertMessage"] = "Product is out of stock!";
                TempData["SweetAlertType"] = "error";
                return RedirectToAction("Index", "Products");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingItem != null)
            {
                if (product.Stock < 1)
                {
                    TempData["SweetAlertMessage"] = "Not enough stock!";
                    TempData["SweetAlertType"] = "error";
                    return RedirectToAction("Index", "Products");
                }

                existingItem.Quantity += 1;
                product.Stock -= 1; // ⬅️ decrease stock
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    UserId = userId,
                    UserName = userName
                };
                _context.CartItems.Add(newItem);
                product.Stock -= 1; // ⬅️ decrease stock
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["SweetAlertMessage"] = "Product added to cart successfully!";
            TempData["SweetAlertType"] = "success";

            //await _hubContext.Clients.All.SendAsync("CartUpdated", $"{User.Identity.Name} added {product.Name} to cart");
            await _hubContext.Clients.All.SendAsync("CartUpdated", new
            {
                Message = $"{User.Identity.Name} added {product?.Name ?? "an item"} from cart",
                ProductId = productId,
                NewStock = product.Stock
            });

            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                if (item.Product != null)
                {
                    item.Product.Stock += item.Quantity;
                    _context.Products.Update(item.Product);
                }

                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();

                TempData["SweetAlertMessage"] = "Product removed from cart successfully!";
                TempData["SweetAlertType"] = "success";

                //await _hubContext.Clients.All.SendAsync("CartUpdated", $"{User.Identity.Name} removed {item.Product?.Name ?? "an item"} from cart");
                await _hubContext.Clients.All.SendAsync("CartUpdated", new
                {
                    Message = $"{User.Identity.Name} removed {item.Product?.Name ?? "an item"} from cart",
                    ProductId = item.Product.Id,
                    NewStock = item.Product.Stock
                });

            }

            return RedirectToAction("Index");
        }
    }
}
