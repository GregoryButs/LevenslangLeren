using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260624000003_AddSecondIntervals")]
    public partial class AddSecondIntervals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "Maandag2Actief", table: "PraktijkInstellingen", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "MaandagStart2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "MaandagEinde2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);

            migrationBuilder.AddColumn<bool>(name: "Dinsdag2Actief", table: "PraktijkInstellingen", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "DinsdagStart2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "DinsdagEinde2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);

            migrationBuilder.AddColumn<bool>(name: "Woensdag2Actief", table: "PraktijkInstellingen", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "WoensdagStart2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "WoensdagEinde2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);

            migrationBuilder.AddColumn<bool>(name: "Donderdag2Actief", table: "PraktijkInstellingen", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "DonderdagStart2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "DonderdagEinde2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);

            migrationBuilder.AddColumn<bool>(name: "Vrijdag2Actief", table: "PraktijkInstellingen", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "VrijdagStart2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "VrijdagEinde2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);

            migrationBuilder.AddColumn<bool>(name: "Zaterdag2Actief", table: "PraktijkInstellingen", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "ZaterdagStart2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ZaterdagEinde2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);

            migrationBuilder.AddColumn<bool>(name: "Zondag2Actief", table: "PraktijkInstellingen", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "ZondagStart2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ZondagEinde2", table: "PraktijkInstellingen", type: "TEXT", nullable: true);

            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"MaandagEinde\" = '12:00', \"Maandag2Actief\" = 1, \"MaandagStart2\" = '13:00', \"MaandagEinde2\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"DinsdagEinde\" = '12:00', \"Dinsdag2Actief\" = 1, \"DinsdagStart2\" = '13:00', \"DinsdagEinde2\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"WoensdagEinde\" = '12:00', \"Woensdag2Actief\" = 1, \"WoensdagStart2\" = '13:00', \"WoensdagEinde2\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"DonderdagEinde\" = '12:00', \"Donderdag2Actief\" = 1, \"DonderdagStart2\" = '13:00', \"DonderdagEinde2\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"VrijdagEinde\" = '12:00', \"Vrijdag2Actief\" = 1, \"VrijdagStart2\" = '13:00', \"VrijdagEinde2\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"ZaterdagEinde\" = '12:00', \"Zaterdag2Actief\" = 0, \"ZaterdagStart2\" = '13:00', \"ZaterdagEinde2\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"ZondagEinde\" = '12:00', \"Zondag2Actief\" = 0, \"ZondagStart2\" = '13:00', \"ZondagEinde2\" = '17:00' WHERE \"Id\" = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Maandag2Actief", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "MaandagStart2", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "MaandagEinde2", table: "PraktijkInstellingen");

            migrationBuilder.DropColumn(name: "Dinsdag2Actief", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "DinsdagStart2", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "DinsdagEinde2", table: "PraktijkInstellingen");

            migrationBuilder.DropColumn(name: "Woensdag2Actief", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "WoensdagStart2", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "WoensdagEinde2", table: "PraktijkInstellingen");

            migrationBuilder.DropColumn(name: "Donderdag2Actief", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "DonderdagStart2", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "DonderdagEinde2", table: "PraktijkInstellingen");

            migrationBuilder.DropColumn(name: "Vrijdag2Actief", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "VrijdagStart2", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "VrijdagEinde2", table: "PraktijkInstellingen");

            migrationBuilder.DropColumn(name: "Zaterdag2Actief", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "ZaterdagStart2", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "ZaterdagEinde2", table: "PraktijkInstellingen");

            migrationBuilder.DropColumn(name: "Zondag2Actief", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "ZondagStart2", table: "PraktijkInstellingen");
            migrationBuilder.DropColumn(name: "ZondagEinde2", table: "PraktijkInstellingen");

            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"MaandagEinde\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"DinsdagEinde\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"WoensdagEinde\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"DonderdagEinde\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"VrijdagEinde\" = '17:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"ZaterdagEinde\" = '14:00' WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"PraktijkInstellingen\" SET \"ZondagEinde\" = '14:00' WHERE \"Id\" = 1;");
        }
    }
}
