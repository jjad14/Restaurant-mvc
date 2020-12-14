using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using Restaurant.Utility;
using Stripe;

namespace Restaurant.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public OrderDetailsCartViewModel cartVM { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            cartVM = new OrderDetailsCartViewModel()
            {
                OrderHeader = new Models.OrderHeader()
            };

            cartVM.OrderHeader.OrderTotal = 0;

            // get current user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // get users shopping cart
            var cart = _db.ShoppingCart.Where(C => C.ApplicationUserId == claim.Value);

            // cart exists
            if (cart != null)
            {
                cartVM.CartList = cart.ToList();
            }

            // iterate through items in cart, get the order total
            foreach(var item in cartVM.CartList)
            {
                item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                cartVM.OrderHeader.OrderTotal = cartVM.OrderHeader.OrderTotal + (item.MenuItem.Price * item.Count);
                item.MenuItem.Description = SD.ConvertToRawHtml(item.MenuItem.Description);

                // reduce description if it exceeds 100 characters
                if (item.MenuItem.Description.Length > 100)
                {
                    item.MenuItem.Description = item.MenuItem.Description.Substring(0, 99) + "...";
                }
            }

            // set order total original - both will be the same before coupons/discount is added
            cartVM.OrderHeader.OrderTotalOriginal = cartVM.OrderHeader.OrderTotal;

            // check for that session and display the price accordingly
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                cartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == cartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                cartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, cartVM.OrderHeader.OrderTotalOriginal);
            }

            return View(cartVM);
        }

        public async Task<IActionResult> Summary()
        {
            cartVM = new OrderDetailsCartViewModel()
            {
                OrderHeader = new Models.OrderHeader()
            };

            cartVM.OrderHeader.OrderTotal = 0;

            // get current user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ApplicationUser applicationUser = await _db.ApplicationUser.Where(u => u.Id == claim.Value).FirstOrDefaultAsync();

            // get users shopping cart
            var cart = _db.ShoppingCart.Where(C => C.ApplicationUserId == claim.Value);

            // cart exists
            if (cart != null)
            {
                cartVM.CartList = cart.ToList();
            }

            // iterate through items in cart, get the order total
            foreach (var item in cartVM.CartList)
            {
                item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                cartVM.OrderHeader.OrderTotal = cartVM.OrderHeader.OrderTotal + (item.MenuItem.Price * item.Count);
            }

            // set order total original - both will be the same before coupons/discount is added
            cartVM.OrderHeader.OrderTotalOriginal = cartVM.OrderHeader.OrderTotal;

            cartVM.OrderHeader.PickupName = applicationUser.Name;
            cartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            cartVM.OrderHeader.PickupTime = DateTime.Now;

            // check for that session and display the price accordingly
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                cartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == cartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                cartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, cartVM.OrderHeader.OrderTotalOriginal);
            }

            return View(cartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            // get current user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            cartVM.CartList = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

            cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            cartVM.OrderHeader.OrderDate = DateTime.Now;
            cartVM.OrderHeader.UserId = claim.Value;
            cartVM.OrderHeader.Status = SD.PaymentStatusPending;
            cartVM.OrderHeader.PickupTime = Convert.ToDateTime(cartVM.OrderHeader.PickupDate.ToShortDateString() + " " + cartVM.OrderHeader.PickupTime.ToShortTimeString());

            // create a list of order details
            List<OrderDetails> orderDetailsList = new List<OrderDetails>();

            _db.OrderHeader.Add(cartVM.OrderHeader);
            await _db.SaveChangesAsync();

            cartVM.OrderHeader.OrderTotalOriginal = 0;

            // iterate through items in cart, get the order total
            foreach (var item in cartVM.CartList)
            {
                item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);

                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = cartVM.OrderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };

                cartVM.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;

                _db.OrderDetails.Add(orderDetails);
            }

            // check for that session and display the price accordingly
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                cartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == cartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                cartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, cartVM.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                cartVM.OrderHeader.OrderTotal = cartVM.OrderHeader.OrderTotalOriginal;
            }
            // set coupon discount amount
            cartVM.OrderHeader.CouponCodeDiscount = cartVM.OrderHeader.OrderTotalOriginal - cartVM.OrderHeader.OrderTotal;

            // remove shopping cart
            _db.ShoppingCart.RemoveRange(cartVM.CartList);

            // reset session
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
            await _db.SaveChangesAsync();

            // Transaction to stripe
            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(cartVM.OrderHeader.OrderTotal * 100),
                Currency = "cad",
                Description = "Order Id: " + cartVM.OrderHeader.Id,
                Source = stripeToken
            };

            // create charge to stripe
            var service = new ChargeService();
            Charge charge = service.Create(options);

            // if null then there is a error with the payment
            if (charge.BalanceTransactionId == null)
            {
                cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else {
                cartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
            }

            // payment succeeded
            if (charge.Status.ToLower() == "succeded")
            {
                cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                cartVM.OrderHeader.Status = SD.StatusSubmitted;
            }
            else
            {
                cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");

            // return RedirectToAction("Confirm", "Order", new { id = cartVM.OrderHeader.Id });
        }

        public IActionResult AddCoupon()
        {
            // TODO: Check if coupon is valid
            // might need to add a isValid prop for the coupon
            // var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == cartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();

            if (cartVM.OrderHeader.CouponCode == null)
            {
                cartVM.OrderHeader.CouponCode = "";
            }

            // TODO: if coupon is valid set the session else dont
            HttpContext.Session.SetString(SD.ssCouponCode, cartVM.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {
            // reset session
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Increment(int cartId)
        {
            // get cart by id
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Count += 1;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Decrement(int cartId)
        {
            // get cart by id
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            // if quantity is 1 then instead of decrementing we remove it
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);

                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);

            } else
            {
                cart.Count -= 1;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            // get cart by id
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            // remove cart
            _db.ShoppingCart.Remove(cart);

            await _db.SaveChangesAsync();

            // update session
            var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);

            return RedirectToAction(nameof(Index));
        }

    }
}

    //< script src = "https://cdnjs.cloudflare.com/ajax/libs/jquery-timepicker/1.10.0/jquery.timepicker.js" ></ script >
    // < script src = "https://cdnjs.cloudflare.com/ajax/libs/timepicker/1.3.5/jquery.timepicker.min.js" ></ script >
    //  < script src = "//cdnjs.cloudflare.com/ajax/libs/timepicker/1.3.5/jquery.timepicker.min.js" ></ script >