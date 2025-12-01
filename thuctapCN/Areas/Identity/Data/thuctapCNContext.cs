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

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Cấu hình Mã nhân viên là duy nhất (khóa chính logic)
        // Chỉ tạo chỉ mục duy nhất nếu Mã nhân viên không null
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.EmployeeCode)
                  .IsUnique()
                  .HasDatabaseName("IX_ApplicationUser_EmployeeCode")
                  .HasFilter("[EmployeeCode] IS NOT NULL");
        });


        // Cấu hình Dự án
        builder.Entity<Project>(entity =>
        {
            // Mã dự án phải duy nhất
            entity.HasIndex(e => e.ProjectCode)
                  .IsUnique()
                  .HasDatabaseName("IX_Project_ProjectCode");

            // Tên dự án phải duy nhất
            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("IX_Project_Name");
        });

        // Cấu hình mối quan hệ Thành viên dự án
        builder.Entity<ProjectMember>(entity =>
        {
            entity.HasOne(pm => pm.Project)
                  .WithMany(p => p.ProjectMembers)
                  .HasForeignKey(pm => pm.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pm => pm.User)
                  .WithMany()
                  .HasForeignKey(pm => pm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Chỉ mục duy nhất tổng hợp: một người dùng không thể xuất hiện 2 lần trong cùng 1 dự án
            entity.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                  .IsUnique()
                  .HasDatabaseName("IX_ProjectMember_ProjectId_UserId");
        });

        // Cấu hình mối quan hệ Phân công công việc
        builder.Entity<TaskAssignment>(entity =>
        {
            entity.HasOne(t => t.Project)
                  .WithMany()
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.AssignedToUser)
                  .WithMany()
                  .HasForeignKey(t => t.AssignedToUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Chỉ mục để tăng hiệu suất truy vấn
            entity.HasIndex(t => t.ProjectId)
                  .HasDatabaseName("IX_TaskAssignment_ProjectId");

            entity.HasIndex(t => t.AssignedToUserId)
                  .HasDatabaseName("IX_TaskAssignment_AssignedToUserId");

            entity.HasIndex(t => t.Status)
                  .HasDatabaseName("IX_TaskAssignment_Status");

            entity.HasIndex(t => t.Deadline)
                  .HasDatabaseName("IX_TaskAssignment_Deadline");
        });

        // Cấu hình mối quan hệ Bình luận công việc
        builder.Entity<TaskComment>(entity =>
        {
            entity.HasOne(c => c.TaskAssignment)
                  .WithMany()
                  .HasForeignKey(c => c.TaskAssignmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Chỉ mục để tăng hiệu suất truy vấn
            entity.HasIndex(c => c.TaskAssignmentId)
                  .HasDatabaseName("IX_TaskComment_TaskAssignmentId");

            entity.HasIndex(c => c.UserId)
                  .HasDatabaseName("IX_TaskComment_UserId");

            entity.HasIndex(c => c.CreatedDate)
                  .HasDatabaseName("IX_TaskComment_CreatedDate");
        });
    }
}
