using JWTCore.Authentication.Helpers;
using JWTCore.Authentication.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using JWTCore.Authentication.Entities;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;

namespace JWTCore.Api
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
            services.AddControllers(options => 
            {
                options.RespectBrowserAcceptHeader = true;
            })
            .AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddCors()
            .Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddDataProtection();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<UserContext>(x => x.UseInMemoryDatabase("TestDB"));

            var section = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(section);

            var appSettings = section.Get<AppSettings>();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x => x.DefaultJwtBearerOptions(appSettings));
            services.AddScoped<ILoginService, LoginService>();
            services.AddAntiforgery(options => 
            {
                options.HeaderName = "X-XSRF-TOKEN";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserContext context)
        {
            var module1 = new Modules
            {
                Id = 1,
                Module = "Categories",
                Status = true
            };
            var module2 = new Modules
            {
                Id = 2,
                Module = "Users",
                Status = true
            };
            context.Modules.AddRange(module1, module2);
            context.SaveChanges();
            var user1 = new User 
            {
                Id = 1,
                FirstName = "Diego",
                LastName = "Pineda",
                Username = "diegoapsilva97@gmail.com",
                Password = "test12345",
                Age = 18
            };
            var user2 = new User 
            {
                Id = 2,
                FirstName = "Alejandro",
                LastName = "Silva",
                Username = "alejandro.silva",
                Password = "test",
                Age = 10
            };
            context.Users.AddRange(user1, user2);
            context.SaveChanges();

            context.UsersModules.AddRange(new UsersModules
            {
                Id = 1,
                IdUser = 1,
                User = user1,
                IdModule = 1,
                Module = module1
            },
            new UsersModules
            {
                Id = 2,
                IdUser = 1,
                User = user1,
                IdModule = 2,
                Module = module2
            },
            new UsersModules
            {
                Id = 3,
                IdUser = 2,
                User = user2,
                IdModule = 1,
                Module = module1
            });
            context.SaveChanges();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                context.Request.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Request.Headers.Add("Referrer-Policy", "no-referrer");
                context.Response.Headers.Add("Feature-Polity", "accelerometer 'none'; camera 'none'; geolocation 'none'; magnetometer 'none'; microphone 'none'; payment 'none'; usb 'none'");
                context.Response.Headers.Add("Content-Security-Polity", "default-src 'self'; report-uri /cspreport");
                await next();
            });

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            // app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseCors(options => 
            {
                options.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}