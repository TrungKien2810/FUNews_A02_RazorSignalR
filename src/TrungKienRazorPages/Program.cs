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

// Compress HTML, CSS, JS and the RSS feed on the way out. The feed is not in the default
// list because its content type is application/rss+xml, not one of the text/* defaults.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/rss+xml", "image/svg+xml" });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// A 404 from a mistyped address is a page, not a blank screen.
app.UseStatusCodePagesWithReExecute("/NotFound", "?code={0}");

// Baseline hardening. No CSP yet: the layout carries one inline script that marks the
// document script-capable before first paint, and a policy without a nonce for it would
// either break the menu or be written as 'unsafe-inline' and mean nothing.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["X-Frame-Options"] = "SAMEORIGIN";
    await next();
});

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    // Our own css/js are requested with asp-append-version, and the vendored libraries
    // change only when the folder does, so a week is a safe cache lifetime for both.
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<NewsHub>(NewsHub.Path);

app.Run();
