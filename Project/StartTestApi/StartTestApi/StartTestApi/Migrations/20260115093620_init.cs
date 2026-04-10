using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StartTestApi.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDatumTijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EindDatumTijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGeannuleerd = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inschrijvingen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeelnemerNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeelnemerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InschrijfDatumTijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inschrijvingen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inschrijvingen_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "Beschrijving", "EindDatumTijd", "IsGeannuleerd", "StartDatumTijd", "Titel" },
                values: new object[,]
                {
                    { 1, "Leer grenzen aangeven en zelfzeker communiceren.", new DateTime(2026, 3, 10, 20, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2026, 3, 10, 18, 0, 0, 0, DateTimeKind.Unspecified), "Assertiviteitstraining voor beginners" },
                    { 2, "Rustige yogasessie om de dag af te sluiten.", new DateTime(2026, 3, 12, 21, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2026, 3, 12, 19, 30, 0, 0, DateTimeKind.Unspecified), "Avond-Yoga voor ontspanning" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inschrijvingen_EventId",
                table: "Inschrijvingen",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inschrijvingen");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
