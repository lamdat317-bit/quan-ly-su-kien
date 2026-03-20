using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 1. CAU HINH DE CHAY DUOC TREN INTERNET
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// FIX: Neu chay tren Render ma chua co Database, he thong van se khong bi "sap"
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Server="))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Dung tam InMemory neu khong co SQL Server (De web hien len duoc)
        options.UseInMemoryDatabase("TempDb");
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseForwardedHeaders();

// FIX: Luon hien chi tiet loi khi dang trong giai doan Demo
app.UseDeveloperExceptionPage(); 

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Tu dong Migration neu co Connection String hop le
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.IsSqlServer())
        {
            context.Database.Migrate();
            // ... (Phần tạo Admin giữ nguyên)
        }
    }
    catch { /* Bo qua loi Database de web van hien len duoc */ }
}

app.Run();
