using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using FirebaseAndAng.Web;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SpaServices.AngularCli;

namespace FireBaseAndAng.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "wwwroot";
            });

            string pathToKeys = Path.Combine(Directory.GetCurrentDirectory(), "keys",
                WebHostEnvironment.IsEnvironment("local") ? Constants.FIREBASE_CONFIG_NAME_LOCAL : Constants.FIREBASE_CONFIG_NAME);

            services.AddControllers();

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(pathToKeys)
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var firebaseProjectName = Configuration["FirebaseProjectName"];
                    options.Authority = string.Format(Constants.GOOGLE_AUTHORITY, firebaseProjectName);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "https://securetoken.google.com/" + firebaseProjectName,
                        ValidateAudience = true,
                        ValidAudience = firebaseProjectName,
                        ValidateLifetime = true
                    };
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FireBaseAndAng.Web", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            bool isLocal = env.IsEnvironment("local");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FireBaseAndAng.Web v1"));
            }

            //
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            //

            app.UseHttpsRedirection();

            app.UseRouting();

            //
            app.UseAuthentication();
            //

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "client-app";
                if (env.IsDevelopment() || isLocal)
                {
                    var startScriptName = isLocal ? "start-local" : "start";
                    spa.UseAngularCliServer(startScriptName);
                }
            });
        }
    }
}
