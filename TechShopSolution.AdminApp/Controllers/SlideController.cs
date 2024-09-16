﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechShopSolution.ApiIntegration;
using TechShopSolution.ViewModels.Common;
using TechShopSolution.ViewModels.Website;
using TechShopSolution.ViewModels.Website.Slide;

namespace TechShopSolution.AdminApp.Controllers
{
    [Authorize]
    public class SlideController : Controller
    {
        private readonly ISlideApiClient _slideApiClient;
        public SlideController(ISlideApiClient slideApiClient)
        {
            _slideApiClient = slideApiClient;
        }
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10)
        {
            var request = new PagingRequestBase()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
            };
            var data = await _slideApiClient.GetSlidePagings(request);
            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            if (TempData["error"] != null)
            {
                ViewBag.ErrorMsg = TempData["error"];
            }
            return View(data);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var result = await _slideApiClient.GetById(id);
            if (!result.IsSuccess || result.ResultObject == null)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index");
            }
            var updateRequest = new SlideUpdateRequest()
            {
                id = id,
                status = result.ResultObject.status,
                link = result.ResultObject.link,
                display_order = result.ResultObject.display_order,
                imageBase64 = result.ResultObject.image
            };
            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            return View(updateRequest);
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(SlideUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);
            var result = await _slideApiClient.UpdateSlide(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Cập nhật thành công";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(request);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(SlideCreateRequest request)
        {
            if (!ModelState.IsValid)
                return View();
            var result = await _slideApiClient.CreateSlide(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Thêm thành công";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(request);
        }
        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id, string pageIndex)
        {
            var result = await _slideApiClient.ChangeStatus(id);
            if (!result.IsSuccess)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
            }
            else
            {
                TempData["result"] = "Thay đổi trạng thái thành công";
                return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
            }
        }
        public async Task<IActionResult> Delete(int id, string pageIndex)
        {
            var result = await _slideApiClient.DeleteSlide(id);
            if (result.IsSuccess)
            {
                TempData["result"] = "Xóa thành công";
                return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
            }
            TempData["error"] = result.Message;
            return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
        }
    }
}
