using BuntzenSupplyChain.Domain.Entities;
using BuntzenSupplyChain.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuntzenSupplyChain.Api.Controllers;

public class ItemsController : Controller
{
    private readonly BuntzenDbContext _db;

    public ItemsController(BuntzenDbContext db)
    {
        _db = db;
    }

    // 1. READ: List all Supply Items
    public async Task<IActionResult> Index()
    {
        var items = await _db.Items.AsNoTracking().ToListAsync();
        return View(items);
    }

    // 2. READ: View details of a single Supply Item
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // 3. CREATE (GET): Show form to add a new item
    public IActionResult Create()
    {
        return View(new SupplyItem());
    }

    // 3. CREATE (POST): Process form submission to add new item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplyItem item)
    {
        if (ModelState.IsValid)
        {
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
            _db.Items.Add(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    // 4. UPDATE (GET): Show form to edit an item
    public async Task<IActionResult> Edit(Guid id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // 4. UPDATE (POST): Process edit form submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SupplyItem item)
    {
        if (id != item.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            _db.Items.Update(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    // 5. DELETE (GET): Show confirmation page
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // 5. DELETE (POST): Confirm and remove from database
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item != null)
        {
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
