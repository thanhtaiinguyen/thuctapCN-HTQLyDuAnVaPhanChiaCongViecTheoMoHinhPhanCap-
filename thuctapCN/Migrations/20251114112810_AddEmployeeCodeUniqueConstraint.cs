using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace thuctapCN.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeCodeUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            // Drop index nếu đã tồn tại
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ApplicationUser_EmployeeCode' AND object_id = OBJECT_ID('AspNetUsers'))
                BEGIN
                    DROP INDEX IX_ApplicationUser_EmployeeCode ON AspNetUsers
                END
            ");

=======
>>>>>>> 7929cc674e75331a1e771f61be947e49f8e3755f
            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_EmployeeCode",
                table: "AspNetUsers",
                column: "EmployeeCode",
<<<<<<< HEAD
                unique: true,
                filter: "[EmployeeCode] IS NOT NULL");
=======
                unique: true);
>>>>>>> 7929cc674e75331a1e771f61be947e49f8e3755f
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUser_EmployeeCode",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
