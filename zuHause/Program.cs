using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using zuHause.Data;
using zuHause.Helpers;
using zuHause.Interfaces;
using zuHause.Interfaces.TenantInterfaces;
using zuHause.Middleware;
using zuHause.Models;
using zuHause.Options;
using zuHause.Services;
using zuHause.Services.TenantServices;
using zuHause.ViewModels;

var builder = WebApplication.CreateBuilder(args);


// 只在非測試環境載入 PDF 庫
if (Environment.GetEnvironmentVariable("SKIP_PDF_LIBRARY") != "true")
{
    try
    {
        var context = new CustomAssemblyLoadContext();

        var path = Path.Combine(Directory.GetCurrentDirectory(), "DinkToPdfLib", "libwkhtmltox.dll");
        context.LoadUnmanagedLibrary(path);

        builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
    }
    catch (System.DllNotFoundException ex)
    {
        // WSL 環境中忽略 libwkhtmltox.dll 載入錯誤
        Console.WriteLine($"PDF 庫載入失敗 (WSL 環境中正常): {ex.Message}");
        // 註冊空的 PDF 轉換器或跳過
    }
    catch (Exception ex)
    {
        Console.WriteLine($"PDF 庫載入失敗: {ex.Message}");
    }
}

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic,
                UnicodeRanges.CjkUnifiedIdeographs);
    options.JsonSerializerOptions.WriteIndented = true;
});

// 會員
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MemberCookieAuth";
})
.AddCookie("MemberCookieAuth", options =>
{
    options.LoginPath = "/Member/Login";
    options.AccessDeniedPath = "/Member/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie 有效時間 60 分鐘，確保完整支付流程
    options.SlidingExpiration = true; // 滑動期限（有活動就重置60分鐘）
    options.Cookie.HttpOnly = true; // 增加安全性
    options.Cookie.IsEssential = true; // 設定為必要的 Cookie
})
.AddCookie("AdminCookies", options =>// 管理員登入驗證
{
    options.LoginPath = "/Auth/Login"; // 登入路徑
    options.LogoutPath = "/Auth/Logout"; // 登出路徑
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie 有效時間 8 小時
    options.SlidingExpiration = true; // 滑動期限（有活動就延長）
});



builder.Services.AddDbContext<ZuHauseContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddMemoryCache();

// 註冊 HttpContextAccessor（用於取得 HTTP 上下文）
builder.Services.AddHttpContextAccessor();

// === Azure Blob Storage 配置與服務註冊 ===
builder.Services.Configure<BlobStorageOptions>(
    builder.Configuration.GetSection(BlobStorageOptions.SectionName));

// 註冊 Azure Blob Storage 服務
builder.Services.AddScoped<IBlobStorageConnectionTest, BlobStorageConnectionTest>();

// === 註冊臨時會話管理服務 ===
builder.Services.AddScoped<ITempSessionService, TempSessionService>();

// === 註冊 URL 生成與 Blob 操作服務 ===
builder.Services.AddScoped<IBlobUrlGenerator, BlobUrlGenerator>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// === 註冊檔案遷移服務 ===
builder.Services.AddScoped<IBlobMigrationService, BlobMigrationService>();

// === 註冊本地到雲端遷移服務 ===
builder.Services.AddScoped<ILocalToBlobMigrationService, LocalToBlobMigrationService>();

// === 註冊臨時檔案清理服務 ===
builder.Services.AddScoped<ITempFileCleanupService, TempFileCleanupService>();
builder.Services.AddHostedService<TempFileCleanupService>();

// 註冊更新申請Log服務
builder.Services.AddScoped<ApplicationService>();

// 註冊 RealDataSeeder
builder.Services.AddScoped<RealDataSeeder>();

//註冊發送通知服務
builder.Services.AddScoped<NotificationService>();
//註冊圖片轉址服務
builder.Services.AddScoped<ImageResolverService>();

// 註冊圖片處理服務
builder.Services.AddScoped<zuHause.Interfaces.IImageProcessor, zuHause.Services.ImageSharpProcessor>();

