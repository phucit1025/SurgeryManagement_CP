﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration.Processors.Security;
using Surgery_1.Data.Context;
using Surgery_1.Services.Implementations;
using Surgery_1.Services.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Surgery_1
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
            services.AddDbContext<AppDbContext>();
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            #region Identity
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<AppDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 0;

                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.MaxFailedAccessAttempts = 8;

            });
            #endregion

            #region Register Services
            services.AddScoped<ISurgeryService, SurgeryService>();
            services.AddScoped<IPostOpService, PostOpService>();
            services.AddScoped<IMedicalSupplyConfirmService, MedicalSupplyConfirmService>();
            services.AddScoped<ISurgeryShiftService, SurgeryShiftService>();
            #endregion

            #region JWT Config

            //Add JWT Authentication 
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtconfig =>
            {
                jwtconfig.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    ValidIssuer = Configuration["JWT:issuer"],
                    ValidAudience = Configuration["JWT:audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:key"])),
                };
            });

            services.AddAuthorization();
            #endregion


            services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials()));
            services.AddAutoMapper();
            services.AddSwagger();
            services.AddHttpContextAccessor();
            services.AddDirectoryBrowser();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, AppDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //dbContext.Database.EnsureDeleted();
                //dbContext.Database.EnsureCreated();

            }
            else
            {
                app.UseHsts();
            }

            app.UseSwaggerUi3WithApiExplorer(settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling =
                    PropertyNameHandling.CamelCase;

                settings.GeneratorSettings.Title = "Surgery Management Api";

                settings.GeneratorSettings.OperationProcessors.Add(new OperationSecurityScopeProcessor("Bearer"));

                settings.GeneratorSettings.DocumentProcessors.Add(new SecurityDefinitionAppender("Bearer",
                    new SwaggerSecurityScheme
                    {
                        Type = SwaggerSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        Description = "Copy 'Bearer ' + valid JWT token into field",
                        In = SwaggerSecurityApiKeyLocation.Header
                    }));
            });
            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();

            #region Init Users
            InitIdentities(serviceProvider, "Admin", "admin1@gmail.com", "Zaq@123");
            #endregion

        }

        private void InitIdentities(IServiceProvider serviceProvider, string roleName, string email, string password)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            Task<IdentityResult> roleResult;

            //Check that there is an user role and create if not
            Task<bool> hasAdminRole = roleManager.RoleExistsAsync(roleName);
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new IdentityRole(roleName));
                roleResult.Wait();
            }

            //Check if the admin user exists and create it if not
            //Add to the user role

            Task<IdentityUser> testAdmin = userManager.FindByEmailAsync(email);
            testAdmin.Wait();

            if (testAdmin.Result == null)
            {
                IdentityUser user = new IdentityUser();
                user.Email = email;
                user.UserName = email;

                Task<IdentityResult> newAdmin = userManager.CreateAsync(user, password);
                newAdmin.Wait();

                if (newAdmin.Result.Succeeded)
                {
                    Task<IdentityResult> newAdminRole = userManager.AddToRoleAsync(user, roleName);
                    newAdminRole.Wait();
                }
            }
        }
    }
}
