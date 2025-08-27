using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarnManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameTypeToSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropColumn(
                name: "Quanity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AgeDays",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "LifeTimeDays",
                table: "Animals");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Products",
                type: "int",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0);
        }

      
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "Quanity",
                table: "Products",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AgeDays",
                table: "Animals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Animals",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "LifeTimeDays",
                table: "Animals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
