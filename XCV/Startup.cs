using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XCV.Data;
using XCV.Entities;
using XCV.Services;
using XCV.ValidationAttributes;

namespace XCV
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            services.AddScoped<ProtectedLocalStorage>();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Pages").AllowAnonymousToPage("/Pages/Index");
            });
            services.AddScoped<IExperienceService, ExperienceService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IShownEmployeePropertiesService, ShownEmployeePropertiesService>();
            services.AddScoped<IHourlyWagesService, HourlyWagesService>();
            services.AddScoped<IDocumentConfigurationService, DocumentConfigurationService>();
            services.AddSingleton<DatabaseUtils>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectActivityService, ProjectActivityService>();
            services.AddScoped<SearchManager>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<FillExperienceTable>();
            services.AddScoped<OfferManager>();
            services.AddScoped<ProjectManager>();
            services.AddScoped<ExperienceManager>();
            services.AddScoped<EmployeeManager>();
            services.AddScoped<FillDummyData>();
            services.AddBlazorDownloadFile();
            services.AddScoped<IDocumentGenerationService, WordDocumentGenerationService>();
            services.AddScoped<DocumentConfigurationManager>();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}