using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JoltXServer.Migrations
{
    /// <inheritdoc />
    public partial class StrategiesAndTrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "93a7b0b3-d950-485f-a355-3539b6c20a99");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ae15d21c-a955-4be6-b9f2-fabbfe2c9c38");

            migrationBuilder.CreateTable(
                name: "Strategy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuyCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SellCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PercentProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxRunUp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDrawDown = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SharpeRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosedTrades = table.Column<int>(type: "int", nullable: false),
                    OpenTrades = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Strategy_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StrategyId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Signal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntryTime = table.Column<long>(type: "bigint", nullable: false),
                    ExitTime = table.Column<long>(type: "bigint", nullable: false),
                    EntryPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Profit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CumProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trade_Strategy_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3e893d61-5f4c-47a3-92cc-9a9766b76db7", "2", "User", "User" },
                    { "587200e2-2d99-485d-8db0-e466e6e1868f", "1", "Admin", "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Strategy_UserId",
                table: "Strategy",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trade_StrategyId",
                table: "Trade",
                column: "StrategyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trade");

            migrationBuilder.DropTable(
                name: "Strategy");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3e893d61-5f4c-47a3-92cc-9a9766b76db7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "587200e2-2d99-485d-8db0-e466e6e1868f");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "93a7b0b3-d950-485f-a355-3539b6c20a99", "1", "Admin", "Admin" },
                    { "ae15d21c-a955-4be6-b9f2-fabbfe2c9c38", "2", "User", "User" }
                });
        }
    }
}
