using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockBite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailySummaryRevenueBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CardRevenue",
                table: "DailySummaries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CashRevenue",
                table: "DailySummaries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StockPurchaseCost",
                table: "DailySummaries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardRevenue",
                table: "DailySummaries");

            migrationBuilder.DropColumn(
                name: "CashRevenue",
                table: "DailySummaries");

            migrationBuilder.DropColumn(
                name: "StockPurchaseCost",
                table: "DailySummaries");
        }
    }
}
