using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehaviourPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActief",
                table: "Patienten",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerwijderdOp",
                table: "Patienten",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerwijderdReden",
                table: "Patienten",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AfspraakDetailViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientVolledigeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientTelefoon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AfspraakTypeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kleurcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Starttijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Eindtijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Voornaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Achternaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolledigeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geboortedatum = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefoonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DossierNummer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsGekoppeld = table.Column<bool>(type: "bit", nullable: false)
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
                    VolledigeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefoonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DossierNummer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AantalAfspraken = table.Column<int>(type: "int", nullable: false),
                    IsGekoppeld = table.Column<bool>(type: "bit", nullable: false)
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
                    PatientNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AfspraakTypeNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Starttijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Eindtijd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Kleurcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientDetailViewModelId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.UpdateData(
                table: "Patienten",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsActief", "VerwijderdOp", "VerwijderdReden" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "Patienten",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsActief", "VerwijderdOp", "VerwijderdReden" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "Patienten",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsActief", "VerwijderdOp", "VerwijderdReden" },
                values: new object[] { true, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_AfspraakListViewModel_PatientDetailViewModelId",
                table: "AfspraakListViewModel",
                column: "PatientDetailViewModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AfspraakDetailViewModel");

            migrationBuilder.DropTable(
                name: "AfspraakListViewModel");

            migrationBuilder.DropTable(
                name: "PatientListViewModel");

            migrationBuilder.DropTable(
                name: "PatientDetailViewModel");

            migrationBuilder.DropColumn(
                name: "IsActief",
                table: "Patienten");

            migrationBuilder.DropColumn(
                name: "VerwijderdOp",
                table: "Patienten");

            migrationBuilder.DropColumn(
                name: "VerwijderdReden",
                table: "Patienten");
        }
    }
}
