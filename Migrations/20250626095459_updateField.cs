using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateApi.Migrations
{
    /// <inheritdoc />
    public partial class updateField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "createdBy",
                table: "RealEstates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "highlightedDate",
                table: "RealEstates",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdBy",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "highlightedDate",
                table: "RealEstates");
        }
    }
}
