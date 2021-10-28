using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MapTalkie.Configuration;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using MapTalkie.Services.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace MapTalkie
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Env { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetJwtSettings());
            services.AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetAuthenticationSettings());
            services.AddScoped<ITokenService, TokenService>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    if (Env.IsDevelopment())
                    {
                        builder.WithOrigins(
                            "http://localhost:3000", "https://localhost:3000"
                            );
                    }
                    else
                    {
                        // TODO
                        throw new NotImplementedException();
                    }
                    
                    builder.AllowCredentials();
                    builder.WithHeaders(HeaderNames.Authorization, HeaderNames.Accept, HeaderNames.CacheControl);
                });
            });
            
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("Default");
                if (Env.IsDevelopment())
                    options.UseSqlite(connectionString);
                else
                    options.UseNpgsql(connectionString);
            });
            
            services
                .AddIdentity<User, Role>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddAuthentication()
                .AddMapTalkieJwtBearer(
                    Configuration.GetJwtSettings(),
                    schemaName: JwtBearerDefaults.AuthenticationScheme,
                    hybridAuthenticationCookie: "_JWTS");

            services.AddAuthorization(auth =>
            {
                auth.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "HybridJWTBearer")
                    .Build();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }
    }
}