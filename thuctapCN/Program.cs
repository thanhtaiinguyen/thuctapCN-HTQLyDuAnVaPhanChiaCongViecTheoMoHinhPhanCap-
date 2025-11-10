using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Data;
using thuctapCN.Models;

namespace thuctapCN
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Lấy chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("thuctapCNContextConnection")
                ?? throw new InvalidOperationException("Connection string 'thuctapCNContextConnection' not found.");

            // Cấu hình DbContext
            builder.Services.AddDbContext<thuctapCNContext>(options =>
                options.UseSqlServer(connectionString));

            // Cấu hình Identity
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                // Tắt xác nhận email để đăng nhập dễ hơn 
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<thuctapCNContext>();

            // Thêm MVC
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            // Bắt buộc người dùng đăng nhập trước khi truy cập ứng dụng
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower();
                if (!context.User.Identity?.IsAuthenticated == true &&
                    !path.StartsWith("/identity/account/login") &&
                    !path.StartsWith("/identity/account/register") &&
                    !path.StartsWith("/css") &&
                    !path.StartsWith("/js") &&
                    !path.StartsWith("/lib") &&
                    !path.StartsWith("/images"))
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
            app.Run();
        }
    }
}
