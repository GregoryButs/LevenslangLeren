using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfsprakenbeheerPsycholoog.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260624000001_AddGoogleEventIdAndSentimentScore")]
    public partial class AddGoogleEventIdAndSentimentScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleEventId",
                table: "Afspraken",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SentimentScore",
                table: "Afspraken",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EmotioneleStabiliteit",
                table: "Patienten",
                type: "REAL",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Afspraken_GoogleEventId",
                table: "Afspraken",
                column: "GoogleEventId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Afspraken_GoogleEventId",
                table: "Afspraken");

            migrationBuilder.DropColumn(
                name: "GoogleEventId",
                table: "Afspraken");

            migrationBuilder.DropColumn(
                name: "SentimentScore",
                table: "Afspraken");

            migrationBuilder.DropColumn(
                name: "EmotioneleStabiliteit",
                table: "Patienten");
        }
    }
}
