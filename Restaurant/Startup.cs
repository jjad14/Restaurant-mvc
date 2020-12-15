using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restaurant.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Restaurant.Utility;
using Stripe;

namespace Restaurant
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
            // mysql database config
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection")));
            
            // identity config
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Stripe Configuration
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            services.AddSingleton<IEmailSender, EmailSender>();
            
            services.AddControllersWithViews();

            services.AddRazorPages().AddRazorRuntimeCompilation();

            // default paths to login/logout and access denied
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

            });

            // facebook authentication
            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = "389441985636910";
                facebookOptions.AppSecret = "055c2cc80e078faa5c7a3f67fcb4a776";
            });

            // google authentication
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "258408073549-nskqghgrm0ka67mhmtg6kopmlpfnl5a8.apps.googleusercontent.com";
                googleOptions.ClientSecret = "TbFC8ZogOVc6hbcQbOOXqhX5";
            });

            // Session configuration - also in Configure
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];

            app.UseSession();

            // Authentication means that you have to authenticate the credentials of a user.
            app.UseAuthentication();

            // Authorization is defined as what access a certain user has
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
