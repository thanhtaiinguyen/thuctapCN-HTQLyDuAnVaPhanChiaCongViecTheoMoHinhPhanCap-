using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Data;
using thuctapCN.Models;
using Microsoft.Data.SqlClient;

namespace thuctapCN.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, bool isInitialSetup = false)
        {
            using var context = new thuctapCNContext(
                serviceProvider.GetRequiredService<DbContextOptions<thuctapCNContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Tạo các roles
            string[] roleNames = { "Admin", "Management", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Cập nhật EmployeeCode cho các user cũ chưa có mã nhân viên
            if (isInitialSetup)
            {
                try
                {
                    var usersWithoutEmployeeCode = await context.Users
                        .Where(u => u.EmployeeCode == null || u.EmployeeCode == "")
                        .ToListAsync();
                    
                    int counter = 1;
                    foreach (var user in usersWithoutEmployeeCode)
                    {
                        // Tạo mã nhân viên tự động dựa trên email hoặc ID
                        var baseCode = user.Email?.Split('@')[0].ToUpper() ?? "USER";
                        var newEmployeeCode = $"{baseCode}{counter:D4}";
                        
                        // Đảm bảo mã không trùng
                        while (await context.Users.AnyAsync(u => u.EmployeeCode == newEmployeeCode))
                        {
                            counter++;
                            newEmployeeCode = $"{baseCode}{counter:D4}";
                        }
                        
                        user.EmployeeCode = newEmployeeCode;
                        counter++;
                    }
                    
                    if (usersWithoutEmployeeCode.Any())
                    {
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng tiếp tục
                    Console.WriteLine($"Lỗi khi cập nhật EmployeeCode: {ex.Message}");
                }
            }

            // Tạo user Admin
            var adminEmail = "admin@thuctapcn.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Quản trị viên",
                    EmployeeCode = "ADMIN001",
                    PhoneNumber = "0123456789",
                    Department = "IT",
                    Position = "Quản trị viên hệ thống"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Đảm bảo user đã có role Admin
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                
                // Đảm bảo user đã có EmployeeCode
                if (string.IsNullOrEmpty(adminUser.EmployeeCode))
                {
                    adminUser.EmployeeCode = "ADMIN001";
                    await userManager.UpdateAsync(adminUser);
                }
            }

            // Tạo user Management
            var managementEmail = "management@thuctapcn.com";
            var managementUser = await userManager.FindByEmailAsync(managementEmail);
            if (managementUser == null)
            {
                managementUser = new ApplicationUser
                {
                    UserName = managementEmail,
                    Email = managementEmail,
                    EmailConfirmed = true,
                    FullName = "Quản lý",
                    EmployeeCode = "MGT001",
                    PhoneNumber = "0987654321",
                    Department = "Quản lý",
                    Position = "Quản lý dự án"
                };

                var result = await userManager.CreateAsync(managementUser, "Management@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managementUser, "Management");
                }
            }
            else
            {
                // Đảm bảo user đã có role Management
                if (!await userManager.IsInRoleAsync(managementUser, "Management"))
                {
                    await userManager.AddToRoleAsync(managementUser, "Management");
                }
                
                // Đảm bảo user đã có EmployeeCode
                if (string.IsNullOrEmpty(managementUser.EmployeeCode))
                {
                    managementUser.EmployeeCode = "MGT001";
                    await userManager.UpdateAsync(managementUser);
                }
            }
        }

        // Method riêng để clean up dữ liệu trước khi migration
        public static async Task CleanupBeforeMigration(IServiceProvider serviceProvider)
        {
            using var context = new thuctapCNContext(
                serviceProvider.GetRequiredService<DbContextOptions<thuctapCNContext>>());

            try
            {
                // Sử dụng RAW SQL để tránh lỗi SqlNullValueException
                var sql = @"
                    DECLARE @counter INT = 1;
                    DECLARE @userId NVARCHAR(450);
                    DECLARE @newCode NVARCHAR(50);

                    DECLARE user_cursor CURSOR FOR
                    SELECT Id FROM AspNetUsers WHERE EmployeeCode IS NULL OR EmployeeCode = '';

                    OPEN user_cursor;
                    FETCH NEXT FROM user_cursor INTO @userId;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        SET @newCode = 'EMP' + RIGHT('0000' + CAST(@counter AS NVARCHAR), 4);
                        
                        WHILE EXISTS (SELECT 1 FROM AspNetUsers WHERE EmployeeCode = @newCode)
                        BEGIN
                            SET @counter = @counter + 1;
                            SET @newCode = 'EMP' + RIGHT('0000' + CAST(@counter AS NVARCHAR), 4);
                        END;
                        
                        UPDATE AspNetUsers SET EmployeeCode = @newCode WHERE Id = @userId;
                        SET @counter = @counter + 1;
                        
                        FETCH NEXT FROM user_cursor INTO @userId;
                    END;

                    CLOSE user_cursor;
                    DEALLOCATE user_cursor;";

                var rowsAffected = await context.Database.ExecuteSqlRawAsync(sql);
                Console.WriteLine($"Đã cleanup EmployeeCode cho users trong database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cleanup EmployeeCode: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.StackTrace}");
                // Không throw exception - để app có thể chạy
            }
        }
    }
}


