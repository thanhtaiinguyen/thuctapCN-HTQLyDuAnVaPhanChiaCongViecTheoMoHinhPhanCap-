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
        
        // Cấu hình EmployeeCode là unique (khóa chính logic)
        // Chỉ tạo unique index nếu EmployeeCode không null
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.EmployeeCode)
                  .IsUnique()
                  .HasDatabaseName("IX_ApplicationUser_EmployeeCode")
                  .HasFilter("[EmployeeCode] IS NOT NULL");
        });


        // Cấu hình Project
        builder.Entity<Project>(entity =>
        {
            // ProjectCode phải unique
            entity.HasIndex(e => e.ProjectCode)
                  .IsUnique()
                  .HasDatabaseName("IX_Project_ProjectCode");

            // Name phải unique
            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("IX_Project_Name");
        });

        // Cấu hình ProjectMember relationships
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

            // Composite unique index: một user không thể xuất hiện 2 lần trong cùng 1 project
            entity.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                  .IsUnique()
                  .HasDatabaseName("IX_ProjectMember_ProjectId_UserId");
        });

        // Cấu hình TaskAssignment relationships
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

            // Indexes for better query performance
            entity.HasIndex(t => t.ProjectId)
                  .HasDatabaseName("IX_TaskAssignment_ProjectId");

            entity.HasIndex(t => t.AssignedToUserId)
                  .HasDatabaseName("IX_TaskAssignment_AssignedToUserId");

            entity.HasIndex(t => t.Status)
                  .HasDatabaseName("IX_TaskAssignment_Status");

            entity.HasIndex(t => t.Deadline)
                  .HasDatabaseName("IX_TaskAssignment_Deadline");
        });

        // Cấu hình TaskComment relationships
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

            // Indexes for better query performance
            entity.HasIndex(c => c.TaskAssignmentId)
                  .HasDatabaseName("IX_TaskComment_TaskAssignmentId");

            entity.HasIndex(c => c.UserId)
                  .HasDatabaseName("IX_TaskComment_UserId");

            entity.HasIndex(c => c.CreatedDate)
                  .HasDatabaseName("IX_TaskComment_CreatedDate");
        });
    }
}
