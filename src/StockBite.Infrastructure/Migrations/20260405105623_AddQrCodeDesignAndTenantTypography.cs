using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockBite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodeDesignAndTenantTypography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                table: "Tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PublicUrl",
                table: "MenuQrCodes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "MenuQrCodes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "MenuQrCodes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "BgColor",
                table: "MenuQrCodes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "MenuQrCodes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "MenuQrCodes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "MenuQrCodes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QrMenuTemplate",
                table: "MenuQrCodes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                table: "MenuQrCodes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TextColor",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BgColor",
                table: "MenuQrCodes");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "MenuQrCodes");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "MenuQrCodes");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "MenuQrCodes");

            migrationBuilder.DropColumn(
                name: "QrMenuTemplate",
                table: "MenuQrCodes");

            migrationBuilder.DropColumn(
                name: "TextColor",
                table: "MenuQrCodes");

            migrationBuilder.AlterColumn<string>(
                name: "PublicUrl",
                table: "MenuQrCodes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "MenuQrCodes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "MenuQrCodes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
