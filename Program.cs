using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Validators;

var builder = WebApplication.CreateBuilder(args);

// 資料庫連線設定
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 配置 Controllers 和相關服務
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    })
    .AddFluentValidation(fv =>
    {
        // 註冊 FluentValidation
        fv.RegisterValidatorsFromAssemblyContaining<TaskCreateDtoValidator>();
    });

// API 文件相關
builder.Services.AddEndpointsApiExplorer();

// 日誌服務
builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.AddDebug();
});

var app = builder.Build();

// 環境相關配置
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware 配置
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 路由配置
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Projects}/{action=Index}/{id?}");

// API 路由
app.MapControllers();

app.Run();