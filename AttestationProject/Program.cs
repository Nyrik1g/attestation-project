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

// — EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// — Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Отключаем требование подтверждённого e-mail
    options.SignIn.RequireConfirmedEmail = false;

    // Настройка паролей
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// — MVC + Razor Pages (для страницы подтверждения и сброса пароля Identity UI)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// — Наш защищённый EmailSender (MailKit, но пропускает, если нет Smtp:Host)
builder.Services.AddTransient<IEmailSender, EmailSender>();

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
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// — Маршруты для MVC и Identity UI
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();  // нужно, чтобы работали страницы Identity

// — Seed ролей
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var name in new[] { "Admin", "User" })
        if (!await roleMgr.RoleExistsAsync(name))
            await roleMgr.CreateAsync(new IdentityRole(name));
}

// — Seed первого админа (замените на ваш e-mail и безопасный пароль)
using (var scope = app.Services.CreateScope())
{
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminMail = "nyrik4653@gmail.com";
    var admin = await userMgr.FindByEmailAsync(adminMail);

    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminMail,
            Email = adminMail,
            EmailConfirmed = true
        };
        // Установите здесь ваш сложный пароль
        await userMgr.CreateAsync(admin, "VeryStrongP@ssw0rd!");
    }

    if (!await userMgr.IsInRoleAsync(admin, "Admin"))
        await userMgr.AddToRoleAsync(admin, "Admin");
}

app.Run();
