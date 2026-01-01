using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherForecastv2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", precision: 10, scale: 3, nullable: false),
                    Longitude = table.Column<double>(type: "REAL", precision: 10, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Provider = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Forecast",
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
                    UvIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecast", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forecast_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Forecast_Location_LocationId1",
                        column: x => x.LocationId1,
                        principalTable: "Location",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Forecast_WeatherModel_WeatherModelId",
                        column: x => x.WeatherModelId,
                        principalTable: "WeatherModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forecast_LocationId",
                table: "Forecast",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Forecast_LocationId1",
                table: "Forecast",
                column: "LocationId1");

            migrationBuilder.CreateIndex(
                name: "IX_Forecast_WeatherModelId",
                table: "Forecast",
                column: "WeatherModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forecast");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "WeatherModel");
        }
    }
}
