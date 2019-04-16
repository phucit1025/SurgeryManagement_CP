using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
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
using Surgery_1.Hubs;
using Surgery_1.Services.Implementations;
using Surgery_1.Services.Interfaces;
using Surgery_1.Services.Utilities;
using System;
using System.IO;
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
                options.Password.RequiredLength = 4;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
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
            services.AddScoped<IDrugService, DrugService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IUtilsService, UtilsService>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISpecialtyService, SpecialtyService>();
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

            var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials()));
            services.AddAutoMapper();
            services.AddSwagger();
            services.AddHttpContextAccessor();
            services.AddDirectoryBrowser();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddSignalR();
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
            app.UseSignalR(routes =>
            {
                routes.MapHub<NotifyHub>("/notify");
            });
            app.UseMvc();


            #region Init Users
            //InitIdentities(serviceProvider, "MedicalSupplier", "supplier1", "zxc@123456");
            //InitIdentities(serviceProvider, "HospitalStaff", "hospital_staff1", "zxc@123456");
            //InitIdentities(serviceProvider, "ChiefNurse", "cnurse1", "zxc@123456");
            //InitIdentities(serviceProvider, "Nurse", "nurse1", "zxc@123456");
            //InitIdentities(serviceProvider, "Nurse", "nurse2", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical1", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical2", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical3", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical4", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical5", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical6", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical7", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical8", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical9", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical10", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical11", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical12", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical13", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical14", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical15", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical16", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical17", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical18", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical19", "zxc@123456");
            //InitIdentities(serviceProvider, "Technical", "technical20", "zxc@123456");
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
