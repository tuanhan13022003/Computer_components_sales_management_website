﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TechShopSolution.ViewModels.Common;
using TechShopSolution.ViewModels.System;

namespace TechShopSolution.Application.System
{
    public interface IAdminService
    {
        string Authenticate(LoginRequest request);
        ApiResult<string> AuthenticateCustomer(LoginRequest request);
    }
}
