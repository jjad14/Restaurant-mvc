using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Utility;

namespace Restaurant.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // get current user id
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // get all the users except the currently logged in user
            var users = await _db.ApplicationUser.Where(u => u.Id != claim.Value).ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Lock(string id)
        {
            // id is null
            if (id == null)
            {
                return NotFound();
            }

            // get user via the id
            var applicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(m => m.Id == id);

            // no user is found
            if(applicationUser == null)
            {
                return NotFound();
            }

            // lock user for 30 days
            applicationUser.LockoutEnd = DateTime.Now.AddDays(30);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Unlock(string id)
        {
            // id is null
            if (id == null)
            {
                return NotFound();
            }

            // get user via the id
            var applicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(m => m.Id == id);

            // no user is found
            if (applicationUser == null)
            {
                return NotFound();
            }

            // set LockoutEnd date to now - so user is now unlocked
            applicationUser.LockoutEnd = DateTime.Now;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
    }
}
