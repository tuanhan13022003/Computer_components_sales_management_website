﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechShopSolution.ViewModels.Catalog.Product;
using TechShopSolution.ViewModels.Website.Contact;
using TechShopSolution.ViewModels.Website.Slide;

namespace TechShopSolution.WebApp.Models
{
    public class HomeViewModel
    {
        public List<SlideViewModel> Sliders { get; set; }
        public PublicProductsViewModel FeaturedProducts { get; set; }
        public PublicProductsViewModel BestSellerProducts { get; set; }
        public List<PublicCayegoyProductsViewModel> ListCategoryProducts { get; set; }
    }
}
