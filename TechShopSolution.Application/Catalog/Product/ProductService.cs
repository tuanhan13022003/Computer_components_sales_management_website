﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechShopSolution.ViewModels.Catalog.Product;
using TechShopSolution.Data.EF;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.IO;
using TechShopSolution.Application.Common;
using TechShopSolution.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using TechShopSolution.ViewModels.Catalog.Category;
using TechShopSolution.ViewModels.Website.Dashboard;

namespace TechShopSolution.Application.Catalog.Product
{
    public class ProductService : IProductService
    {
        private readonly TechShopDBContext _context;
        private readonly IStorageService _storageService;
        public ProductService(TechShopDBContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }
        public async Task<ApiResult<bool>> Create(ProductCreateRequest request)
        {
           
            
            var product = new TechShopSolution.Data.Entities.Product
            {
                name = request.Name,
                best_seller = request.Best_seller,
                brand_id = request.Brand_id,
                code = request.Code,
                create_at = DateTime.Now,
                descriptions = request.Descriptions,
                featured = request.Featured,
                instock = request.Instock,
                meta_descriptions = request.Meta_descriptions,
                meta_keywords = request.Meta_keywords,
                meta_tittle = request.Meta_tittle,
                promotion_price = !string.IsNullOrWhiteSpace(request.Promotion_price) ? decimal.Parse(request.Promotion_price) : 0,
                short_desc = request.Short_desc,
                slug = request.Slug,
                specifications = request.Specifications,
                isActive = request.IsActive,
                isDelete = false,
                unit_price = decimal.Parse(request.Unit_price),
                warranty = request.Warranty != null ? (int)request.Warranty : 0,
            };
            if (request.Image != null)
            {
                product.image = await this.SaveFile(request.Image);
            }
            if (request.More_images != null)
            {
                if (request.More_images.Count == 1)
                    product.more_images = await this.SaveFile(request.More_images[0]);
                else
                {
                    for (int i = 0; i < request.More_images.Count(); i++)
                    {
                        product.more_images += await this.SaveFile(request.More_images[i]) + ",";
                    }
                }
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            string[] cateIDs = request.CateID.Split(",");
            foreach (string cateID in cateIDs)
            {
                if (cateID != "")
                {
                    var productInCategory = new TechShopSolution.Data.Entities.CategoryProduct
                    {
                        cate_id = int.Parse(cateID),
                        product_id = product.id
                    };
                    _context.CategoryProducts.Add(productInCategory);
                }
            }
            await _context.SaveChangesAsync();
            return new ApiSuccessResult<bool>();
        }
        public async Task<ApiResult<bool>> Delete(int productID)
        {
            try
            {
                var product = await _context.Products.FindAsync(productID);
                if (product == null)
                {
                     return new ApiErrorResult<bool>($"Sản phẩm không tồn tại");
                }
                if (await _context.OrDetails.AnyAsync(x => x.product_id == productID))
                {
                    product.isDelete = true;
                    product.delete_at = DateTime.Now;

                    if (product.more_images != null)
                    {
                        List<string> moreImages = product.more_images.Split(",").ToList();
                        foreach (string img in moreImages)
                        {
                            await _storageService.DeleteFileAsync(img);
                        }
                        product.more_images = "";
                    }

                    var RatingItems = await _context.Ratings.Where(x => x.product_id == productID).ToListAsync();
                    if (RatingItems != null)
                    {
                        _context.Ratings.RemoveRange(RatingItems);
                    }

                    var FavoriteItems = await _context.Favorites.Where(x => x.product_id == productID).ToListAsync();
                    if (FavoriteItems != null)
                    {
                        _context.Favorites.RemoveRange(FavoriteItems);
                    }
                }
                else
                {
                    await _storageService.DeleteFileAsync(product.image);

                    if (product.more_images != null)
                    {
                        List<string> moreImages = product.more_images.Split(",").ToList();
                        foreach (string img in moreImages)
                        {
                            await _storageService.DeleteFileAsync(img);
                        }
                        product.more_images = "";
                    }

                    var RatingItems = await _context.Ratings.Where(x => x.product_id == productID).ToListAsync();
                    if (RatingItems != null)
                    {
                        _context.Ratings.RemoveRange(RatingItems);
                    }

                    var FavoriteItems = await _context.Favorites.Where(x => x.product_id == productID).ToListAsync();
                    if (FavoriteItems != null)
                    {
                        _context.Favorites.RemoveRange(FavoriteItems);
                    }
                    _context.Products.Remove(product);
                }
               
                await _context.SaveChangesAsync();
                return new ApiSuccessResult<bool>();
            }
            catch (Exception ex)
            {
                return new ApiErrorResult<bool>(ex.Message);
            }
        }
        public async Task<ApiResult<bool>> DeleteImage(int id, string fileName)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                List<string> moreImagesName = product.more_images.Split(",").ToList();
                bool flag = false;
                foreach (string moreimage in moreImagesName)
                {
                    if (moreimage != "")
                    {
                        if (moreimage.Equals(fileName))
                        {
                            moreImagesName.Remove(moreimage);
                            string MoreImageAfterDelete = null;
                            foreach (string imagestr in moreImagesName)
                            {
                                if (imagestr != "")
                                {
                                    MoreImageAfterDelete += imagestr + ",";
                                }
                            }
                            product.more_images = MoreImageAfterDelete;
                            product.update_at = DateTime.Now;
                            var result = await _storageService.DeleteFileAsync(fileName);
                            await _context.SaveChangesAsync();
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                    return new ApiErrorResult<bool>("Hình này không tồn tại trong CSDL của sản phẩm");
                else return new ApiSuccessResult<bool>();
            }
            return new ApiErrorResult<bool>("Sản phẩm không tồn tại");
        }
        public PagedResult<ProductOverViewModel> GetAllPaging(GetProductPagingRequest request)
        {
            try
            {
                var query = from p in _context.Products
                            join pic in _context.CategoryProducts on p.id equals pic.product_id
                            where p.isDelete == false
                            select new { p, pic };

                if (!String.IsNullOrEmpty(request.Keyword))
                    query = query.Where(x => x.p.name.Contains(request.Keyword));

                if (request.CategoryID != null)
                {
                    query = query.Where(x => x.pic.cate_id == request.CategoryID);
                }

                if (request.BrandID != null)
                {
                    query = query.Where(x => x.p.brand_id == request.BrandID);
                }

                var data = query.AsEnumerable()
                   .OrderByDescending(m => m.p.create_at)
                   .GroupBy(g => g.p);

                int totalRow = data.Count();

                List<ProductOverViewModel> result = data.Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(a => new ProductOverViewModel()
                    {
                        id = a.Key.id,
                        name = a.Key.name,
                        best_seller = a.Key.best_seller,
                        isActive = a.Key.isActive,
                        create_at = a.Key.create_at,
                        featured = a.Key.featured,
                        image = a.Key.image,
                        instock = a.Key.instock,
                        promotion_price = a.Key.promotion_price,
                        short_desc = a.Key.short_desc,
                        slug = a.Key.slug,
                        unit_price = a.Key.unit_price,
                    }).ToList();

                var pageResult = new PagedResult<ProductOverViewModel>()
                {
                    TotalRecords = totalRow,
                    PageSize = request.PageSize,
                    PageIndex = request.PageIndex,
                    Items = result,
                };
                return pageResult;
            }
            catch
            {
                var pageResult = new PagedResult<ProductOverViewModel>()
                {
                    TotalRecords = 0,
                    PageSize = request.PageSize,
                    PageIndex = request.PageIndex,
                    Items = null,
                };
                return pageResult;
            }
        }
        public PagedResult<ProductOverViewModel> GetPublicProducts(GetPublicProductPagingRequest request)
        {
            try
            {
                var query = from p in _context.Products
                            join pic in _context.CategoryProducts on p.id equals pic.product_id
                            join c in _context.Categories on pic.cate_id equals c.id
                            where p.isDelete == false
                            select new { p, pic, c };

                if (!String.IsNullOrEmpty(request.Keyword))
                    query = query.Where(x => EF.Functions.Like(x.p.name, $"%{request.Keyword}%"));

                if (!String.IsNullOrEmpty(request.CategorySlug))
                    query = query.Where(x => x.c.cate_slug.Equals(request.CategorySlug));

                if (!String.IsNullOrEmpty(request.BrandSlug))
                {
                    query = query.Where(x => x.p.Brand.brand_slug.Equals(request.BrandSlug));
                }
                switch (request.idSortType)
                {
                    case 1:
                        query = query.OrderBy(x => x.p.name);
                        break;
                    case 2:
                        query = query.OrderByDescending(x => x.p.name);
                        break;
                    case 3:
                        query = query.OrderBy(x => x.p.promotion_price > 0 ? x.p.promotion_price : x.p.unit_price);
                        break;
                    case 4:
                        query = query.OrderByDescending(x => x.p.promotion_price > 0 ? x.p.promotion_price : x.p.unit_price);
                        break;
                }
                if (request.Lowestprice != null && request.Highestprice != null)
                {
                    query = query.Where(x => (x.p.promotion_price > 0 ? x.p.promotion_price : x.p.unit_price) >= request.Lowestprice 
                    && (x.p.promotion_price > 0 ? x.p.promotion_price : x.p.unit_price) <= request.Highestprice);
                } else if (request.Lowestprice != null && request.Highestprice == null)
                {
                    query = query.Where(x => (x.p.promotion_price > 0 ? x.p.promotion_price : x.p.unit_price) >= request.Lowestprice);
                } else if (request.Lowestprice == null && request.Highestprice != null)
                {
                    query = query.Where(x => (x.p.promotion_price > 0 ? x.p.promotion_price : x.p.unit_price) <= request.Highestprice);
                }


                var data = query.AsEnumerable()
                    .GroupBy(g => g.p);

                int totalRow = data.Count();

                List<ProductOverViewModel> result = data.Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(a => new ProductOverViewModel()
                    {
                        id = a.Key.id,
                        name = a.Key.name,
                        best_seller = a.Key.best_seller,
                        create_at = a.Key.create_at,
                        featured = a.Key.featured,
                        image = a.Key.image,
                        instock = a.Key.instock,
                        promotion_price = a.Key.promotion_price,
                        short_desc = a.Key.short_desc,
                        slug = a.Key.slug,
                        isActive = a.Key.isActive,
                        unit_price = a.Key.unit_price,
                    }).ToList();


                var pageResult = new PagedResult<ProductOverViewModel>()
                {
                    TotalRecords = totalRow,
                    PageSize = request.PageSize,
                    PageIndex = request.PageIndex,
                    Items = result,
                };
                return pageResult;
            }
            catch
            {
                var pageResult = new PagedResult<ProductOverViewModel>()
                {
                    TotalRecords = 0,
                    PageSize = request.PageSize,
                    PageIndex = request.PageIndex,
                    Items = null,
                };
                return pageResult;
            }
        }
        public async Task<PublicProductsViewModel> GetFeaturedProduct(int take)
        {
            try
            {
                var query = from p in _context.Products
                            where p.isDelete == false && p.featured == true && p.isActive == true
                            select new { p };

                int Count = await query.CountAsync();

                var data = query.OrderBy(emp => Guid.NewGuid())
                    .Take(take)
                    .Select(a => new ProductOverViewModel()
                    {
                        id = a.p.id,
                        name = a.p.name,
                        best_seller = a.p.best_seller,
                        featured = a.p.featured,
                        image = a.p.image,
                        instock = a.p.instock,
                        promotion_price = a.p.promotion_price,
                        short_desc = a.p.short_desc,
                        slug = a.p.slug,
                        unit_price = a.p.unit_price,
                    }).ToListAsync();


                return new PublicProductsViewModel { Count = Count, Products = await data };
            }
            catch
            {
                return new PublicProductsViewModel { Count = 0, Products = null };
            }
        }
        public async Task<PublicProductsViewModel> GetProductsByCategory(int id, int take)
        {
            try
            {
                var query = from p in _context.Products
                            join pic in _context.CategoryProducts on p.id equals pic.product_id
                            join c in _context.Categories on pic.cate_id equals c.id
                            where p.isDelete == false && c.id == id
                            select new { p, pic, c };

                int Count = await query.CountAsync();

                var data = query.OrderBy(emp => Guid.NewGuid())
                    .Take(take)
                    .Select(a => new ProductOverViewModel()
                    {
                        id = a.p.id,
                        name = a.p.name,
                        best_seller = a.p.best_seller,
                        create_at = a.p.create_at,
                        featured = a.p.featured,
                        image = a.p.image,
                        instock = a.p.instock,
                        promotion_price = a.p.promotion_price,
                        short_desc = a.p.short_desc,
                        slug = a.p.slug,
                        unit_price = a.p.unit_price,
                    }).ToListAsync();


                return new PublicProductsViewModel { Count = Count, Products = await data };
            }
            catch
            {
                return new PublicProductsViewModel { Count = 0, Products = null };
            }
        }
        public async Task<PublicCayegoyProductsViewModel> GetHomeProductByCategory(int id, int take)
        {
            try
            {
                var query = from p in _context.Products
                            join pic in _context.CategoryProducts on p.id equals pic.product_id
                            join c in _context.Categories on pic.cate_id equals c.id 
                            where p.isDelete == false && c.id == id && p.isActive == true && p.instock != 0
                            select new { p, c };

                var category = await query.Select(a => new CategoryViewModel()
                {
                    id = a.c.id,
                    cate_name = a.c.cate_name,
                    cate_slug = a.c.cate_slug,
                    meta_descriptions = a.c.meta_descriptions,
                    meta_keywords = a.c.meta_keywords,
                    meta_title = a.c.meta_title,
                    create_at = a.c.create_at,
                    parent_id = a.c.parent_id,
                    isActive = a.c.isActive,
                }).FirstAsync();

                var data = query.AsEnumerable()
                   .GroupBy(g => g.p);

                int totalRow = data.Count();

                int Count = await query.CountAsync();

                List<ProductOverViewModel> Products = data.OrderBy(emp => Guid.NewGuid())
                    .Take(take)
                    .Select(a => new ProductOverViewModel()
                    {
                        id = a.Key.id,
                        name = a.Key.name,
                        best_seller = a.Key.best_seller,
                        featured = a.Key.featured,
                        image = a.Key.image,
                        create_at = a.Key.create_at,
                        instock = a.Key.instock,
                        promotion_price = a.Key.promotion_price,
                        short_desc = a.Key.short_desc,
                        slug = a.Key.slug,
                        unit_price = a.Key.unit_price,
                    }).ToList();

                return new PublicCayegoyProductsViewModel { Count = Count, Products = Products, Category = category };
            }
            catch
            {
                return new PublicCayegoyProductsViewModel { Count = 0, Products = null, Category = null };
            }
        }
        public List<ProductOverViewModel> GetProductsRelated(int idBrand, int take)
        {
            try
            {
                var query = from p in _context.Products
                            join pic in _context.CategoryProducts on p.id equals pic.product_id
                            where p.isDelete == false && p.brand_id == idBrand && p.isActive == true
                            select new { p };

                var group = query.AsEnumerable()
                   .GroupBy(g => g.p);

                var data = group.OrderBy(emp => Guid.NewGuid())
                    .Take(take)
                    .Select(a => new ProductOverViewModel()
                    {
                        id = a.Key.id,
                        name = a.Key.name,
                        best_seller = a.Key.best_seller,
                        featured = a.Key.featured,
                        image = a.Key.image,
                        promotion_price = a.Key.promotion_price,
                        slug = a.Key.slug,
                        create_at = a.Key.create_at,
                        unit_price = a.Key.unit_price,
                    }).ToList();

                return data;
            }
            catch
            {
                return null;
            }
        }
        public async Task<PublicProductsViewModel> GetBestSellerProduct(int take)
        {
            try
            {
                var query = from p in _context.Products
                            where p.isDelete == false && p.best_seller == true && p.isActive == true && p.isActive == true
                            select new { p };

                int Count = await query.CountAsync();

                var data = query.OrderBy(emp => Guid.NewGuid())
                    .Take(take)
                    .Select(a => new ProductOverViewModel()
                    {
                        id = a.p.id,
                        name = a.p.name,
                        best_seller = a.p.best_seller,
                        create_at = a.p.create_at,
                        featured = a.p.featured,
                        image = a.p.image,
                        instock = a.p.instock,
                        promotion_price = a.p.promotion_price,
                        short_desc = a.p.short_desc,
                        slug = a.p.slug,
                        unit_price = a.p.unit_price,
                    }).ToListAsync();


                return new PublicProductsViewModel { Count = Count, Products = await data };

            }
            catch
            {
                return new PublicProductsViewModel { Count = 0, Products = null };
            }
        }
        public async Task<ApiResult<ProductViewModel>> GetById(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.isDelete)
            {
                return new ApiErrorResult<ProductViewModel>("Sản phẩm không tồn tại hoặc đã bị xóa");
            }
            string CateIds = "";
            var pic = await _context.CategoryProducts.Where(x => x.product_id == productId).ToListAsync();
            if (pic != null)
            {
                foreach (var cate in pic)
                {
                    CateIds += cate.cate_id + ",";
                }
            }

            var productViewModel = new ProductViewModel
            {
                id = product.id,
                name = product.name,
                best_seller = product.best_seller,
                brand_id = product.brand_id,
                CateID = CateIds,
                code = product.code,
                create_at = product.create_at,
                descriptions = product.descriptions,
                featured = product.featured,
                image = product.image,
                instock = product.instock,
                meta_descriptions = product.meta_descriptions,
                meta_keywords = product.meta_keywords,
                meta_tittle = product.meta_tittle,
                more_images = product.more_images,
                promotion_price = product.promotion_price,
                short_desc = product.short_desc,
                slug = product.slug,
                specifications = product.specifications,
                isActive = product.isActive,
                unit_price = product.unit_price,
                warranty = product.warranty,
            };
            return new ApiSuccessResult<ProductViewModel>(productViewModel);
        }
        public async Task<ApiResult<ProductViewModel>> GetPublicProductDetail(string slug, int? cus_id)
        {

            var query = from p in _context.Products
                        join pic in _context.CategoryProducts on p.id equals pic.product_id
                        join c in _context.Categories on pic.cate_id equals c.id
                        join b in _context.Brands on p.brand_id equals b.id
                        where p.isDelete == false && p.slug == slug
                        select new { p, pic, c, b};

            if (await query.CountAsync() == 0)
            {
                return new ApiErrorResult<ProductViewModel>("Sản phẩm không tồn tại hoặc đã bị xóa");
            }

            var dataProduct = query.AsEnumerable()
               .GroupBy(g => g.p);


            ProductViewModel Pro = dataProduct.Select(x => new ProductViewModel()
            {
                best_seller = x.Key.best_seller,
                slug = x.Key.slug,
                brand_id = x.Key.brand_id,
                brand_name = x.Key.Brand.brand_name,
                CateID = x.Key.ProductInCategory.First().Category.id.ToString(),
                cate_name = x.Key.ProductInCategory.First().Category.cate_name.ToString(),
                cate_slug = x.Key.ProductInCategory.First().Category.cate_slug.ToString(),
                code = x.Key.code,
                image = x.Key.image,
                create_at = x.Key.create_at,
                descriptions = x.Key.descriptions,
                featured = x.Key.featured,
                id = x.Key.id,
                instock = x.Key.instock,
                view_count = x.Key.view_count,
                isActive = x.Key.isActive,
                meta_descriptions = x.Key.meta_descriptions,
                meta_keywords = x.Key.meta_keywords,
                meta_tittle = x.Key.meta_tittle,
                more_images = x.Key.more_images,
                name = x.Key.name,
                promotion_price = x.Key.promotion_price,
                short_desc = x.Key.short_desc,
                specifications = x.Key.specifications,
                unit_price = x.Key.unit_price,
                warranty = x.Key.warranty
            }).FirstOrDefault();


            if (cus_id != null)
            {
                if (await _context.Favorites.AnyAsync(x => x.cus_id == cus_id && x.product_id == Pro.id))
                    Pro.isFavorite = true;
                else Pro.isFavorite = false;
            } else Pro.isFavorite = false;
            Pro.favorite_count = await _context.Favorites.Where(x => x.product_id == Pro.id).CountAsync();

            var product = await _context.Products.FindAsync(Pro.id);
            product.view_count = product.view_count + 1;
            await _context.SaveChangesAsync();

            return new ApiSuccessResult<ProductViewModel>(Pro);
        }
        public List<RatingViewModel> GetRatingsProduct(string slug)
        {
            try
            {
                var query = from r in _context.Ratings
                             join c in _context.Customers on r.cus_id equals c.id
                             join p in _context.Products on r.product_id equals p.id
                             where p.isDelete == false && p.isActive == true && p.slug == slug
                             select new { p, c, r };

                var dataRating = query.AsEnumerable()
                  .GroupBy(g => g.r);

                List<RatingViewModel> ListRating = dataRating.OrderByDescending(x => x.Key.date_rating)
                    .Select(x => new RatingViewModel()
                    {
                        cus_id = x.Key.cus_id,
                        content = x.Key.content,
                        cus_name = x.Key.Customer.name,
                        date_rating = x.Key.date_rating,
                        product_id = x.Key.product_id,
                        score = x.Key.score
                    }).ToList();

                return ListRating;

            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> isValidSlug(string Code, string slug)
        {
            if (await _context.Products.AnyAsync(x => x.slug.Equals(slug) && !x.code.Equals(Code) && x.isDelete == false))
                return false;
            return true;
        }
        public async Task<ApiResult<bool>> Update(ProductUpdateRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(request.Id);
                if (product == null || product.isDelete) return new ApiErrorResult<bool>($"Không tìm thấy sản phẩm: {request.Id}");

                product.name = request.Name;
                product.slug = request.Slug;
                product.warranty = request.Warranty != null ? (int)request.Warranty : 0;
                product.brand_id = request.Brand_id;
                product.specifications = request.Specifications;
                product.short_desc = request.Short_desc;
                product.descriptions = request.Descriptions;
                product.isActive = request.IsActive;
                product.meta_descriptions = request.Meta_descriptions;
                product.meta_keywords = request.Meta_keywords;
                product.unit_price = decimal.Parse(request.Unit_price);
                product.promotion_price = !string.IsNullOrWhiteSpace(request.Promotion_price) ? decimal.Parse(request.Promotion_price) : 0;
                product.best_seller = request.Best_seller;
                product.featured = request.Featured;
                product.instock = request.Instock;
                product.brand_id = request.Brand_id;
                product.meta_tittle = request.Meta_tittle;
                product.code = request.Code;
                product.update_at = DateTime.Now;
                if (request.Image != null)
                {
                    await _storageService.DeleteFileAsync(product.image);
                    product.image = await this.SaveFile(request.Image);
                }
                if (request.More_images != null)
                {
                    for (int i = 0; i < request.More_images.Count(); i++)
                    {
                        product.more_images += await this.SaveFile(request.More_images[i]) + ",";
                    }
                }

                var pic = await _context.CategoryProducts.Where(x => x.product_id == request.Id).ToListAsync();
                if (pic != null)
                {
                    foreach (var cate in pic)
                    {
                        _context.CategoryProducts.Remove(cate);
                    }
                }

                string[] cateIDs = request.CateID.Split(",");
                foreach (string cateID in cateIDs)
                {

                    if (cateID != "")
                    {
                        var productInCategory = new TechShopSolution.Data.Entities.CategoryProduct
                        {
                            cate_id = int.Parse(cateID),
                            product_id = request.Id
                        };
                        _context.CategoryProducts.Add(productInCategory);
                    }
                }
                await _context.SaveChangesAsync();
                return new ApiSuccessResult<bool>();
            }
            catch
            {
                return new ApiErrorResult<bool>("Cập nhật thất bại");
            }
        }
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
        public async Task<ApiResult<bool>> ChangeStatus(int id)
        {
            try
            {
                var productExist = await _context.Products.FindAsync(id);
                if (productExist != null || productExist.isDelete)
                {
                    if (productExist.isActive)
                        productExist.isActive = false;
                    else productExist.isActive = true;
                    productExist.update_at = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return new ApiSuccessResult<bool>();
                }
                else return new ApiErrorResult<bool>("Không tìm thấy sản phẩm này");
            }
            catch
            {
                return new ApiErrorResult<bool>("Cập nhật thất bại");
            }
        }
        public async Task<ApiResult<bool>> OffBestSeller(int id)
        {
            try
            {
                var productExist = await _context.Products.FindAsync(id);
                if (productExist != null || productExist.isDelete)
                {
                    if (productExist.best_seller)
                        productExist.best_seller = false;
                    else productExist.best_seller = true;
                    productExist.update_at = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return new ApiSuccessResult<bool>();
                }
                else return new ApiErrorResult<bool>("Không tìm thấy sản phẩm này");
            }
            catch
            {
                return new ApiErrorResult<bool>("Cập nhật thất bại");
            }
        }
        public async Task<ApiResult<bool>> OffFeatured(int id)
        {
            try
            {
                var productExist = await _context.Products.FindAsync(id);
                if (productExist != null || productExist.isDelete)
                {
                    if (productExist.featured)
                        productExist.featured = false;
                    else productExist.featured = true;
                    productExist.update_at = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return new ApiSuccessResult<bool>();
                }
                else return new ApiErrorResult<bool>("Không tìm thấy sản phẩm này");
            }
            catch
            {
                return new ApiErrorResult<bool>("Cập nhật thất bại");
            }
        }
        public List<ProductRankingViewModel> GetProductViewRanking(int take)
        {
            var query = from p in _context.Products
                        where p.isDelete == false
                        select new { p };

            var group = query.AsEnumerable()
               .GroupBy(g => g.p);

            var listMostViewProducts = group.OrderByDescending(x => x.Key.view_count)
                 .Take(take)
                 .Select(a => new ProductRankingViewModel()
                 {
                     id = a.Key.id,
                     name = a.Key.name,
                     best_seller = a.Key.best_seller,
                     featured = a.Key.featured,
                     image = a.Key.image,
                     promotion_price = a.Key.promotion_price,
                     slug = a.Key.slug,
                     create_at = a.Key.create_at,
                     unit_price = a.Key.unit_price,
                     count = a.Key.view_count,
                 }).ToList();

            return new List<ProductRankingViewModel>(listMostViewProducts);
        }
        public List<ProductRankingViewModel> GetProductMostSalesRanking(int take)
        {
            var query = from od in _context.OrDetails
                        join o in _context.Orders on od.order_id equals o.id
                        join p in _context.Products on od.product_id equals p.id
                        where o.status != -1
                        select new { p, od };

            var group = query.AsEnumerable()
               .GroupBy(g => g.p);

            var result = new List<ProductRankingViewModel>();


            foreach (var item in group)
            {
                var product = new ProductRankingViewModel();
                product.id = item.Key.id;
                product.name = item.Key.name;
                product.image = item.Key.image;
                product.slug = item.Key.slug;
                product.count = 0;
                foreach(var od in item.Key.OrderDetails)
                {
                    product.count += od.quantity;
                }
                result.Add(product);
            }

            result = result.OrderByDescending(x => x.count).Take(take).ToList();

            return new List<ProductRankingViewModel>(result);
        }
        public List<ProductRankingViewModel> GetProductFavoriteRanking(int take)
        {
            var query = from p in _context.Products
                        join f in _context.Favorites on p.id equals f.product_id 
                        where p.isDelete == false
                        select new { p, f };

            var group = query.AsEnumerable()
               .GroupBy(g => g.p);

            var listMostViewProducts = group.OrderByDescending(x => x.Key.Favoriters.Count)
                 .Take(take)
                 .Select(a => new ProductRankingViewModel()
                 {
                     id = a.Key.id,
                     name = a.Key.name,
                     best_seller = a.Key.best_seller,
                     featured = a.Key.featured,
                     image = a.Key.image,
                     promotion_price = a.Key.promotion_price,
                     slug = a.Key.slug,
                     create_at = a.Key.create_at,
                     unit_price = a.Key.unit_price,
                     count = a.Key.Favoriters.Count,
                 }).ToList();

            return new List<ProductRankingViewModel>(listMostViewProducts);
        }
    }
}
