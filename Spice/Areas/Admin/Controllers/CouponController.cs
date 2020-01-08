using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]

    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public Coupon Coupon { get; set; }

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async  Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        //Get Create
        public IActionResult Create()
        {
            return View();
        }

        //Post Create
        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if(files.Count>0)
                {
                    byte[] picture1 = null;
                    using (var fs= files[0].OpenReadStream())
                    {
                    using (var ms =new MemoryStream())
                    {
                            fs.CopyTo(ms);
                            picture1 = ms.ToArray();
                    }
                    }

                        Coupon.Picture = picture1;
                }
               
            await  _db.Coupon.AddAsync(Coupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            }
            return View(Coupon);
        }

        //Get Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupon model = await _db.Coupon.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }

          return View(model);
        }


        //Get Post
        [HttpPost,ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }
            Coupon model =await _db.Coupon.FindAsync(id);
            if(model==null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] picture1 = null;
                    using (var fs = files[0].OpenReadStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            picture1 = ms.ToArray();
                        }
                    }

                    model.Picture = picture1;
                }
                model.Name = Coupon.Name;
                model.CouponType = Coupon.CouponType;
                model.Discount = Coupon.Discount;
                model.MinimumAmount = Coupon.MinimumAmount;
                model.IsActive = Coupon.IsActive;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Coupon);
        }

        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupon model = await _db.Coupon.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        //Get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupon model = await _db.Coupon.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        //Post Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupon model = await _db.Coupon.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            _db.Coupon.Remove(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}