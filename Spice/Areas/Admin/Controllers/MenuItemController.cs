using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]

    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnviornment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }
        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnviornment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Categories = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            return View(await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).ToListAsync());
        }

        //Get Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        //Post Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["ddlSubCategoryIdList"].ToString());

            if (ModelState.IsValid)
            {
                _db.MenuItem.Add(MenuItemVM.MenuItem);
                await _db.SaveChangesAsync();

                string webRootPath = _hostingEnviornment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);
                if (files.Count > 0)
                {
                    var upload = Path.Combine(webRootPath, "Images");
                    var extension = Path.GetExtension(files[0].FileName);
                    using (var fileStream = new FileStream(Path.Combine(upload + @"\" + MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                        menuItemFromDb.Image = @"\Images\" + MenuItemVM.MenuItem.Id + extension;
                    }
                }
                else
                {
                    var upload = Path.Combine(webRootPath + @"\Images\" + SD.DefaultFoodImage);
                    System.IO.File.Copy(upload, webRootPath + @"\Images\" + MenuItemVM.MenuItem.Id + ".png");
                    menuItemFromDb.Image = @"\Images\" + MenuItemVM.MenuItem.Id + ".png";
                }
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(MenuItemVM);
        }

        //Get Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            MenuItem model = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if(model==null)
            {
                return NotFound();
            }
        
            MenuItemVM.MenuItem = model;
            MenuItemVM.SubCategories = await _db.SubCategory.Where(m => m.CategoryId == model.CategoryId).ToListAsync();
            return View(MenuItemVM);
        }

        //Post Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["ddlSubCategoryIdList"].ToString());
        
            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategories = await _db.SubCategory.Where(m => m.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            if (ModelState.IsValid)
            {
                //_db.MenuItem.Update(MenuItemVM.MenuItem);
                //await _db.SaveChangesAsync();

                string webRootPath = _hostingEnviornment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);
                if (files.Count > 0)
                {
                    //New Image has been uploaded
                    var upload = Path.Combine(webRootPath, "Images");
                    var extension = Path.GetExtension(files[0].FileName);

                    //Delete Exiting file
                    var existingImage = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                    if (System.IO.File.Exists(existingImage))
                    {
                        System.IO.File.Delete(existingImage);
                    }

                  
                    using (var fileStream = new FileStream(Path.Combine(upload + @"\" + MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                        menuItemFromDb.Image = @"\Images\" + MenuItemVM.MenuItem.Id + extension;
                    }
                }
                menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
                menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
                menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
                menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
                menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
                menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(MenuItemVM);
        }

        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if(id==null)
            { return NotFound(); }

            MenuItem menuitem = await _db.MenuItem.FindAsync(id);
            if(menuitem==null)
            {
                return NotFound();
            }

            menuitem.Category= _db.Category.Where(m => m.Id == menuitem.CategoryId).FirstOrDefault();
            menuitem.SubCategory = _db.SubCategory.Where(m => m.Id == menuitem.SubCategoryId).FirstOrDefault();
            return View(menuitem);
        }

        //Get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItem model = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = model;
            MenuItemVM.SubCategories = await _db.SubCategory.Where(m => m.CategoryId == model.CategoryId).ToListAsync();
            return View(MenuItemVM);
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
            MenuItem model = await _db.MenuItem.FindAsync(id);

            if (model == null)
            {
                return NotFound();
            }
            string webRootPath = _hostingEnviornment.WebRootPath;
            var existingImage = Path.Combine(webRootPath, model.Image.TrimStart('\\'));

            if (System.IO.File.Exists(existingImage))
            {
                System.IO.File.Delete(existingImage);
            }
            _db.MenuItem.Remove(model);
              await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}