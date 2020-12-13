using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using Restaurant.Utility;

namespace Restaurant.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // populate view model
            IndexViewModel indexVM = new IndexViewModel()
            {
                MenuItems = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Categories = await _db.Category.ToListAsync(),
                Coupons = await _db.Coupon.Where(c => c.IsActive).ToListAsync()
            };

            // get current user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // claim will be null if user is not logged in
            if (claim != null)
            {
                // get users shopping count
                var count = _db.ShoppingCart.Where(u => u.ApplicationUserId == claim.Value).ToList().Count;

                // save shopping cart count in session
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);
            }

            return View(indexVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            // get menu item by id
            var menuItemFromDb = await _db.MenuItem
                                    .Include(m => m.SubCategory)
                                    .Where(m => m.Id == id)
                                    .FirstOrDefaultAsync();

            // populate shopping cart
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                MenuItem = menuItemFromDb,
                MenuItemId = menuItemFromDb.Id
            };

            // no menu item exists
            if (menuItemFromDb == null)
            {
                return NotFound();
            }

            return View(shoppingCart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart cart)
        {
            cart.Id = 0;

            if (ModelState.IsValid)
            {
                // get current user
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                // set id of cart user id
                cart.ApplicationUserId = claim.Value;

                // get the users cart for that item
                ShoppingCart cartFromDb = await _db.ShoppingCart
                                            .Where(c => c.ApplicationUserId == cart.ApplicationUserId &&
                                                    c.MenuItemId == cart.MenuItemId)
                                            .FirstOrDefaultAsync();

                // menu item is not in the shopping cart so we add it
                if (cart == null)
                {
                   await _db.ShoppingCart.AddAsync(cart);
                }
                else
                {
                    // item is already in the shopping cart so we add to the quantity
                    cartFromDb.Count = cartFromDb.Count + cart.Count;
                }

                await _db.SaveChangesAsync();

                // get users shopping cart total items
                var count = _db.ShoppingCart.Where(c => c.ApplicationUserId == cart.ApplicationUserId).ToList().Count();

                // update shopping cart count via session
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);

                return RedirectToAction(nameof(Index));

            }
            else
            {
                // model state is invalid, so we pass the menu item details again
                var menuItemFromDb = await _db.MenuItem
                                        .Include(m => m.SubCategory)
                                        .Where(m => m.Id == cart.MenuItemId)
                                        .FirstOrDefaultAsync();

                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    MenuItem = menuItemFromDb,
                    MenuItemId = menuItemFromDb.Id
                };

                return View(shoppingCart);
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
