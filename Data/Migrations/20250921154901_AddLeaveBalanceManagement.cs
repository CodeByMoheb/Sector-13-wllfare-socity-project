using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveBalanceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safe table transfers - only transfer if they exist in zidan schema
            string[] tablesToTransfer = {
                "Shifts", "PermanentMembers", "Notices", "Leaves", "Employees", 
                "Donors", "Attendances", "AspNetUserTokens", "AspNetUsers", 
                "AspNetUserRoles", "AspNetUserLogins", "AspNetUserClaims", 
                "AspNetRoles", "AspNetRoleClaims", "ApprovalRequests"
            };

            foreach (var tableName in tablesToTransfer)
            {
                migrationBuilder.Sql($@"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'zidan' AND TABLE_NAME = '{tableName}')
                    BEGIN
                        ALTER SCHEMA [dbo] TRANSFER [zidan].[{tableName}];
                    END
                ");
            }

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalEntitled = table.Column<int>(type: "int", nullable: false),
                    Used = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Pending = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_Year_LeaveType",
                schema: "dbo",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "Year", "LeaveType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveBalances",
                schema: "dbo");

            migrationBuilder.EnsureSchema(
                name: "zidan");

            // Safe table transfers back - only transfer if they exist in dbo schema
            string[] tablesToTransfer = {
                "Shifts", "PermanentMembers", "Notices", "Leaves", "Employees", 
                "Donors", "Attendances", "AspNetUserTokens", "AspNetUsers", 
                "AspNetUserRoles", "AspNetUserLogins", "AspNetUserClaims", 
                "AspNetRoles", "AspNetRoleClaims", "ApprovalRequests"
            };

            foreach (var tableName in tablesToTransfer)
            {
                migrationBuilder.Sql($@"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '{tableName}')
                    BEGIN
                        ALTER SCHEMA [zidan] TRANSFER [dbo].[{tableName}];
                    END
                ");
            }
        }
    }
}
