﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechShopSolution.ApiIntegration;
using TechShopSolution.ViewModels.Website.Contact;

namespace TechShopSolution.AdminApp.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly IContactApiClient _contactApiClient;
        public ContactController(IContactApiClient contactApiClient)
        {
            _contactApiClient = contactApiClient;
        }
        public async Task<IActionResult> Detail()
        {
            var contact = await _contactApiClient.GetcontactInfos();
            var updateModel = new ContactUpdateRequest()
            {
                adress = contact.ResultObject.adress,
                imageBase64 = contact.ResultObject.company_logo,
                email = contact.ResultObject.email,
                company_name = contact.ResultObject.company_name,
                fax = contact.ResultObject.fax,
                hotline = contact.ResultObject.hotline,
                id = contact.ResultObject.id,
                phone = contact.ResultObject.phone,
                social_fb = contact.ResultObject.social_fb,
                social_instagram = contact.ResultObject.social_instagram,
                social_twitter = contact.ResultObject.social_twitter,
                social_youtube = contact.ResultObject.social_youtube
            };
            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            if (TempData["error"] != null)
            {
                ViewBag.ErrorMsg = TempData["error"];
            }
            return View(updateModel);
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(ContactUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return View("Detail",request);
            var result = await _contactApiClient.UpdateContact(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Cập nhật thành công";
                return RedirectToAction("Detail");
            }
            ModelState.AddModelError("", result.Message);
            return View("Detail", request);
        }
        public async Task<IActionResult> ListFeedbacks(string keyword, int pageIndex = 1, int pageSize = 10)
        {
            var request = new GetFeedbackPagingRequets()
            {
                Keyword = keyword,
                PageIndex = pageIndex,
                PageSize = pageSize,
            };
            var data = await _contactApiClient.GetFeedbackPagings(request);
            ViewBag.Keyword = keyword;
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
        public async Task<IActionResult> DetailFeedBack(int id)
        {
            var result = await _contactApiClient.GetFeedback(id);
            if (!result.IsSuccess || result.ResultObject == null)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("ListFeedbacks");
            }
            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            if (TempData["error"] != null)
            {
                ViewBag.ErrorMsg = TempData["error"];
            }
            return View(result.ResultObject);
        }
        [HttpGet]
        public async Task<IActionResult> FeedbackChangeStatus(int id)
        {
           
            var result = await _contactApiClient.ChangeFeedbackStatus(id);
            if (result.IsSuccess)
            {
                TempData["result"] = "Đánh đấu đã xem thành công";
                return RedirectToAction("DetailFeedBack", new { id = id });
            }
            TempData["error"] = result.Message;
            return RedirectToAction("DetailFeedBack", new { id = id });
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _contactApiClient.Delete(id);
            if (result.IsSuccess)
            {
                TempData["result"] = "Xóa Feedback thành công";
                return RedirectToAction("ListFeedbacks");
            }
            TempData["error"] = result.Message;
            return RedirectToAction("ListFeedbacks");
        }
    }
}
