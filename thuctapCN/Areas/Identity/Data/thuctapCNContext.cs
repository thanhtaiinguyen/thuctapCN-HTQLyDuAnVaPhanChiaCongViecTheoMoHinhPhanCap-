using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using thuctapCN.Models;

namespace thuctapCN.Data;

public class thuctapCNContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public thuctapCNContext(DbContextOptions<thuctapCNContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Cấu hình EmployeeCode là unique (khóa chính logic)
        // Chỉ tạo unique index nếu EmployeeCode không null
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.EmployeeCode)
                  .IsUnique()
                  .HasDatabaseName("IX_ApplicationUser_EmployeeCode")
                  .HasFilter("[EmployeeCode] IS NOT NULL");
        });
    }
}
