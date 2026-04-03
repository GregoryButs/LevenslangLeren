using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class IntitializeDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BuyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxesLevelId = table.Column<int>(type: "int", nullable: false),
                    AmountStock = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Taxes_TaxesLevelId",
                        column: x => x.TaxesLevelId,
                        principalTable: "Taxes",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "AI Glasses" },
                    { 2, "Food" },
                    { 3, "Drinks" }
                });

            migrationBuilder.InsertData(
                table: "Taxes",
                columns: new[] { "Id", "Name", "TaxLevel" },
                values: new object[,]
                {
                    { 1, "Standaard", 21 },
                    { 2, "Uitzonderingen en maaltijden", 12 },
                    { 3, "Basisnoden", 6 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Active", "AmountStock", "BuyPrice", "CategoryId", "Code", "Name", "TaxesLevelId" },
                values: new object[,]
                {
                    { 1, true, 12, 589.99m, 1, "X1", "XReal One Pro", 1 },
                    { 2, false, 20, 399.99m, 1, "XAir", "XReal Air Pro", 1 },
                    { 3, true, 12, 1.00m, 2, "FoodApple", "Apple", 2 },
                    { 4, true, 12, 0.50m, 3, "DrinkWWater", "Water", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TaxesLevelId",
                table: "Products",
                column: "TaxesLevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Taxes");
        }
    }
}
