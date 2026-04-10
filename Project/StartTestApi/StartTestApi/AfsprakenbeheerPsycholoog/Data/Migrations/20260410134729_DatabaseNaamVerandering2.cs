using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseNaamVerandering2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AfspraakTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandaardDuurMinuten = table.Column<int>(type: "int", nullable: false),
                    Kleurcode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfspraakTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patienten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Voornaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Achternaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geboortedatum = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefoonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DossierNummer = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patienten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Afspraken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Starttijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Eindtijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Afspraken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Afspraken_AfspraakTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "AfspraakTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Afspraken_Patienten_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patienten",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AfspraakTypes",
                columns: new[] { "Id", "Kleurcode", "Naam", "StandaardDuurMinuten" },
                values: new object[,]
                {
                    { 1, "#4A90D9", "Intake", 90 },
                    { 2, "#7ED321", "Therapie", 60 },
                    { 3, "#F5A623", "Evaluatie", 45 },
                    { 4, "#D0021B", "Crisis", 30 }
                });

            migrationBuilder.InsertData(
                table: "Patienten",
                columns: new[] { "Id", "Achternaam", "DossierNummer", "Email", "Geboortedatum", "Telefoonnummer", "Voornaam" },
                values: new object[,]
                {
                    { 1, "Janssens", "DOS-001", "jan@test.be", new DateOnly(1985, 3, 12), "0471000001", "Jan" },
                    { 2, "Peeters", null, "marie@test.be", new DateOnly(1992, 7, 4), "0471000002", "Marie" },
                    { 3, "De Smedt", "DOS-002", "pieter@test.be", new DateOnly(1978, 11, 20), "0471000003", "Pieter" }
                });

            migrationBuilder.InsertData(
                table: "Afspraken",
                columns: new[] { "Id", "Eindtijd", "Opmerkingen", "PatientId", "Starttijd", "Status", "TypeId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 4, 14, 11, 30, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 4, 14, 10, 0, 0, 0, DateTimeKind.Unspecified), 0, 1 },
                    { 2, new DateTime(2026, 4, 15, 15, 0, 0, 0, DateTimeKind.Unspecified), null, 2, new DateTime(2026, 4, 15, 14, 0, 0, 0, DateTimeKind.Unspecified), 0, 2 },
                    { 3, new DateTime(2026, 4, 1, 9, 45, 0, 0, DateTimeKind.Unspecified), null, 1, new DateTime(2026, 4, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), 1, 3 },
                    { 4, new DateTime(2026, 3, 20, 17, 0, 0, 0, DateTimeKind.Unspecified), "Patiënt heeft afgezegd", 3, new DateTime(2026, 3, 20, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Afspraken_PatientId",
                table: "Afspraken",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Afspraken_TypeId",
                table: "Afspraken",
                column: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Afspraken");

            migrationBuilder.DropTable(
                name: "AfspraakTypes");

            migrationBuilder.DropTable(
                name: "Patienten");
        }
    }
}
