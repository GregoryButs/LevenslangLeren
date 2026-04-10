using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StartTestApi.Migrations
{
    /// <inheritdoc />
    public partial class adddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Inschrijvingen",
                columns: new[] { "Id", "DeelnemerEmail", "DeelnemerNaam", "EventId", "InschrijfDatumTijd" },
                values: new object[,]
                {
                    { 1, "jan@jansen.com", "Jan Jansen", 1, new DateTime(2026, 2, 20, 14, 15, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "piet@gmail.com", "Piet Pietersen", 1, new DateTime(2026, 2, 22, 10, 30, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Inschrijvingen",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Inschrijvingen",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
