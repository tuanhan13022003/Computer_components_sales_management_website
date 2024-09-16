﻿using System.Threading.Tasks;
using TechShopSolution.ViewModels.Common;
using TechShopSolution.ViewModels.Sales;
using TechShopSolution.ViewModels.Website.Dashboard;

namespace TechShopSolution.ApiIntegration
{
    public interface IOrderApiClient
    {
        Task<ApiResult<string>> CreateOrder(CheckoutRequest request);
        Task<PagedResult<OrderViewModel>> GetOrderPagings(GetOrderPagingRequest request);
        Task<ApiResult<OrderDetailViewModel>> GetById(int id);
        Task<ApiResult<string>> PaymentConfirm(int id);
        Task<ApiResult<string>> CancelOrder(OrderCancelRequest request);
        Task<ApiResult<string>> ConfirmOrder(int id);
        Task<ApiResult<bool>> UpdateAddress(OrderUpdateAddressRequest request);
        Task<ApiResult<OrderStatisticsViewModel>> GetOrderStatistics();
    }
}
