using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Hubs;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
            if (product == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += 1;
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
            }
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("CartUpdated", $"{User.Identity.Name} added {product.Name} to cart");
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("CartUpdated", $"{User.Identity.Name} removed {item.Product?.Name ?? "an item"} from cart");
            }

            return RedirectToAction("Index");
        }
    }
}
