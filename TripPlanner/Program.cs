using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Components;
using TripPlanner.Components.Account;
using TripPlanner.Components.Trips;
using TripPlanner.Data;
using TripPlanner.Data.Repositories;
using TripPlanner.Services;
using AntDesign;
using Microsoft.Data.Sqlite;

namespace TripPlanner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            // DB provider per environment
            if (builder.Environment.IsEnvironment("Testing"))
            {
                // Prefer file-based Sqlite in Testing; fallback to in-memory shared cache
                var e2eConn = builder.Configuration.GetConnectionString("E2E");
                if (!string.IsNullOrWhiteSpace(e2eConn))
                {
                    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                        options.UseSqlite(e2eConn));
                }
                else
                {
                    var sqlite = new SqliteConnection("Data Source=:memory:;Cache=Shared");
                    sqlite.Open();
                    builder.Services.AddSingleton(sqlite);

                    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                        options.UseSqlite(sqlite));
                }
            }
            else
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.AddScoped<ITripRepository, TripRepository>();
            builder.Services.AddScoped<ITripManager, TripManager>();
            builder.Services.AddScoped<ITripPlanGeneratorService, TripPlanGeneratorService>();
            builder.Services.AddAntDesign();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped(sp =>
            {
                var navigation = sp.GetRequiredService<NavigationManager>();
                return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else if (!app.Environment.IsEnvironment("Testing"))
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            if (!app.Environment.IsEnvironment("Testing"))
            {
                app.UseHttpsRedirection();
            }

            app.UseAntiforgery();
            app.MapStaticAssets();

            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();

            app.MapAdditionalIdentityEndpoints();
            app.MapTripEndpoints();

            app.Run();
        }
    }
}
