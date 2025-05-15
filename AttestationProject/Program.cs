using AttestationProject.Data;
using AttestationProject.Models;
using AttestationProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// — Подключаем конфигурацию из файлов
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true);

// — MVC
builder.Services.AddControllersWithViews();

// — Наш защищённый EmailSender (MailKit, но пропускает, если нет Smtp:Host)
builder.Services.AddTransient<IEmailSender, EmailSender>();

// — EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// — Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// — Настройка паролей
builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireDigit = true;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireLowercase = false;
});

// — Настройка cookie
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// — Конвейер HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // на локали можно включить подробные страницы ошибок
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// — Seed ролей
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var name in new[] { "Admin", "User" })
        if (!await roleMgr.RoleExistsAsync(name))
            await roleMgr.CreateAsync(new IdentityRole(name));
}

// — Seed первого админа (замените на ваш e-mail)
using (var scope = app.Services.CreateScope())
{
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminMail = "nyrik4653@gmail.com";
    var admin = await userMgr.FindByEmailAsync(adminMail);
    if (admin != null && !await userMgr.IsInRoleAsync(admin, "Admin"))
        await userMgr.AddToRoleAsync(admin, "Admin");
}

app.Run();
