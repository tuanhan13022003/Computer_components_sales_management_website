﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TechShopSolution.ApiIntegration;
using TechShopSolution.ViewModels.Catalog.Customer;
using TechShopSolution.ViewModels.Location;

namespace TechShopSolution.AdminApp.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerApiClient _customerApiClient;
        public CustomerController(ICustomerApiClient customerApiClient)
        {
            _customerApiClient = customerApiClient;
        }
        public async Task<IActionResult> Index(string keyword, int pageIndex = 1, int pageSize = 10)
        {
            var request = new GetCustomerPagingRequest()
            {
                Keyword = keyword,
                PageIndex = pageIndex,
                PageSize = pageSize,
            };
            var data = await _customerApiClient.GetCustomerPagings(request);
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
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CustomerCreateRequest request)
        {
            if (!ModelState.IsValid)
                return View();
            var result = await _customerApiClient.CreateCustomer(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Thêm khách hàng thành công";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(request);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var result = await _customerApiClient.GetById(id);
            var latest_order = await _customerApiClient.GetLatestOrder(id, 20);
            if (!result.IsSuccess || result.ResultObject == null)
            {
                TempData["error"] = result.Message;
                return RedirectToAction("Index");
            }
            var updateRequest = new CustomerUpdateRequest()
            {
                Id = id,
                name = result.ResultObject.name,
                address = result.ResultObject.address,
                birthday = result.ResultObject.birthday,
                email = result.ResultObject.email,
                sex = result.ResultObject.sex,
                phone = result.ResultObject.phone,
                isActive = result.ResultObject.isActive
            };
            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            ViewBag.LatestOrde = latest_order;
            return View(updateRequest);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CustomerUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LatestOrde = await _customerApiClient.GetLatestOrder(request.Id, 20);
                return View(request);
            }
            var result = await _customerApiClient.UpdateCustomer(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Cập nhật khách hàng thành công";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(request);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateAddress(int id)
        {
            var result = await _customerApiClient.GetById(id);
            if (!result.IsSuccess || result.ResultObject == null)
                ModelState.AddModelError("", result.Message);
            var updateAddressRequest = new CustomerUpdateAddressRequest()
            {
                Id = id,
                City = null,
                District = null,
                House = null,
                Ward = null
            };
            return View(updateAddressRequest);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(CustomerUpdateAddressRequest request)
        {
            if (!ModelState.IsValid)
                return View();
            var result = await _customerApiClient.UpdateAddress(request);
            if (result.IsSuccess)
            {
                TempData["result"] = "Cập nhật địa chỉ thành công";
                return RedirectToAction(nameof(Update), new { id = request.Id });
            }
            ModelState.AddModelError("", result.Message);
            return View(request);
        }
        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id, string pageIndex)
        {
            var result = await _customerApiClient.ChangeStatus(id);
            if (result.IsSuccess)
            {
                TempData["result"] = "Thay đổi trạng thái thành công";
                return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
            }
            TempData["error"] = result.Message;
            return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
        }
        public async Task<IActionResult> Delete(int id, string pageIndex)
        {
            var result = await _customerApiClient.Delete(id);
            if (result.IsSuccess)
            {
                TempData["result"] = "Xóa khách hàng thành công";
                return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
            }
            TempData["error"] = result.Message;
            return RedirectToAction("Index", new { pageIndex = !string.IsNullOrWhiteSpace(pageIndex) ? int.Parse(pageIndex) : 1 });
        }
        public async Task<JsonResult> LoadProvince()
        {
            try
            {
                var result = await _customerApiClient.LoadProvince();
                if (result == null || !result.IsSuccess)
                {
                    return null;
                }
                return Json(new
                {
                    data = result.ResultObject,
                    status = true
                });
            }
            catch
            {
                return null;
            }
        }
        public async Task<JsonResult> LoadDistrict(int provinceID)
        {
            try
            {
                var result = await _customerApiClient.LoadDistrict(provinceID);
                if (result == null || !result.IsSuccess)
                {
                    return null;
                }
                return Json(new
                {
                    data = result.ResultObject,
                    status = true
                });
            }
            catch
            {
                return null;
            }
        }
        public async Task<JsonResult> LoadWard(int districtID)
        {
            try
            {
                var result = await _customerApiClient.LoadWard(districtID);
                if (result == null || !result.IsSuccess)
                {
                    return null;
                }
                return Json(new
                {
                    data = result.ResultObject,
                    status = true
                });
            }
            catch
            {
                return null;
            }
        }
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> VerifyEmail(string email, int Id)
        {
            if (await _customerApiClient.VerifyEmail(email) == false)
            {
                return Json($"Email {email} đã được sử dụng.");
            }
            return Json(true);
        }
    }
}
