using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockBite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSplitPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CardAmount",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashAmount",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CashAmount",
                table: "Orders");
        }
    }
}
