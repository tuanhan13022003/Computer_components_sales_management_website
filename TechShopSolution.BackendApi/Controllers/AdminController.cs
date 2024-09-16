﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechShopSolution.Application.System;
using TechShopSolution.ViewModels.System;

namespace TechShopSolution.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpPost("AdminLogin")]
        public IActionResult Authenticate([FromBody] LoginRequest request)
        {
            if (ModelState.IsValid)
            {
                string resultToken =  _adminService.Authenticate(request);
                if (!string.IsNullOrEmpty(resultToken))
                {
                    return Ok(resultToken);
                }
                else return BadRequest("Sai thông tin đăng nhập");
            }
            return BadRequest(ModelState);
        }
        [HttpPost("CustomerLogin")]
        public IActionResult AuthenticateCustomer([FromBody] LoginRequest request)
        {
            var result = _adminService.AuthenticateCustomer(request);
            if (result.IsSuccess)
                return Ok(result.ResultObject);
            else return BadRequest(result.Message);
        }
    }
}
