using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using TechShopSolution.Application.Catalog.Brand;
using TechShopSolution.Application.Catalog.Category;
using TechShopSolution.Application.Catalog.Coupon;
using TechShopSolution.Application.Catalog.Customer;
using TechShopSolution.Application.Catalog.Location;
using TechShopSolution.Application.Catalog.Order;
using TechShopSolution.Application.Catalog.PaymentMethod;
using TechShopSolution.Application.Catalog.Product;
using TechShopSolution.Application.Catalog.Transport;
using TechShopSolution.Application.Common;
using TechShopSolution.Application.Dapper.Report;
using TechShopSolution.Application.System;
using TechShopSolution.Application.Website.Contact;
using TechShopSolution.Application.Website.Slide;
using TechShopSolution.Data.EF;
using TechShopSolution.Utilities.Constants;
using TechShopSolution.ViewModels.Catalog.Brand;
using TechShopSolution.ViewModels.Catalog.Brand.Validator;
using TechShopSolution.ViewModels.Catalog.Category;
using TechShopSolution.ViewModels.Catalog.Category.Validator;
using TechShopSolution.ViewModels.Catalog.Customer;
using TechShopSolution.ViewModels.Catalog.Customer.Validation;
using TechShopSolution.ViewModels.Catalog.Customer.Validator;
using TechShopSolution.ViewModels.Catalog.Product;
using TechShopSolution.ViewModels.Catalog.Product.Validator;
using TechShopSolution.ViewModels.System;
using TechShopSolution.ViewModels.Transport;
using TechShopSolution.ViewModels.Transport.Validator;
using TechShopSolution.ViewModels.Website.Contact;
using TechShopSolution.ViewModels.Website.Contact.Validator;
using TechShopSolution.ViewModels.Website.Slide;
using TechShopSolution.ViewModels.Website.Slide.Validator;

namespace TechShopSolution.BackendApi
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
            services.AddDbContext<TechShopDBContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString(SystemConstants.MainConnectionString)));

            //Declare DI
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IStorageService, FileStorageService>();
            services.AddTransient<IAdminService, AdminService>();
            services.AddTransient<IValidator<LoginRequest>, LoginRequestValidator>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IBrandService, BrandService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<ICouponService, CouponService>();
            services.AddTransient<IPaymentMethodService, PaymentMethodService>();
            services.AddTransient<ITransportService, TransportService>();
            services.AddTransient<ISlideService, SlideService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<ILoadLocationService, LoadLocationService>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<IValidator<CustomerCreateRequest>, CreateRequestValidator>();
            services.AddTransient<IValidator<CustomerUpdateRequest>, UpdateRequestValidator>();
            services.AddTransient<IValidator<CustomerUpdateAddressRequest>, UpdateAddressRequestValidator>();
            services.AddTransient<IValidator<ProductCreateRequest>, CreateProductValidator>();
            services.AddTransient<IValidator<ProductUpdateRequest>, UpdateProductValidator>();
            services.AddTransient<IValidator<CreateCategoryRequest>, CreateCategoryValidator>();
            services.AddTransient<IValidator<UpdateCategoryRequest>, UpdateCategoryValidator>();
            services.AddTransient<IValidator<BrandCreateRequest>, BrandCreateValidator>();
            services.AddTransient<IValidator<BrandUpdateRequest>, BrandUpdateValidator>();
            services.AddTransient<IValidator<CustomerRegisterRequest>, CustomerRegisterValidator>();
            services.AddTransient<IValidator<TransporterCreateRequest>, TransporterCreateValidator>();
            services.AddTransient<IValidator<TransporterCreateRequest>, TransporterCreateValidator>();
            services.AddTransient<IValidator<TransporterUpdateRequest>, TransporterUpdateValidator>();
            services.AddTransient<IValidator<SlideUpdateRequest>, SlideUpdateValidator>();
            services.AddTransient<IValidator<SlideCreateRequest>, SlideCreateValidator>();
            services.AddTransient<IValidator<ContactUpdateRequest>, ContactUpdateValidator>();

            services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<LoginRequestValidator>());


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger TechShop", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                  {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,
                        },
                        new List<string>()
                      }
                    });
            });

            string issuer = Configuration.GetValue<string>("Tokens:Issuer");
            string signingKey = Configuration.GetValue<string>("Tokens:Key");
            byte[] signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(signingKey);

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = issuer,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = System.TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
                };
            });
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger TechShop V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
