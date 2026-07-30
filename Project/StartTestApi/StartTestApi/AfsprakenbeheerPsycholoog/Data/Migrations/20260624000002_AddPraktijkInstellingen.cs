using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260624000002_AddPraktijkInstellingen")]
    public partial class AddPraktijkInstellingen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PraktijkInstellingen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GoogleCalendarId = table.Column<string>(type: "TEXT", nullable: true),
                    MaandagActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaandagStart = table.Column<string>(type: "TEXT", nullable: true),
                    MaandagEinde = table.Column<string>(type: "TEXT", nullable: true),
                    DinsdagActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    DinsdagStart = table.Column<string>(type: "TEXT", nullable: true),
                    DinsdagEinde = table.Column<string>(type: "TEXT", nullable: true),
                    WoensdagActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    WoensdagStart = table.Column<string>(type: "TEXT", nullable: true),
                    WoensdagEinde = table.Column<string>(type: "TEXT", nullable: true),
                    DonderdagActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    DonderdagStart = table.Column<string>(type: "TEXT", nullable: true),
                    DonderdagEinde = table.Column<string>(type: "TEXT", nullable: true),
                    VrijdagActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    VrijdagStart = table.Column<string>(type: "TEXT", nullable: true),
                    VrijdagEinde = table.Column<string>(type: "TEXT", nullable: true),
                    ZaterdagActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    ZaterdagStart = table.Column<string>(type: "TEXT", nullable: true),
                    ZaterdagEinde = table.Column<string>(type: "TEXT", nullable: true),
                    ZondagActief = table.Column<bool>(type: "INTEGER", nullable: false),
                    ZondagStart = table.Column<string>(type: "TEXT", nullable: true),
                    ZondagEinde = table.Column<string>(type: "TEXT", nullable: true),
                    SlotDuurMinuten = table.Column<int>(type: "INTEGER", nullable: false),
                    BufferMinuten = table.Column<int>(type: "INTEGER", nullable: false),
                    LocatiePraktijk = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocatieGoogleMeet = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocatieTelefoon = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinimaalVoorafUren = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximaleToekomstDagen = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PraktijkInstellingen", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO \"PraktijkInstellingen\" (\"Id\", \"GoogleCalendarId\", \"MaandagActief\", \"MaandagStart\", \"MaandagEinde\", \"DinsdagActief\", \"DinsdagStart\", \"DinsdagEinde\", \"WoensdagActief\", \"WoensdagStart\", \"WoensdagEinde\", \"DonderdagActief\", \"DonderdagStart\", \"DonderdagEinde\", \"VrijdagActief\", \"VrijdagStart\", \"VrijdagEinde\", \"ZaterdagActief\", \"ZaterdagStart\", \"ZaterdagEinde\", \"ZondagActief\", \"ZondagStart\", \"ZondagEinde\", \"SlotDuurMinuten\", \"BufferMinuten\", \"LocatiePraktijk\", \"LocatieGoogleMeet\", \"LocatieTelefoon\", \"MinimaalVoorafUren\", \"MaximaleToekomstDagen\") VALUES (1, 'primary', 1, '09:00', '17:00', 1, '09:00', '17:00', 1, '09:00', '17:00', 1, '09:00', '17:00', 1, '09:00', '17:00', 0, '10:00', '14:00', 0, '10:00', '14:00', 60, 15, 1, 1, 1, 12, 30);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PraktijkInstellingen");
        }
    }
}
