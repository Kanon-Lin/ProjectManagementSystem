using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.BackgroundServices;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.Dtos;
using ProjectManagementSystem.Models.EFModels;
using ProjectManagementSystem.Repositories;
using ProjectManagementSystem.Services;
using ProjectManagementSystem.Validators;

var builder = WebApplication.CreateBuilder(args);

// 1. 基礎服務配置
builder.Services.AddLogging(configure =>  // 移到最前面，因為其他服務可能需要用到日誌
{
    configure.AddConsole();
    configure.AddDebug();
});

// 2. 資料庫服務
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 新增: Email 和任務提醒相關服務
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ITaskReminderService, TaskReminderService>();
builder.Services.AddHostedService<ReminderHostedService>();

// 3. MVC和API相關服務
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// 4. API 文件服務
builder.Services.AddEndpointsApiExplorer();

// Task相關驗證器
builder.Services.AddScoped<IValidator<TaskCreateDto>, TaskCreateDtoValidator>();
builder.Services.AddScoped<IValidator<TaskUpdateDto>, TaskUpdateDtoValidator>();

// Repository註冊
builder.Services.AddScoped<IMemberRepository, MemberRepository>();


// 5. 建立應用程式
var app = builder.Build();

// 6. 環境配置
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 7. Middleware配置（順序很重要）
app.UseHttpsRedirection();    // HTTPS 重定向
app.UseStaticFiles();         // 靜態檔案
app.UseRouting();            // 路由
app.UseAuthorization();      // 授權

// 8. 路由配置
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Projects}/{action=Index}/{id?}");
app.MapControllers();  // API 路由

// 9. 啟動應用程式
app.Run();