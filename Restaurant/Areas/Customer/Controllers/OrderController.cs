using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using Restaurant.Utility;

namespace Restaurant.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        // private readonly IEmailSender _emailSender;
        private int PageSize = 2;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            // get current user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()
            };

            return View(orderDetailsViewModel);
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage = 1)
        {
            // get current user
           var claimsIdentity = (ClaimsIdentity)User.Identity;
           var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            // get all the users order headers
            List<OrderHeader> OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.UserId == claim.Value).ToListAsync();

            // for each order header, populate the OrderDetailsViewModel view model
            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };

                // add the individual OrderDetailsViewModel to the list
                orderListVM.Orders.Add(individual);
            }

            // paging info - total number of orders
            var count = orderListVM.Orders.Count;
            // paginate the info
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.Id)
                                 .Skip((productPage - 1) * PageSize)
                                 .Take(PageSize).ToList();

            // populate paging info in view model
            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                // custom tag helper will remove colon with value as anchor tag
                urlParam = "/Customer/Order/OrderHistory?productPage=:" 
            };

            return View(orderListVM);
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> ManageOrder()
        {
            List<OrderDetailsViewModel> orderDetailsVM = new List<OrderDetailsViewModel>();

            // get all orders whos status is either submitted or in process
            List<OrderHeader> OrderHeaderList = await _db.OrderHeader
                                                    .Where(o => o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess)
                                                    .OrderByDescending(o => o.PickupTime)
                                                    .ToListAsync();

            // for each order, populate OrderDetailsViewModel and add to the viewmodel
            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };

                orderDetailsVM.Add(individual);
            }

            // return orders which are sorted by pickup time
            return View(orderDetailsVM.OrderBy(o => o.OrderHeader.PickupTime));
        }

        public async Task<IActionResult> GetOrderDetails(int id)
        {
            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id),
                OrderDetails = await _db.OrderDetails.Where(m => m.OrderId == id).ToListAsync()
            };

            orderDetailsViewModel.OrderHeader.ApplicationUser = await _db.ApplicationUser
                                                                    .FirstOrDefaultAsync(u => u.Id == orderDetailsViewModel.OrderHeader.UserId);

            return PartialView("_IndividualOrderDetails", orderDetailsViewModel);
        }

        public IActionResult GetOrderStatus(int Id)
        {
            // return partialView with the order header
            return PartialView("_OrderStatus", _db.OrderHeader.Where(m => m.Id == Id).FirstOrDefault().Status);

        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(int orderId)
        {
            // change order status to in process
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderReady(int orderId)
        {
            // change order status to ready
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusReady;
            await _db.SaveChangesAsync();

            // TODO: email to notify user that order is ready for pickup

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int orderId)
        {
            // change order status to cancelled
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Order");
        }
    }
}
