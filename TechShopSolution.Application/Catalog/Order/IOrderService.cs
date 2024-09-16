﻿using System.Threading.Tasks;
using TechShopSolution.ViewModels.Common;
using TechShopSolution.ViewModels.Sales;
using TechShopSolution.ViewModels.Website.Dashboard;

namespace TechShopSolution.Application.Catalog.Order
{
    public interface IOrderService
    {
        Task<ApiResult<string>> Create(CheckoutRequest request);
        PagedResult<OrderViewModel> GetAllPaging(GetOrderPagingRequest request);
        Task<ApiResult<OrderDetailViewModel>> Detail(int id);
        Task<ApiResult<string>> CancelOrder(OrderCancelRequest request);
        Task<ApiResult<string>> PaymentConfirm(int id);
        Task<ApiResult<string>> ConfirmOrder(int id);
        Task<ApiResult<bool>> UpdateAddress(OrderUpdateAddressRequest request);
        PagedResult<OrderPublicViewModel> GetCustomerOrders(GetCustomerOrderRequest request);
        ApiResult<OrderPublicViewModel> GetDetailOrder(int id, int cus_id );
        ApiResult<OrderStatisticsViewModel> GetOrderStatistics();
    }
}
