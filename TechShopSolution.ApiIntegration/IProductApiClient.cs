﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechShopSolution.ViewModels.Catalog.Brand;
using TechShopSolution.ViewModels.Catalog.Category;
using TechShopSolution.ViewModels.Catalog.Product;
using TechShopSolution.ViewModels.Common;
using TechShopSolution.ViewModels.Website.Dashboard;

namespace TechShopSolution.ApiIntegration
{
    public interface IProductApiClient
    {
        Task<PagedResult<ProductViewModel>> GetProductPagings(GetProductPagingRequest request);
        Task<PagedResult<ProductViewModel>> GetProductPagingsWithMainImage(GetProductPagingRequest request);
        Task<PagedResult<ProductViewModel>> GetPublicProducts(GetPublicProductPagingRequest request);
        Task<ApiResult<bool>> CreateProduct(ProductCreateRequest request);
        Task<ApiResult<bool>> UpdateProduct(ProductUpdateRequest request);
        Task<ApiResult<bool>> ChangeStatus(int Id);
        Task<ApiResult<bool>> OffBestSeller(int Id);
        Task<ApiResult<bool>> OffFeautured(int Id);
        Task<ApiResult<bool>> Delete(int cusID);
        Task<ApiResult<bool>> DeleteImage(int id, string fileName);
        Task<ApiResult<ProductViewModel>> GetById(int id);
        Task<ApiResult<ProductViewModel>> GetPublicProductDetail(string slug, int? cus_id);
        Task<List<RatingViewModel>> GetRatingsProduct(string slug);
        Task<bool> isValidSlug(string Code, string slug);
        Task<PublicCayegoyProductsViewModel> GetHomeProducts(int id, int take);
        Task<List<CategoryViewModel>> GetAllCategory();
        Task<List<BrandViewModel>> GetAllBrand();
        Task<PublicProductsViewModel> GetFeaturedProducts(int take);
        Task<PublicProductsViewModel> GetBestSellerProducts(int take);
        Task<PublicProductsViewModel> GetProductsByCategory(int id, int take);
        Task<List<ProductOverViewModel>> GetProductsRelated(int id, int take);
        Task<List<ProductRankingViewModel>> GetProductViewRanking(int take);
        Task<List<ProductRankingViewModel>> GetProductMostSalesRanking(int take);
        Task<List<ProductRankingViewModel>> GetProductFavoriteRanking(int take);
    }
}