// 註冊統一圖片管理系統服務
builder.Services.AddScoped<zuHause.Interfaces.IEntityExistenceChecker, zuHause.Services.EntityExistenceChecker>();
builder.Services.AddScoped<zuHause.Interfaces.IDisplayOrderManager, zuHause.Services.DisplayOrderManager>();
builder.Services.AddScoped<zuHause.Interfaces.IImageQueryService, zuHause.Services.ImageQueryService>();
builder.Services.AddScoped<zuHause.Interfaces.IImageUploadService, zuHause.Services.ImageUploadService>();
builder.Services.AddScoped<zuHause.Services.Interfaces.IImageValidationService, zuHause.Services.ImageValidationService>();

// 註冊刊登方案驗證服務
builder.Services.AddScoped<zuHause.Services.Interfaces.IListingPlanValidationService, zuHause.Services.ListingPlanValidationService>();

// 註冊設備分類查詢服務
builder.Services.AddScoped<zuHause.Services.Interfaces.IEquipmentCategoryQueryService, zuHause.Services.EquipmentCategoryQueryService>();

// 註冊房源圖片 Facade 服務
builder.Services.AddScoped<zuHause.Interfaces.IPropertyImageService, zuHause.Services.PropertyImageService>();

// 註冊房源付款服務
builder.Services.AddScoped<zuHause.Interfaces.IPropertyPaymentService, zuHause.Services.PropertyPaymentService>();

// === 註冊管理員權限檢查服務 ===
// 用於 AdminController 的後端權限驗證
// 配合 RequireAdminPermissionAttribute 使用，防止直接 URL 存取無權限功能
builder.Services.AddScoped<zuHause.Services.Interfaces.IPermissionService, zuHause.Services.PermissionService>();

// === 註冊訊息模板服務 ===
// 用於參數化模板的解析和替換
builder.Services.AddScoped<zuHause.Services.MessageTemplateService>();

// === 註冊 Google Maps 整合服務 ===
// 地理編碼、地點搜尋、距離計算服務
builder.Services.AddHttpClient<zuHause.Interfaces.IGoogleMapsService, zuHause.Services.GoogleMapsService>();
builder.Services.AddScoped<zuHause.Interfaces.IApiUsageTracker, zuHause.Services.ApiUsageTracker>();
builder.Services.AddScoped<zuHause.Interfaces.IPropertyMapCacheService, zuHause.Services.PropertyMapCacheService>();

//註冊 Stripe 第三方金流付款設定
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
StripeConfiguration.ApiKey = builder.Configuration["StripeSettings:SecretKey"];

//首頁功能
builder.Services.AddScoped<IDataAccessService, DataAccessService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// === 新增 Session 服務配置 ===
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 超時時間，例如 30 分鐘
    options.Cookie.HttpOnly = true; // 設定 Session Cookie 只能透過 HTTP 訪問，增加安全性
    options.Cookie.IsEssential = true; // 設定 Session Cookie 為必要的，以便 Session 能夠工作
});
// =============================


builder.Services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();
builder.Services.AddScoped<MemberService>();

var app = builder.Build();

// 在開發環境確保基礎資料存在
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<RealDataSeeder>();
        try
        {
            await seeder.EnsureDataAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "確保基礎資料失敗");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // 認證中間件需要在 Session 之前
app.UseSession(); // 啟用 Session 中間件
app.UseMiddleware<ModuleTrackingMiddleware>(); // 模組追蹤中間件
app.UseAuthorization();
app.MapControllers();

// 先註冊 API Controllers 路由
app.MapControllers();

app.MapControllerRoute(
    name: "default",

//家具首頁路由
//pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

//pattern: "{controller=Home}/{action=Index}/{id?}");
//pattern: "{controller=Dashboard}/{action=Index}/{id?}");

//租屋首頁路由
pattern: "{controller=Tenant}/{action=FrontPage}/{id?}");




app.Run();

// 讓測試專案可以存取 Program 類別
public partial class Program { }
