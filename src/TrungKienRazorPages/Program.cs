using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Repositories;
using TrungKienRazorPages.Hubs;
using TrungKienRazorPages.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// CreateBuilder registers the Windows EventLog provider by default, which
// needs admin rights to open the ".NET Runtime" source. Running without
// elevation every request then dies with "Access is denied" inside the
// logger itself. Console + Debug are all this app needs.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddRazorPages(options =>
{
    // The Login page is the default page of the application.
    options.Conventions.AddPageRoute("/Account/Login", "");

    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Staff", "StaffOnly");
});
builder.Services.AddSignalR();

builder.Services.Configure<AdminAccountOptions>(builder.Configuration.GetSection(AdminAccountOptions.SectionName));

// Razor Pages -> Repository -> DAO (singleton) -> DbContext
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(AccountRoles.AdminName));
    options.AddPolicy("StaffOnly", p => p.RequireRole(AccountRoles.StaffName));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<NewsHub>(NewsHub.Path);

app.Run();
