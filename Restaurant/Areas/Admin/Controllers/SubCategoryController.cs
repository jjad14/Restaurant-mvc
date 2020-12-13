using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using Restaurant.Utility;

namespace Restaurant.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET index
        public async Task<IActionResult> Index()
        {
            // return list of subcategories
            var subCategories = await _db.SubCategory.Include(s => s.Category).ToListAsync();

            return View(subCategories);
        }

        // GET - CREATE 
        public async Task<IActionResult> Create()
        {
            // populate view model
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        // POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // get the subcategory if it exists - keeps subcategories unique
                var doesSubCategoryExist = _db.SubCategory
                    .Include(s => s.Category)
                    .Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                // if count is greater than 0 then subcategory exists
                if (doesSubCategoryExist.Count() > 0)
                {
                    // Error
                    StatusMessage = "Error : Sub Category exists under " + doesSubCategoryExist.First().Category.Name + " category. Please use another name";
                }
                else
                {
                    // add subcategory to db
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            // modelstate is invalid - return view model
            SubCategoryAndCategoryViewModel modelVm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(modelVm);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            // get a list of subcategories from a specific category
            subCategories = await (from subCategory in _db.SubCategory
                             where subCategory.CategoryId == id
                             select subCategory).ToListAsync();

            // return as a json response
            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        // GET - EDIT 
        public async Task<IActionResult> Edit(int? id)
        {
            // no id 
            if(id == null)
            {
                return NotFound();
            }

            // get subcategory via the id
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);

            // no subcategory exists
            if (subCategory == null)
            {
                return NotFound();
            }

            // populate and return view model
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        // POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // check if subcategory exists by checking if names match and by the category id
                var doesSubCategoryExist = _db.SubCategory
                    .Include(s => s.Category)
                    .Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                // edited subcategory already exists
                if (doesSubCategoryExist.Count() > 0)
                {
                    // Error, it is not a distinct entry inside the same category
                    StatusMessage = "Error : Sub Category exists under " + doesSubCategoryExist.First().Category.Name + " category. Please use another name";
                }
                else
                {
                    // get the subcategory from the db
                    var subCatFromDb = await _db.SubCategory.FindAsync(model.SubCategory.Id);
                    // update name
                    subCatFromDb.Name = model.SubCategory.Name;

                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            // model state is invalid, return view model
            SubCategoryAndCategoryViewModel modelVm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),
                StatusMessage = StatusMessage
            };

            // modelVm.SubCategory.Id = id;

            return View(modelVm);
        }

        // GET - DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get subcategory via id
            var subCategory = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);

            // no subcategory exists
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        // GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get subcategory via id
            var subCategory = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);

            // no subcategory exists
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // get subcategory by id
            var subCategory = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);

            // remove subcategory from db
            _db.SubCategory.Remove(subCategory);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
