using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RealEstateApi.Migrations
{
    /// <inheritdoc />
    public partial class Add_RealEstates_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RealEstates",
                columns: table => new
                {
                    realEstateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    province = table.Column<string>(type: "text", nullable: false),
                    district = table.Column<string>(type: "text", nullable: false),
                    ward = table.Column<string>(type: "text", nullable: false),
                    street = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    area = table.Column<float>(type: "real", nullable: false),
                    roadWidth = table.Column<float>(type: "real", nullable: true),
                    floors = table.Column<int>(type: "integer", nullable: true),
                    bedrooms = table.Column<int>(type: "integer", nullable: true),
                    bathrooms = table.Column<int>(type: "integer", nullable: true),
                    direction = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    length = table.Column<float>(type: "real", nullable: false),
                    width = table.Column<float>(type: "real", nullable: false),
                    postedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstates", x => x.realEstateId);
                });

            migrationBuilder.CreateTable(
                name: "RealEstateImages",
                columns: table => new
                {
                    imageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imageUrl = table.Column<string>(type: "text", nullable: false),
                    realEstateId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealEstateImages", x => x.imageId);
                    table.ForeignKey(
                        name: "FK_RealEstateImages_RealEstates_realEstateId",
                        column: x => x.realEstateId,
                        principalTable: "RealEstates",
                        principalColumn: "realEstateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RealEstateImages_realEstateId",
                table: "RealEstateImages",
                column: "realEstateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RealEstateImages");

            migrationBuilder.DropTable(
                name: "RealEstates");
        }
    }
}
