using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sector_13_Welfare_Society___Digital_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateDynamicContentTablesFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            // Check if tables exist in zidan schema before renaming
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'zidan' AND TABLE_NAME = 'PreviousCandidates')
                BEGIN
                    ALTER SCHEMA [dbo] TRANSFER [zidan].[PreviousCandidates];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'zidan' AND TABLE_NAME = 'LeadershipMessages')
                BEGIN
                    ALTER SCHEMA [dbo] TRANSFER [zidan].[LeadershipMessages];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'zidan' AND TABLE_NAME = 'ElectedCandidates')
                BEGIN
                    ALTER SCHEMA [dbo] TRANSFER [zidan].[ElectedCandidates];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ensure zidan schema exists
            migrationBuilder.EnsureSchema(
                name: "zidan");

            // Check if tables exist in dbo schema before renaming back
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'PreviousCandidates')
                BEGIN
                    ALTER SCHEMA [zidan] TRANSFER [dbo].[PreviousCandidates];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'LeadershipMessages')
                BEGIN
                    ALTER SCHEMA [zidan] TRANSFER [dbo].[LeadershipMessages];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ElectedCandidates')
                BEGIN
                    ALTER SCHEMA [zidan] TRANSFER [dbo].[ElectedCandidates];
                END
            ");
        }
    }
}
