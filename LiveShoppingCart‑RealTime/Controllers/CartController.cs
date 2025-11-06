using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LiveShoppingCart_RealTime.Controllers
{
    [Route("Cart")]
    [Authorize] // Ensure user is logged in
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // <-- fixed here

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return View(cart);
        }


        // POST: Cart/Add/5
        [HttpPost("Add/{productId}")]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return NotFound();

                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    PriceAtPurchase = product.Price
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove/5
        [HttpPost("Remove/{itemId}")]
        public async Task<IActionResult> Remove(int itemId)
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem == null) return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity/5
        [HttpPost("UpdateQuantity/{itemId}")]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            if (quantity < 1) return BadRequest("Quantity must be at least 1");

            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem == null) return NotFound();

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Clear
        [HttpPost("Clear")]
        public async Task<IActionResult> Clear()
        {
            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
