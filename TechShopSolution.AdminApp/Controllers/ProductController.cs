﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechShopSolution.ViewModels.Catalog.Product;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using TechShopSolution.ApiIntegration;
using TechShopSolution.ViewModels.Catalog.Category;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechShopSolution.AdminApp.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductApiClient _productApiClient;
        [Obsolete]
        private readonly IHostingEnvironment _environment;

        [Obsolete]
        public ProductController(IProductApiClient productApiClient, IHostingEnvironment environment)
        {
            _productApiClient = productApiClient;
            _environment = environment;
        }
        public async Task<IActionResult> Index(string keyword, int? CategoryID, int? BrandID, int pageIndex = 1, int pageSize = 10)
        {
            var categoryList = await _productApiClient.GetAllCategory();
            var request = new GetProductPagingRequest()
            {
                Keyword = keyword,
                BrandID = BrandID,
                CategoryID = CategoryID,
                PageIndex = pageIndex,
                PageSize = pageSize,
            };
            var data = await _productApiClient.GetProductPagings(request);

            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            if (TempData["error"] != null)
            {
                ViewBag.ErrorMsg = TempData["error"];
            }
            var Categories = await OrderCateToTree(categoryList);
            var Brands = await _productApiClient.GetAllBrand();
            ViewBag.Brands = Brands.Select(x => new SelectListItem()
            {
                Text = x.brand_name,
                Value = x.id.ToString(),
                Selected = BrandID != null && x.id == BrandID
            });
            ViewBag.Categories = Categories.Select(x => new SelectListItem()
            {
                Text = x.cate_name,
                Value = x.id.ToString(),
                Selected = CategoryID != null && x.id == CategoryID
            });
            return View(data);
        }
        public async Task<List<CategoryViewModel>> OrderCateToTree(List<CategoryViewModel> lst, int parent_id = 0, int level = 0)
        {
            if (lst != null)
            {
                List<CategoryViewModel> result = new List<CategoryViewModel>();
                foreach (CategoryViewModel cate in lst)
                {
                    if (cate.parent_id == parent_id)
                    {
                        CategoryViewModel tree = new CategoryViewModel();
                        tree = cate;
                        tree.level = level;
                        tree.cate_name = String.Concat(Enumerable.Repeat("|————", level)) + tree.cate_name;

                        result.Add(tree);
                        List<CategoryViewModel> child = await OrderCateToTree(lst, cate.id, level + 1);
                        result.AddRange(child);
                    }
                }
                return result;
            }
            return null;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categoryList = await _productApiClient.GetAllCategory();
            ViewBag.ListCate = await OrderCateToTree(categoryList);
            ViewBag.ListBrand = await _productApiClient.GetAllBrand();
            return View();
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var categoryList = await _productApiClient.GetAllCategory();
                ViewBag.ListCate = await OrderCateToTree(categoryList);
                ViewBag.ListBrand = await _productApiClient.GetAllBrand();
                return View(request);
            }
            var result = await _productApiClient.CreateProduct(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(request);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var result = await Task.Run(() => _productApiClient.GetById(id));
            if (!result.IsSuccess || result.ResultObject == null)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index");
            }
            var updateRequest = new ProductUpdateRequest()
            {
                Id = result.ResultObject.id,
                Best_seller = result.ResultObject.best_seller,
                Brand_id = result.ResultObject.brand_id,
                CateID = result.ResultObject.CateID,
                Code = result.ResultObject.code,
                Descriptions = result.ResultObject.descriptions,
                Featured = result.ResultObject.featured,
                Instock = result.ResultObject.instock,
                IsActive = result.ResultObject.isActive,
                more_image_name = result.ResultObject.more_images,
                image_name = result.ResultObject.image,
                Meta_descriptions = result.ResultObject.meta_descriptions,
                Meta_keywords = result.ResultObject.meta_keywords,
                Meta_tittle = result.ResultObject.meta_tittle,
                Name = result.ResultObject.name,
                Promotion_price = result.ResultObject.promotion_price.ToString(),
                Short_desc = result.ResultObject.short_desc,
                Slug = result.ResultObject.slug,
                Specifications = result.ResultObject.specifications,
                Unit_price = result.ResultObject.unit_price.ToString(),
                Warranty = result.ResultObject.warranty
            };
            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            var categoryList = await _productApiClient.GetAllCategory();
            ViewBag.ListCate = await OrderCateToTree(categoryList);
            ViewBag.ListBrand = await _productApiClient.GetAllBrand();
            return View(updateRequest);
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Update", request.Id);
            var result = await _productApiClient.UpdateProduct(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(request);
        }
        public async Task sendListMoreImage(List<IFormFile> files)
        {
            if (files != null)
            {
                foreach (IFormFile image in files)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\assets\ProductImage", image.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id, string pageIndex, string keyword, int? CategoryID, int? BrandID)
        {
            var result = await _productApiClient.ChangeStatus(id);
            if (!result.IsSuccess)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index",
                    new
                    {
                        pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1,
                        keyword = keyword,
                        CategoryID = CategoryID,
                        BrandID = BrandID
                    });
            }
            else
            {
                TempData["result"] = "Thay đổi trạng thái thành công";
                return RedirectToAction("Index",
                     new
                     {
                         pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1,
                         keyword = keyword,
                         CategoryID = CategoryID,
                         BrandID = BrandID
                     });
            }
        }
        [HttpGet]
        public async Task<IActionResult> OffBestSeller(int id, string pageIndex, string keyword, int? CategoryID, int? BrandID)
        {
            var result = await _productApiClient.OffBestSeller(id);
            if (!result.IsSuccess)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index",
                    new
                    {
                        pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1,
                        keyword = keyword,
                        CategoryID = CategoryID,
                        BrandID = BrandID
                    });
            }
            TempData["result"] = "Thay đổi trạng thái thành công";
            return RedirectToAction("Index",
                    new
                    {
                        pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1,
                        keyword = keyword,
                        CategoryID = CategoryID,
                        BrandID = BrandID
                    });
        }
        [HttpGet]
        public async Task<IActionResult> OffFeatured(int id, string pageIndex, string keyword, int? CategoryID, int? BrandID)
        {
            var result = await _productApiClient.OffFeautured(id);
            if (!result.IsSuccess)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index",
                   new
                   {
                       pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1,
                       keyword = keyword,
                       CategoryID = CategoryID,
                       BrandID = BrandID
                   });
            }
            TempData["result"] = "Thay đổi trạng thái thành công";
            return RedirectToAction("Index",
                   new
                   {
                       pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1,
                       keyword = keyword,
                       CategoryID = CategoryID,
                       BrandID = BrandID
                   });
        }
        [HttpPost]
        public async Task<JsonResult> DeleteImage(int id, string fileName)
        {
            var result = await _productApiClient.DeleteImage(id, fileName);
            if (result.IsSuccess)
                return Json(new { success = true, message = "Xóa hình ảnh thành công" });
            return Json(new { success = false, message = result.Message });
        }
        public async Task<IActionResult> Delete(int id, string pageIndex)
        {
            var result = await _productApiClient.Delete(id);
            if (result == null)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
            }
            else
            {
                TempData["result"] = "Xóa sản phẩm thành công";
                return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
            }
        }
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> isValidSlug(string code, string slug)
        {
            if (await _productApiClient.isValidSlug(code, slug) == false)
            {
                return Json($"Đường dẫn {slug} đã được sử dụng.");
            }
            return Json(true);
        }
    }
}
