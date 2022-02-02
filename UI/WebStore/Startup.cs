
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WebStore.DAL.Context;
using System;
using WebStore.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using WebStore.Interfaces.Services;
using WebStore.Services.Services.InSQL;
using WebStore.Services.Services.InCookies;
using WebStore.Services.Data;
using WebStore.Interfaces.TestAPI;
using WebStore.WebAPI.Clients.Values;
using WebStore.WebAPI.Clients.Employees;
using WebStore.WebAPI.Clients.Products;
using WebStore.WebAPI.Clients.Orders;
using WebStore.WebAPI.Clients.Identity;
using Microsoft.Extensions.Logging;
using WebStore.Logger;
using WebStore.Infrastructure.Middleware;
using WebStore.Services.Services;
using Polly;
using System.Net.Http;
using Polly.Extensions.Http;

namespace WebStore
{
    public record Startup(IConfiguration Configuration)
    {
       
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddIdentity<User, Role>()
              //.AddEntityFrameworkStores<WebStoreDB>()
              .AddIdentityWebStoreWebAPIClients()
              .AddDefaultTokenProviders();

            //services.AddIdentityWebStoreWebAPIClients();

            services.Configure<IdentityOptions>(opt =>
            {
#if true
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 3;
                opt.Password.RequiredUniqueChars = 3;
#endif

                opt.User.RequireUniqueEmail = false;
                opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIGKLMNOPQRSTUVWXYZ1234567890";

                opt.Lockout.AllowedForNewUsers = false;
                opt.Lockout.MaxFailedAccessAttempts = 10;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.Cookie.Name = "GB.WebStore";
                opt.Cookie.HttpOnly = true;

                opt.ExpireTimeSpan = TimeSpan.FromDays(10);

                opt.LoginPath = "/Account/Login";
                opt.LogoutPath = "/Account/Logout";
                opt.AccessDeniedPath = "/Account/AccessDenied";

                opt.SlidingExpiration = true;
            });

            services.AddScoped<ICartStore, InCookiesCartStore>();
            services.AddScoped<ICartService, CartService>();

            services.AddHttpClient("WebStoreWebAPI", client => client.BaseAddress = new(Configuration["WebAPI"]))
               .AddTypedClient<IValuesService, ValuesClient>()
               .AddTypedClient<IEmployeeData, EmployeesClient>()
               .AddTypedClient<IProductData, ProductsClient>()
               .AddTypedClient<IOrderService, OrdersClient>()
               .SetHandlerLifetime(TimeSpan.FromMinutes(5))     
               .AddPolicyHandler(GetRetryPolicy())              
               .AddPolicyHandler(GetCircuitBreakerPolicy());    

            static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int MaxRetryCount = 5, int MaxJitterTime = 1000)
            {
                var jitter = new Random();
                return HttpPolicyExtensions
                   .HandleTransientHttpError()
                   .WaitAndRetryAsync(MaxRetryCount, RetryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, RetryAttempt)) +
                        TimeSpan.FromMilliseconds(jitter.Next(0, MaxJitterTime)));
            }

            static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
                HttpPolicyExtensions
                   .HandleTransientHttpError()
                   .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, TimeSpan.FromSeconds(30));

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory log)
        {
            log.AddLog4Net();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseStatusCodePagesWithRedirects("~/home/status/{0}");

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWelcomePage("/welcome");

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/greetings", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });

                endpoints.MapControllerRoute(
                      name: "areas",
                      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    "default", "{controller=Home}/{action=Index}/{id?}");
              
                                
            });
        }
    }
}
