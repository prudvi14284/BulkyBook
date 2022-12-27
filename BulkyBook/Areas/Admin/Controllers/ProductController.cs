using System;
using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

		public object Guide { get; private set; }

		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        //Index
        public IActionResult Index()
        {
            return View();
        }

        //Get Action Method Edit
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
				CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Id.ToString()
				}),
			};
			if (id == null || id == 0)
            {
                ////create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
                //update product
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id== id);
                return View(productVM);
            }   
        }

        //Post Action Method Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj,IFormFile file)
        {
			//Update Image
			if (ModelState.IsValid)
			{
				string wwwRootPath = _hostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString();
					var uploads = Path.Combine(wwwRootPath, @"images\products");
					var extension = Path.GetExtension(file.FileName);

					if (obj.Product.ImageUrl != null)
					{
						var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
					{
						file.CopyTo(fileStreams);
					}
					obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

				}
				if (obj.Product.Id == 0)
				{
					_unitOfWork.Product.Add(obj.Product);
				}
				else
				{
					_unitOfWork.Product.Update(obj.Product);
				}
				_unitOfWork.Save();
				TempData["success"] = "Product created successfully";
				return RedirectToAction("Index");
			}
			return View(obj);
		}

        //Get Action Method Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //var ProductFromDb = _db.Categories.Find(id);
            var ProductFromDb = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            //var ProductFromDb = _db.Categories.SingleOrDefault(u => u.Id == id);
            if (ProductFromDb == null)
            {
                return NotFound();
            }
            return View(ProductFromDb);
        }

        //Post Action Method Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }

        #region API CALLS
        [HttpGet]
		[HttpGet]
		public IActionResult GetAll()
		{
			var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
			return Json(new { data = productList });
		}
		#endregion
	}
}
