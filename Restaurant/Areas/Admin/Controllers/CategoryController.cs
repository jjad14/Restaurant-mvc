using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Utility;

namespace Restaurant.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            // return list of categories
            return View(await _db.Category.ToListAsync());
        }

        // GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        // POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if(ModelState.IsValid)
            {
                // add category to db
                _db.Category.Add(category);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET- EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            // no id was passed
            if (id == null)
            {
                return NotFound();
            }

            // find category by id
            var category = await _db.Category.FindAsync(id);

            // no category exists
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                // update category
                _db.Category.Update(category);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            // no id was passed
            if (id == null)
            {
                return NotFound();
            }

            // find category by id
            var category = await _db.Category.FindAsync(id);

            // no category exists
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST - DELETE
        [HttpDelete, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            // find category by id
            var category = await _db.Category.FindAsync(id);

            // no category exists
            if (category == null)
            {
                return View();
            }

            // delete category from db
            _db.Category.Remove(category);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET - DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            // no id was passed
            if (id == null)
            {
                return NotFound();
            }

            // find category by id
            var category = await _db.Category.FindAsync(id);

            // no category exists
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

    }
}
