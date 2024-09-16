﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TechShopSolution.ViewModels.Common;
using TechShopSolution.ViewModels.Website.Contact;

namespace TechShopSolution.ApiIntegration
{
    public interface IContactApiClient
    {
        Task<ApiResult<ContactViewModel>> GetcontactInfos();
        Task<ApiResult<bool>> UpdateContact(ContactUpdateRequest request);
        Task<ApiResult<bool>> SendFeedback(FeedbackCreateRequest request);
        Task<ApiResult<FeedbackViewModel>> GetFeedback(int id);
        Task<ApiResult<bool>> Delete(int id);
        Task<PagedResult<FeedbackViewModel>> GetFeedbackPagings(GetFeedbackPagingRequets request);
        Task<ApiResult<bool>> ChangeFeedbackStatus(int Id);
    }
}
