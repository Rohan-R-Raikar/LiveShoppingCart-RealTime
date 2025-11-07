using LiveShoppingCart_RealTime.Data;
using LiveShoppingCart_RealTime.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveShoppingCart_RealTime.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ApplicationDbContext context, Logger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index() =>
            View(await _context.Categories.ToListAsync());

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["SweetAlertMessage"] = "Category Added Successfully!";
                    TempData["SweetAlertType"] = "success";

                    _logger.LogInformation("New Category Added Successfully");

                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception Occured while Creating Category");
                return View(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error Occured while Editing Category");
                return View(ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            try
            {
                if (id != category.Id) return NotFound();
                if (ModelState.IsValid)
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["SweetAlertMessage"] = "Category Edited Successfully!";
                    TempData["SweetAlertType"] = "success";

                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error Occured while Editing Category");
                return View(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error Occured while Deleting Category");
                return View(ex);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                }

                TempData["SweetAlertMessage"] = "Category Deleted Successfully!";
                TempData["SweetAlertType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error Occured while Deleting Category");
                return View(ex);
            }
        }
    }
}
