using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Data;
using thuctapCN.Models;

namespace thuctapCN
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Lấy chuỗi kết nối từ appsettings.json hoặc Environment Variables
            var connectionString = builder.Configuration.GetConnectionString("thuctapCNContextConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__thuctapCNContextConnection")
                ?? throw new InvalidOperationException("Connection string 'thuctapCNContextConnection' not found.");

            // Cấu hình DbContext
            builder.Services.AddDbContext<thuctapCNContext>(options =>
                options.UseSqlServer(connectionString));

            // Cấu hình Identity với Roles
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                // Tắt xác nhận email để đăng nhập dễ hơn 
                options.SignIn.RequireConfirmedAccount = false;
                
                // Cấu hình password
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                
                // Cấu hình user
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<thuctapCNContext>();

            // Thêm MVC
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Đảm bảo thư mục wwwroot tồn tại
            if (!Directory.Exists(app.Environment.WebRootPath))
            {
                Directory.CreateDirectory(app.Environment.WebRootPath);
            }

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                
                // Bảo mật headers
                app.Use(async (context, next) =>
                {
                    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                    context.Response.Headers["X-Frame-Options"] = "DENY";
                    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                    await next();
                });
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            // Tạo thư mục uploads và cấu hình static files
            var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads"
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Bắt buộc người dùng đăng nhập trước khi truy cập ứng dụng
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
                if (!context.User.Identity?.IsAuthenticated == true &&
                    !path.StartsWith("/identity/account/login") &&
                    !path.StartsWith("/identity/account/register") &&
                    !path.StartsWith("/css") &&
                    !path.StartsWith("/js") &&
                    !path.StartsWith("/lib") &&
                    !path.StartsWith("/images") &&
                    !path.StartsWith("/uploads"))
                {
                    context.Response.Redirect("/Identity/Account/Login");
                    return;
                }
                await next();
            });

            // Mặc định routing
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //Bắt buộc để Identity UI hoạt động
            app.MapRazorPages();

            // Tạo thư mục uploads/avatars nếu chưa có
            var avatarsPath = Path.Combine(app.Environment.WebRootPath, "uploads", "avatars");
            if (!Directory.Exists(avatarsPath))
            {
                try
                {
                    Directory.CreateDirectory(avatarsPath);
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Đã tạo thư mục uploads: {Path}", avatarsPath);
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Không thể tạo thư mục uploads: {Path}", avatarsPath);
                }
            }

            // Chạy migration và seed dữ liệu
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    var context = services.GetRequiredService<thuctapCNContext>();
                    
                    // Cleanup dữ liệu TRƯỚC KHI kiểm tra migrations
                    logger.LogInformation("Đang cleanup dữ liệu...");
                    await SeedData.CleanupBeforeMigration(services);
                    
                    // Kiểm tra xem có pending migrations không
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Đang chạy migration...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Đã chạy migration thành công.");
                    }

                    // Seed dữ liệu sau khi migration hoàn thành
                    var shouldSeed = Environment.GetEnvironmentVariable("RUN_SEED_DATA") == "true" || app.Environment.IsDevelopment();
                    if (shouldSeed)
                    {
                        await SeedData.Initialize(services);
                        logger.LogInformation("Đã seed dữ liệu thành công.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Lỗi khi khởi tạo database: {Error}", ex.Message);
                    // Không throw exception để app vẫn có thể chạy
                }
            }

            app.Run();
        }
    }
}
