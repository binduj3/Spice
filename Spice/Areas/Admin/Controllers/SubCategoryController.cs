using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]

    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        [TempData]
        public string StatusMessage { get; set; }
        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //Get Index
        public async Task<IActionResult> Index()
        {
            return View(await _db.SubCategory.Include(s=>s.Category).ToListAsync());
        }

        //Get Create
        public async Task<IActionResult> Create()
        {
            SubCategoryandCategoryViewModel model = new SubCategoryandCategoryViewModel() {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

        //Post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryandCategoryViewModel model)
        {
            if(ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubCategoryExists.Count() > 0)
                {
                    StatusMessage = "Error : Sub Category exists under  " + doesSubCategoryExists.First().Category.Name + " category. Please use another name";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryandCategoryViewModel modelVm = new SubCategoryandCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s=>s.Name).Select(s => s.Name).Distinct().ToListAsync(),
                StatusMessage=StatusMessage
            };
            return View(modelVm);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await (from subCategory in _db.SubCategory
                                   where subCategory.CategoryId == id
                                   select subCategory).ToListAsync();
            return Json(new SelectList(subCategories,"Id","Name"));

        }

        //Get Edit
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.FindAsync(Id);
            if(subCategory==null)
            {
                return NotFound();
            }
            SubCategoryandCategoryViewModel model = new SubCategoryandCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

        //Post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryandCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubCategoryExists.Count() > 0)
                {
                    StatusMessage = "Error : Sub Category exists under  " + doesSubCategoryExists.First().Category.Name + " category. Please use another name";
                }
                else
                {
                    var subCategory = _db.SubCategory.Find(model.SubCategory.Id);
                    if(subCategory == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        subCategory.Name = model.SubCategory.Name;
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                
                }
            }
            SubCategoryandCategoryViewModel modelVm = new SubCategoryandCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(modelVm);
        }

        //Get Delete
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.FindAsync(Id);
            if (subCategory == null)
            {
                return NotFound();
            }
            SubCategoryandCategoryViewModel model = new SubCategoryandCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

        //Post Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(SubCategoryandCategoryViewModel model)
        {
                    if (model.SubCategory == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        _db.SubCategory.Remove(model.SubCategory);
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
        }

        //Get Details
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.FindAsync(Id);
            if (subCategory == null)
            {
                return NotFound();
            }
            SubCategoryandCategoryViewModel model = new SubCategoryandCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(model);
        }

    }
}