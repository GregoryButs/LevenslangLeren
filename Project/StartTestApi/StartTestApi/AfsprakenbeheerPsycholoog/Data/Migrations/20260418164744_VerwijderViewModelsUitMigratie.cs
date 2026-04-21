using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    /// <inheritdoc />
    public partial class VerwijderViewModelsUitMigratie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AfspraakDetailViewModel");

            migrationBuilder.DropTable(
                name: "AfspraakListViewModel");

            migrationBuilder.DropTable(
                name: "PatientListViewModel");

            migrationBuilder.DropTable(
                name: "PatientDetailViewModel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AfspraakDetailViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AfspraakTypeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eindtijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kleurcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientTelefoon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Patient
                    = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Starttijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfspraakDetailViewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientDetailViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Achternaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DossierNummer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geboortedatum = table.Column<DateOnly>(type: "date", nullable: false),
                    IsGekoppeld = table.Column<bool>(type: "bit", nullable: false),
                    Telefoonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolledigeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Voornaam = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDetailViewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientListViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AantalAfspraken = table.Column<int>(type: "int", nullable: false),
                    DossierNummer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsGekoppeld = table.Column<bool>(type: "bit", nullable: false),
                    Telefoonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolledigeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientListViewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AfspraakListViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AfspraakTypeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eindtijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kleurcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientDetailViewModelId = table.Column<int>(type: "int", nullable: true),
                    PatientNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Starttijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfspraakListViewModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AfspraakListViewModel_PatientDetailViewModel_PatientDetailViewModelId",
                        column: x => x.PatientDetailViewModelId,
                        principalTable: "PatientDetailViewModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AfspraakListViewModel_PatientDetailViewModelId",
                table: "AfspraakListViewModel",
                column: "PatientDetailViewModelId");
        }
    }
}
