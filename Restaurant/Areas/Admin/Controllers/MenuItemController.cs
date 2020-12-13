using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using Restaurant.Utility;

namespace Restaurant.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _host;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment host)
        {
            _db = db;
            _host = host;

            // initalize menu item
            MenuItemVM = new MenuItemViewModel() 
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }

        public async Task<IActionResult> Index()
        {
            // get list of menu items
            var menuItems = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();

            return View(menuItems);
        }

        // GET - CREATE
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            // get subcategory id
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            // add menu item
            _db.MenuItem.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            // Save Image section
            // get wwwroot path
            string webRootPath = _host.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            // get menu item
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            // file has been uploaded
            if (files.Count > 0)
            {
                // add images to wwwroot path (access images)
                var uploads = Path.Combine(webRootPath, "images");
                // get file extension
                var extension = Path.GetExtension(files[0].FileName);

                // creating file, renaming it
                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    // copy file to file stream
                    files[0].CopyTo(fileStream);
                }

                // add new file name with path to the menuItemFromDb
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            else
            {
                // no file was uploaded, so use default image
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get menu item
            MenuItemVM.MenuItem = await _db.MenuItem
                                        .Include(p => p.Category)
                                        .Include(p => p.SubCategory)
                                        .SingleOrDefaultAsync(p => p.Id == id);

            // get subcategories
            MenuItemVM.SubCategory = await _db.SubCategory
                                        .Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId)
                                        .ToListAsync();

            // no menu item exists
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                // if the model state is invalid we have to assign the subcategory or it will show us an error
                MenuItemVM.SubCategory = await _db.SubCategory
                                            .Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId)
                                            .ToListAsync();
                return View(MenuItemVM);
            }

            // Save Image section
            string webRootPath = _host.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                // New image has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                // delete the old file
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // upload the new file
                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_new;
            }
            // else no image was reuploaded

            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET - DETAILS 
        public async Task<IActionResult> Details(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get menu item
            MenuItemVM.MenuItem = await _db.MenuItem
                                    .Include(m => m.Category)
                                    .Include(m => m.SubCategory)
                                    .SingleOrDefaultAsync(m => m.Id == id);

            // no menu item exists
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get menu item
            MenuItemVM.MenuItem = await _db.MenuItem
                                    .Include(m => m.Category)
                                    .Include(m => m.SubCategory)
                                    .SingleOrDefaultAsync(m => m.Id == id);

            // menu item does not exist
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // get wwwroot path
            string webRootPath = _host.WebRootPath;

            // find menu item by id
            MenuItem menuItem = await _db.MenuItem.FindAsync(id);

            // menu item does not exist
            if (menuItem != null)
            {
                // get image path of menu item
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                // if the image exists then delete it
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                // remove menu item from db
                _db.MenuItem.Remove(menuItem);

                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
