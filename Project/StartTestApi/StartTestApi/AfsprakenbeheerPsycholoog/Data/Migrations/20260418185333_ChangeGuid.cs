using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReeksId",
                table: "Afspraken",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 1,
                column: "ReeksId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 2,
                column: "ReeksId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 3,
                column: "ReeksId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Afspraken",
                keyColumn: "Id",
                keyValue: 4,
                column: "ReeksId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReeksId",
                table: "Afspraken");
        }
    }
}
