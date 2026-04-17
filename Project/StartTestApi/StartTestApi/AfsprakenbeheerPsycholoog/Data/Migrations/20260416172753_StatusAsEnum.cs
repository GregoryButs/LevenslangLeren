using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    /// <inheritdoc />
    public partial class StatusAsEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Afspraken",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 1,
                column: "Status",
                value: "Gepland");

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 2,
                column: "Status",
                value: "Gepland");

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 3,
                column: "Status",
                value: "Voltooid");

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 4,
                column: "Status",
                value: "Geannuleerd");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Afspraken",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 1,
                column: "Status",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 2,
                column: "Status",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 3,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 4,
                column: "Status",
                value: 2);
        }
    }
}
