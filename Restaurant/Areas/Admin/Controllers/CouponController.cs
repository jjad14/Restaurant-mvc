using System;
using System.Collections.Generic;
using System.IO;
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
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // return list of coupons
            var coupons = await _db.Coupon.ToListAsync();

            return View(coupons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            // the images for our coupons will not be saved on the server but rather on the database
            if (ModelState.IsValid)
            {
                // if it is valid we'll fetch the file that was uploaded for the image 
                var files = HttpContext.Request.Form.Files;

                // file was uploaded
                if (files.Count > 0)
                {
                    //  we need to convert the file into a stream of bytes to store it into a database
                    byte[] p1 = null;

                    // this will start reading the file
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        // So what this will do is it will convert our image into a stream of bytes and stored it into var p1
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }

                    }
                    //  and then we just need to add it to our pictures inside the database.
                    coupon.Picture = p1;
                }
                // add all the other entries to the database
                _db.Coupon.Add(coupon);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get coupon by id
            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);

            // no coupon exists
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            // coupon does not exist
            if (coupon.Id == 0)
            {
                return NotFound();
            }

            // get coupon by id
            var couponFromDb = await _db.Coupon.Where(c => c.Id == coupon.Id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                // if it is valid we'll fetch the file that was uploaded for the image 
                var files = HttpContext.Request.Form.Files;
                // file was uploaded
                if (files.Count > 0)
                {
                    // we are saving the image on the database rather than the server
                    // image will be saved in byte array
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponFromDb.Picture = p1;
                }
                // no file was uploaded
                // update the other properties
                couponFromDb.MinimumAmount = coupon.MinimumAmount;
                couponFromDb.Name = coupon.Name;
                couponFromDb.Discount = coupon.Discount;
                couponFromDb.CouponType = coupon.CouponType;
                couponFromDb.IsActive = coupon.IsActive;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Details(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get coupon by id
            var coupon = await _db.Coupon
                .FirstOrDefaultAsync(m => m.Id == id);

            // coupon does not exist
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            // no id passed
            if (id == null)
            {
                return NotFound();
            }

            // get coupon by id
            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            // return coupon to view
            return View(coupon);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // get coupon by id and remove it from db
            var coupons = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            _db.Coupon.Remove(coupons);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
