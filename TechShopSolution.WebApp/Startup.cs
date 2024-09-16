using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechShopSolution.ApiIntegration;
using TechShopSolution.ViewModels.Catalog.Customer;
using TechShopSolution.ViewModels.Catalog.Customer.Validator;

namespace TechShopSolution.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.LoginPath = "/dang-nhap";
                  options.AccessDeniedPath = "/User/Forbidden/";
              });
            services.AddControllersWithViews().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CustomerRegisterValidator>());
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(180);
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IProductApiClient, ProductApiClient>();
            services.AddTransient<ICategoryApiClient, CategoryApiClient>();
            services.AddTransient<IBrandApiClient, BrandApiClient>();
            services.AddTransient<IAdminApiClient, AdminApiClient>();
            services.AddTransient<ICouponApiClient, CouponApiClient>();
            services.AddTransient<ICustomerApiClient, CustomerApiClient>();
            services.AddTransient<IPaymentApiClient, PaymentApiClient>();
            services.AddTransient<IOrderApiClient, OrderApiClient>();
            services.AddTransient<ISlideApiClient, SlideApiClient>();
            services.AddTransient<IContactApiClient, ContactApiClient>();
            services.AddTransient<IValidator<CustomerPublicUpdateRequest>, CustomerUpdatePublicValidator>();
            services.AddTransient<IValidator<CustomerPublicUpdateRequest>, CustomerUpdatePublicValidator>();

            IMvcBuilder builder = services.AddRazorPages();
            var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
#if DEBUG   
            if (enviroment == Environments.Development)
            {
                builder.AddRazorRuntimeCompilation();
            }
#endif
    }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("/Error/{0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {


                endpoints.MapControllerRoute(
                   name: "Chi Tiet san pham",
                   pattern: "/{san-pham}/{slug}", new
                   {
                       controller = "Product",
                       action = "Detail"
                   });
                endpoints.MapControllerRoute(
                   name: "Danh sach san pham",
                   pattern: "/danh-muc/{slug}", new
                   {
                       controller = "Product",
                       action = "Category"
                   });
                endpoints.MapControllerRoute(
                  name: "Dang nhap",
                  pattern: "/dang-nhap", new
                  {
                      controller = "Account",
                      action = "Login"
                  });

                endpoints.MapControllerRoute(
                  name: "Dang Ky",
                  pattern: "/dang-ky", new
                  {
                      controller = "Account",
                      action = "Register"
                  });
                endpoints.MapControllerRoute(
                  name: "Tim kiem",
                  pattern: "/san-pham", new
                  {
                      controller = "Product",
                      action = "SearchProducts"
                  });
                endpoints.MapControllerRoute(
                 name: "Chi tiet tai khoan",
                 pattern: "/tai-khoan", new
                 {
                     controller = "Account",
                     action = "Detail"
                 });
                endpoints.MapControllerRoute(
                name: "Chi tiet gio hang",
                pattern: "/gio-hang", new
                {
                    controller = "Cart",
                    action = "Index"
                });
                endpoints.MapControllerRoute(
               name: "don hang da mua",
               pattern: "tai-khoan/don-hang", new
               {
                   controller = "Order",
                   action = "OrderTracking"
               });

                endpoints.MapControllerRoute(
                  name: "chi tiet don hang da mua",
                  pattern: "tai-khoan/don-hang/{id}", new
                  {
                      controller = "Order",
                      action = "OrderDetail"
                  });

                endpoints.MapControllerRoute(
              name: "don hang da mua",
              pattern: "tai-khoan/san-pham-yeu-thich", new
              {
                  controller = "Account",
                  action = "FavoriteProducts"
              });

                endpoints.MapControllerRoute(
                  name: "Thanh toan gio hang",
                  pattern: "/gio-hang/thanh-toan", new
                  {
                      controller = "Cart",
                      action = "Checkout"
                  });
                endpoints.MapControllerRoute(
                name: "Lien he",
                pattern: "/lien-he", new
                {
                    controller = "Contact",
                    action = "Index"
                });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
