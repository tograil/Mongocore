using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity4.Mongo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace identityMongo
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            _env = env;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDb"));
            services.AddSingleton<IUserStore<MongoIdentityUser>>(provider =>
            {
                var options = provider.GetService<IOptions<MongoDbSettings>>();
                var client = new MongoClient(options.Value.ConnectionString);
                var database = client.GetDatabase(options.Value.DatabaseName);

                return new MongoUserStore<MongoIdentityUser>(database);
            });
            // Add framework services.

            services.Configure<IdentityOptions>(options =>
            {
                var dataProtectionPath = Path.Combine(_env.WebRootPath, "identity-artifacts");
                options.Cookies.ApplicationCookie.AuthenticationScheme = "ApplicationCookie";
                options.Cookies.ApplicationCookie.DataProtectionProvider = DataProtectionProvider.Create(dataProtectionPath);
                options.Lockout.AllowedForNewUsers = true;
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "Bearer",
                Authority = "http://localhost:54008",
                ClientId = "MongoTest",
                Scope = { "MongoTest" },
                RequireHttpsMetadata = false,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = "http://localhost:54008",
                AllowedScopes = { "MongoTest" },

                RequireHttpsMetadata = false
            });
            app.UseIdentity();

            app.UseIdentityServer();


            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 &&
                   !Path.HasExtension(context.Request.Path.Value) &&
                   !context.Request.Path.Value.StartsWith("/api/"))
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });

            app.UseMvcWithDefaultRoute();

            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        private void AddDefaultTokenProviders(IServiceCollection services)
        {
            var dataProtectionProviderType = typeof(DataProtectorTokenProvider<>).MakeGenericType(typeof(MongoIdentityUser));
            var phoneNumberProviderType = typeof(PhoneNumberTokenProvider<>).MakeGenericType(typeof(MongoIdentityUser));
            var emailTokenProviderType = typeof(EmailTokenProvider<>).MakeGenericType(typeof(MongoIdentityUser));
            AddTokenProvider(services, TokenOptions.DefaultProvider, dataProtectionProviderType);
            AddTokenProvider(services, TokenOptions.DefaultEmailProvider, emailTokenProviderType);
            AddTokenProvider(services, TokenOptions.DefaultPhoneProvider, phoneNumberProviderType);
        }

        private void AddTokenProvider(IServiceCollection services, string providerName, Type provider)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(provider);
            });

            services.AddSingleton(provider);
        }

        public class UserClaimsPrincipalFactory<TUser> : Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<TUser>
            where TUser : class
        {
            public UserClaimsPrincipalFactory(
                UserManager<TUser> userManager,
                IOptions<IdentityOptions> optionsAccessor)
            {
                if (userManager == null)
                {
                    throw new ArgumentNullException(nameof(userManager));
                }
                if (optionsAccessor == null || optionsAccessor.Value == null)
                {
                    throw new ArgumentNullException(nameof(optionsAccessor));
                }

                UserManager = userManager;
                Options = optionsAccessor.Value;
            }

            public UserManager<TUser> UserManager { get; private set; }

            public IdentityOptions Options { get; private set; }

            public virtual async Task<ClaimsPrincipal> CreateAsync(TUser user)
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                var userId = await UserManager.GetUserIdAsync(user);
                var userName = await UserManager.GetUserNameAsync(user);
                var id = new ClaimsIdentity(Options.Cookies.ApplicationCookieAuthenticationScheme,
                    Options.ClaimsIdentity.UserNameClaimType,
                    Options.ClaimsIdentity.RoleClaimType);
                id.AddClaim(new Claim(Options.ClaimsIdentity.UserIdClaimType, userId));
                id.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, userName));
                if (UserManager.SupportsUserSecurityStamp)
                {
                    id.AddClaim(new Claim(Options.ClaimsIdentity.SecurityStampClaimType,
                        await UserManager.GetSecurityStampAsync(user)));
                }
                if (UserManager.SupportsUserRole)
                {
                    var roles = await UserManager.GetRolesAsync(user);
                    foreach (var roleName in roles)
                    {
                        id.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, roleName));
                    }
                }
                if (UserManager.SupportsUserClaim)
                {
                    id.AddClaims(await UserManager.GetClaimsAsync(user));
                }

                return new ClaimsPrincipal(id);
            }
        }
    }
}
