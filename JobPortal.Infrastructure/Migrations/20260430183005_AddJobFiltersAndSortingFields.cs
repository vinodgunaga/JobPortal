using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobFiltersAndSortingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceLevel",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JobType",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxSalary",
                table: "Jobs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinSalary",
                table: "Jobs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MaxSalary",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MinSalary",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Jobs");
        }
    }
}
