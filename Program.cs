using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

var cookieSecurePolicy = CookieSecurePolicy.SameAsRequest;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = cookieSecurePolicy;
    });

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    SeedData.EnsureSeedData(context);

    var env = services.GetRequiredService<IWebHostEnvironment>();
    var backupDir = Path.Combine(env.ContentRootPath, "backups");
    Directory.CreateDirectory(backupDir);

    var dbPath = context.Database.GetConnectionString();
    if (dbPath != null && dbPath.StartsWith("Data Source="))
        dbPath = dbPath["Data Source=".Length..];
    else
        dbPath = Path.Combine(env.ContentRootPath, "app.db");

    if (System.IO.File.Exists(dbPath))
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var autoBackup = Path.Combine(backupDir, $"auto_{timestamp}.db");
        System.IO.File.Copy(dbPath, autoBackup, overwrite: true);

        var backups = new DirectoryInfo(backupDir)
            .GetFiles("auto_*.db")
            .OrderByDescending(f => f.LastWriteTime)
            .Skip(5)
            .ToList();
        foreach (var old in backups)
            old.Delete();
    }
}

app.Run();
