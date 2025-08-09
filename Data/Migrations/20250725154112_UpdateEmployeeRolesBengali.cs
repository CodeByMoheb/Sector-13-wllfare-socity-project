using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeRolesBengali : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            // Create Employees table only if it does not already exist
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Employees](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Role] [nvarchar](50) NOT NULL,
        [JoiningDate] [datetime2] NOT NULL,
        [BaseSalary] [decimal](18,2) NOT NULL,
        [IsActive] [bit] NOT NULL,
        [CreatedAt] [datetime2] NOT NULL,
        [UpdatedAt] [datetime2] NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
");

            // Create Attendances table only if it does not already exist
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendances]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Attendances](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [EmployeeId] [int] NOT NULL,
        [Date] [datetime2] NOT NULL,
        [IsPresent] [bit] NOT NULL,
        [Remarks] [nvarchar](200) NULL,
        CONSTRAINT [PK_Attendances] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Attendances_Employees_EmployeeId] FOREIGN KEY([EmployeeId]) REFERENCES [dbo].[Employees]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Attendances_EmployeeId] ON [dbo].[Attendances]([EmployeeId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
