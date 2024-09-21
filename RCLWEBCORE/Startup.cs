using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RCLWEBCORE.Filters;
using RCLWEBCORE.Insfrastructures.Data;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Repositories;
using RCLWEBCORE.Insfrastructures.Services;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using Rotativa.AspNetCore;

namespace RCLWEBCORE
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<SISRoyalDbContext>(options =>
               options.UseSqlServer(
                   Configuration.GetConnectionString("DefaultConnection2")));
            //services.AddMvc();
            //services.AddControllers();

            services.AddControllersWithViews();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(60);//You can set Time   
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(
                options =>
                {
                    options.ExpireTimeSpan= TimeSpan.FromMinutes(60);
                    options.LoginPath = "/Account/Login";
                    options.EventsType = typeof(CustomCookieAuthenticationEvents);
                    options.AccessDeniedPath = "";
                }
              );

            
            services.AddRazorPages();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<CustomCookieAuthenticationEvents>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ITrackerService, TrackerService>();


            //services.AddRazorRuntimeCompilation();
            //services.AddMvc().AddRazorPagesOptions(options =>
            //{
            //    options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "");
            //});


        }
        public void Configure(/*WebApplicationBuilder*/ IApplicationBuilder app, IWebHostEnvironment env/*, IDbInitializer dbInit*/)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");

            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}


            //app.UseExceptionHandler("/Home/Error/{0}");
            //app.UseStatusCodePagesWithReExecute("/Error/{0}");
            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            };
            app.UseCookiePolicy(cookiePolicyOptions);
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            //dbInit.Initialize();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=ViewAllCodes}/{id?}");
                endpoints.MapRazorPages();
            });

            //app.Run();


            //  RotativaConfiguration.Setup((Microsoft.AspNetCore.Hosting.IHostingEnvironment)env);
            //  RotativaConfiguration.Setup((Microsoft.AspNetCore.Hosting.IHostingEnvironment)env, "Rotativa");
            RotativaConfiguration.Setup(env.WebRootPath, "Rotativa");
        }
    }
}
