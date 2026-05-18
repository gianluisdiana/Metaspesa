using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class UseDecimalMoneyColumns : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.AlterColumn<decimal>(
          name: "price",
          schema: "shopping",
          table: "shopping_items",
          type: "numeric(18,2)",
          precision: 18,
          scale: 2,
          nullable: false,
          oldClrType: typeof(float),
          oldType: "real");

      migrationBuilder.AlterColumn<decimal>(
          name: "last_known_price",
          schema: "shopping",
          table: "registered_items",
          type: "numeric(18,2)",
          precision: 18,
          scale: 2,
          nullable: false,
          oldClrType: typeof(float),
          oldType: "real");

      migrationBuilder.AlterColumn<decimal>(
          name: "price_paid",
          schema: "shopping",
          table: "purchases",
          type: "numeric(18,2)",
          precision: 18,
          scale: 2,
          nullable: false,
          oldClrType: typeof(float),
          oldType: "real");

      migrationBuilder.AlterColumn<decimal>(
          name: "price",
          schema: "market",
          table: "products_history",
          type: "numeric(18,2)",
          precision: 18,
          scale: 2,
          nullable: false,
          oldClrType: typeof(float),
          oldType: "real");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.AlterColumn<float>(
          name: "price",
          schema: "shopping",
          table: "shopping_items",
          type: "real",
          nullable: false,
          oldClrType: typeof(decimal),
          oldType: "numeric(18,2)",
          oldPrecision: 18,
          oldScale: 2);

      migrationBuilder.AlterColumn<float>(
          name: "last_known_price",
          schema: "shopping",
          table: "registered_items",
          type: "real",
          nullable: false,
          oldClrType: typeof(decimal),
          oldType: "numeric(18,2)",
          oldPrecision: 18,
          oldScale: 2);

      migrationBuilder.AlterColumn<float>(
          name: "price_paid",
          schema: "shopping",
          table: "purchases",
          type: "real",
          nullable: false,
          oldClrType: typeof(decimal),
          oldType: "numeric(18,2)",
          oldPrecision: 18,
          oldScale: 2);

      migrationBuilder.AlterColumn<float>(
          name: "price",
          schema: "market",
          table: "products_history",
          type: "real",
          nullable: false,
          oldClrType: typeof(decimal),
          oldType: "numeric(18,2)",
          oldPrecision: 18,
          oldScale: 2);
    }
  }
}