using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherForecast.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", precision: 10, scale: 5, nullable: false),
                    Longitude = table.Column<double>(type: "REAL", precision: 10, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    FetchDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValidDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WeatherModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Temperature2m = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    ApparentTemperature = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    Precipitation = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    PrecipitationType = table.Column<string>(type: "TEXT", nullable: true),
                    PrecipitationProbability = table.Column<int>(type: "INTEGER", nullable: false),
                    WindSpeed10m = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    Humidity2m = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    PressureSurface = table.Column<double>(type: "REAL", precision: 8, scale: 2, nullable: false),
                    CloudCover = table.Column<int>(type: "INTEGER", nullable: false),
                    Visibility = table.Column<double>(type: "REAL", precision: 8, scale: 2, nullable: false),
                    UvIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forecasts_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Forecasts_WeatherModels_WeatherModelId",
                        column: x => x.WeatherModelId,
                        principalTable: "WeatherModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_LocationId_FetchDate",
                table: "Forecasts",
                columns: new[] { "LocationId", "FetchDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_ValidDate",
                table: "Forecasts",
                column: "ValidDate");

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_WeatherModelId",
                table: "Forecasts",
                column: "WeatherModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherModels_Name",
                table: "WeatherModels",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "WeatherModels");
        }
    }
}
