using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuctapCN.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskAssignmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentProgress = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskReports_TaskAssignments_TaskAssignmentId",
                        column: x => x.TaskAssignmentId,
                        principalTable: "TaskAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_CreatedDate",
                table: "TaskReports",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_IsRead",
                table: "TaskReports",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_TaskAssignmentId",
                table: "TaskReports",
                column: "TaskAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_UserId",
                table: "TaskReports",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskReports");
        }
    }
}
