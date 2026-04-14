using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockBite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTimer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTimerPaused",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimerLastStartedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TimerOffsetSeconds",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTimerPaused",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TimerLastStartedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TimerOffsetSeconds",
                table: "Orders");
        }
    }
}
