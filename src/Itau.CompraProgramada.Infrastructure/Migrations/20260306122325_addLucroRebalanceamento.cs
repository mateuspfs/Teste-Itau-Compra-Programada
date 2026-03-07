using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itau.CompraProgramada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addLucroRebalanceamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TickerVendido",
                table: "Rebalanceamentos",
                type: "varchar(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldMaxLength: 12)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "TickerComprado",
                table: "Rebalanceamentos",
                type: "varchar(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldMaxLength: 12)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Lucro",
                table: "Rebalanceamentos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lucro",
                table: "Rebalanceamentos");

            migrationBuilder.UpdateData(
                table: "Rebalanceamentos",
                keyColumn: "TickerVendido",
                keyValue: null,
                column: "TickerVendido",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "TickerVendido",
                table: "Rebalanceamentos",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldMaxLength: 12,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Rebalanceamentos",
                keyColumn: "TickerComprado",
                keyValue: null,
                column: "TickerComprado",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "TickerComprado",
                table: "Rebalanceamentos",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldMaxLength: 12,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
