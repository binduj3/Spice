using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //GET List All
        public async Task<IActionResult> Index()
        {
            return View(await _db.Category.ToListAsync());
        }

        //Get Create
        public IActionResult Create()
        {
            return View();
        }

        //Post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if(ModelState.IsValid)
            {
                 _db.Category.Add(category);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
                //return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        //GET EDIT
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id==null)
            {
                return NotFound();
            }
            Category category = await _db.Category.FindAsync(Id);
            if(category==null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Update(category);
              await  _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        //GET DELETE
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            Category category = await _db.Category.FindAsync(Id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? Id)
        {
            if (Id == null)
            { return NotFound(); }
            Category category = await _db.Category.FindAsync(Id);
            if(category==null)
            {
                return NotFound();
            }
            _db.Category.Remove(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET DETAILS
        public async Task<IActionResult>Details(int? Id)
        {
            if(Id==null)
            {
                return NotFound();
            }
            Category category = await _db.Category.FindAsync(Id);
            if(category==null)
            {
                return NotFound();
            }
            return View(category);
        }

    }
}