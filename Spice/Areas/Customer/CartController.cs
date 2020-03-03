using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using Stripe;

namespace Spice.Areas.Customer
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext db;

        [BindProperty]
        public OrderDetailsCart detailCart { get; set; }
        public CartController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IActionResult> Index()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };
            detailCart.OrderHeader.OrderTotal = 0;
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var cart = db.ShoppingCarts.Where(a => a.ApplicationUserId == claim.Value);

            if (cart != null)
            {
                detailCart.listCart = cart.ToList();
            }

            foreach (var list in detailCart.listCart)
            {
                list.MenuItem = await db.MenuItem.FirstOrDefaultAsync(a => a.Id == list.MenuItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);
                if (list.MenuItem.Description.Length > 100)
                {
                    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                }
            }
            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal;
          if(HttpContext.Session.GetString(SD.ssCouponCode)!=null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await db.Coupon.Where(a => a.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }
            return View(detailCart);
        }

        public IActionResult AddCoupoun()
        {
            try
            {
                if (detailCart.OrderHeader.CouponCode == null)
                {
                    detailCart.OrderHeader.CouponCode = "";
                }
                HttpContext.Session.SetString(SD.ssCouponCode, detailCart.OrderHeader.CouponCode);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                throw ;
                
            }
        }

        public IActionResult RemoveCoupon()
        {
            try
            {
                HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task< IActionResult> Plus(int cartId)
        {
            try
            {
                var cart = await db.ShoppingCarts.Where(a => a.Id == cartId).FirstOrDefaultAsync();
                cart.Count += 1;
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            try
            {
                var cart = await db.ShoppingCarts.Where(a => a.Id == cartId).FirstOrDefaultAsync();
                if (cart.Count == 1)
                {
                    db.ShoppingCarts.Remove(cart);
                    await db.SaveChangesAsync();

                    var cnt = db.ShoppingCarts.Where(a => a.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                    HttpContext.Session.SetString(SD.ssShoppingCartCount, cnt.ToString());
                }
                else
                {
                    cart.Count -= 1;
                    await db.SaveChangesAsync();
                }
             
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            try
            {
                var cart = await db.ShoppingCarts.Where(a => a.Id == cartId).FirstOrDefaultAsync();
                db.ShoppingCarts.Remove(cart);
                await db.SaveChangesAsync();

                var cnt = db.ShoppingCarts.Where(a => a.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetString(SD.ssShoppingCartCount, cnt.ToString());
            
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IActionResult> Summary()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };
            detailCart.OrderHeader.OrderTotal = 0;
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var cart = db.ShoppingCarts.Where(a => a.ApplicationUserId == claim.Value);
            ApplicationUser applicationUser = await db.ApplicationUser.Where(a => a.Id == claim.Value).FirstOrDefaultAsync();
            if (cart != null)
            {
                detailCart.listCart = cart.ToList();
            }

            foreach (var list in detailCart.listCart)
            {
                list.MenuItem = await db.MenuItem.FirstOrDefaultAsync(a => a.Id == list.MenuItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
               
            }
            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal;
            detailCart.OrderHeader.PickupName = applicationUser.Name;
            detailCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            detailCart.OrderHeader.PickUpTime = DateTime.Now;


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await db.Coupon.Where(a => a.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }
            return View(detailCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(string StripeToken)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detailCart.listCart = await db.ShoppingCarts.Where(a => a.ApplicationUserId == claim.Value).ToListAsync();

            detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            detailCart.OrderHeader.OrderDate = DateTime.Now;
            detailCart.OrderHeader.UserId = claim.Value;
            detailCart.OrderHeader.Status = SD.PaymentStatusPending;
            detailCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailCart.OrderHeader.PickUpDate.ToShortDateString() + " " + detailCart.OrderHeader.PickUpTime.ToShortDateString());

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            db.OrderHeader.Add(detailCart.OrderHeader);
            await db.SaveChangesAsync();

            detailCart.OrderHeader.OrderTotalOriginal = 0;

            foreach (var list in detailCart.listCart)
            {
                list.MenuItem = await db.MenuItem.FirstOrDefaultAsync(a => a.Id == list.MenuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = list.MenuItemId,
                    OrderHeaderId = detailCart.OrderHeader.Id,
                    Description = list.MenuItem.Description,
                    Name = list.MenuItem.Name,
                    Price = list.MenuItem.Price,
                    Count = list.Count
                };
                detailCart.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                db.OrderDetails.Add(orderDetails);
            }

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await db.Coupon.Where(a => a.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotalOriginal;
            }
            detailCart.OrderHeader.CouponCodeDiscount = detailCart.OrderHeader.OrderTotalOriginal - detailCart.OrderHeader.OrderTotal;
            await db.SaveChangesAsync();
            db.ShoppingCarts.RemoveRange(detailCart.listCart);
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
            await db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(detailCart.OrderHeader.OrderTotal * 100),
                Currency = "CAD",
                Description = "Order Id:" + detailCart.OrderHeader.Id,
                Source = StripeToken
            };
            var service = new ChargeService();
            Charge charge = service.Create(options);
            if (charge.BalanceTransactionId == null)
            {
                detailCart.OrderHeader.Status = SD.PaymentStatusRejected;
            }
            else
            {
                detailCart.OrderHeader.TranscationId = charge.BalanceTransactionId;

            }
            if (charge.Status.ToLower() == "succeeded")
            {
                detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;

                detailCart.OrderHeader.Status = SD.StatusSubmitted;

            }
            else
            {
                detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;

            }
            await db.SaveChangesAsync();
            return RedirectToAction("Confirm","Order",new {id=detailCart.OrderHeader.Id });
        }

    }
}