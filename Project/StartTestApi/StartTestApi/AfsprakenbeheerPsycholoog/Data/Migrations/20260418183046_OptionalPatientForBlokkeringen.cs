using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptionalPatientForBlokkeringen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "Afspraken",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "VereistPatient",
                table: "AfspraakTypes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "AfspraakTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VereistPatient",
                value: true);

            migrationBuilder.UpdateData(
                table: "AfspraakTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VereistPatient",
                value: true);

            migrationBuilder.UpdateData(
                table: "AfspraakTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VereistPatient",
                value: true);

            migrationBuilder.UpdateData(
                table: "AfspraakTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VereistPatient",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VereistPatient",
                table: "AfspraakTypes");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "Afspraken",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
