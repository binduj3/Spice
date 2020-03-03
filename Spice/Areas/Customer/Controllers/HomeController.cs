using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;

namespace Spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        private readonly IEmailSender emailService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db,IEmailSender emailService)
        {
            _db = db;
            this.emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
       if(claim!=null)
            {
                int count = _db.ShoppingCarts.Where(a => a.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32("ssCartCount", count);
                await emailService.SendEmailAsync(_db.Users.Where(a => a.Id == claim.Value).FirstOrDefault().Email, "Spice Order created", "Order has being summited successfully");
            }
            IndexViewModel IndexVm = new IndexViewModel()
            {
                MenuItem = await _db.MenuItem.Include(m => m.SubCategory).Include(m => m.Category).ToListAsync(),
                Category = await _db.Category.ToListAsync(),
                Coupon = await _db.Coupon.Where(m => m.IsActive == true).ToListAsync()
            };

          
            return View(IndexVm);
        }

        public IActionResult Privacy()
        {
            

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                MenuItem menuItemFromDb = await _db.MenuItem.Include(a => a.Category).Include(a => a.SubCategory).Where(a => a.Id == id).FirstOrDefaultAsync();
                ShoppingCart shopping = new ShoppingCart
                {
                    MenuItem = menuItemFromDb,
                    MenuItemId = menuItemFromDb.Id
                };
                return View(shopping);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            try
            {
                shoppingCart.Id = 0;
                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    shoppingCart.ApplicationUserId = claim.Value;

                    ShoppingCart shoppingFromDb = await _db.ShoppingCarts
                                                .Where(a => a.ApplicationUserId == shoppingCart.ApplicationUserId && a.MenuItemId == shoppingCart.MenuItem.Id)
                                                .FirstOrDefaultAsync();

                    if(shoppingFromDb==null)
                    {
                        _db.ShoppingCarts.Add(shoppingCart);
                    }
                    else
                    {
                        shoppingFromDb.Count += shoppingCart.Count;
                    }
                    await _db.SaveChangesAsync();

                    var itemCount =   _db.ShoppingCarts.Where(a => a.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();
                    HttpContext.Session.SetInt32("ssCartCount", itemCount);
                   return RedirectToAction("Index");
                }
              
                    MenuItem menuItemFromDb = await _db.MenuItem.Include(a => a.Category).Include(a => a.SubCategory).Where(a => a.Id == shoppingCart.MenuItemId).FirstOrDefaultAsync();
                    ShoppingCart shopping = new ShoppingCart
                    {
                        MenuItem = menuItemFromDb,
                        MenuItemId = menuItemFromDb.Id
                    };
                    return View(shopping);
                
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}
