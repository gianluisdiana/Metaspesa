using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class RefactoringProductHistoryForFormat : Migration {
    private static readonly string[] ProductHistoryIdentityColumns = ["id", "product_id"];
    private static readonly string[] ProductHistoryFormatColumns = ["product_format_id", "product_id"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropForeignKey(
          name: "FK_products_history_product_formats_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropIndex(
          name: "IX_products_history_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropCheckConstraint(
          name: "chk_positive_price",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropColumn(
          name: "ProductFormatDbEntityId",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropColumn(
          name: "image_url",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropColumn(
          name: "quantity",
          schema: "market",
          table: "products_history");

      migrationBuilder.AlterTable(
          name: "products_history",
          schema: "market",
          comment: "Historical price and format data for products, tracking changes over time.",
          oldComment: "Historical price and quantity data for products, tracking changes over time");

      migrationBuilder.AddColumn<int>(
          name: "ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items",
          type: "integer",
          nullable: true);

      migrationBuilder.AlterColumn<decimal>(
          name: "price",
          schema: "market",
          table: "products_history",
          type: "numeric(10,2)",
          precision: 10,
          scale: 2,
          nullable: false,
          oldClrType: typeof(decimal),
          oldType: "numeric(18,2)",
          oldPrecision: 18,
          oldScale: 2);

      migrationBuilder.AddColumn<int>(
          name: "product_format_id",
          schema: "market",
          table: "products_history",
          type: "integer",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.AddUniqueConstraint(
          name: "ak_product_history_id_product_id",
          schema: "market",
          table: "products_history",
          columns: ProductHistoryIdentityColumns);

      migrationBuilder.CreateIndex(
          name: "IX_shopping_items_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items",
          column: "ProductsHistoryDbEntityId");

      migrationBuilder.CreateIndex(
          name: "idx_product_history_id_product_id",
          schema: "market",
          table: "products_history",
          columns: ProductHistoryIdentityColumns,
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_product_history_product_format_id",
          schema: "market",
          table: "products_history",
          column: "product_format_id");

      migrationBuilder.CreateIndex(
          name: "IX_products_history_product_format_id_product_id",
          schema: "market",
          table: "products_history",
          columns: ProductHistoryFormatColumns);

      migrationBuilder.AddCheckConstraint(
          name: "chk_positive_price",
          schema: "market",
          table: "products_history",
          sql: "price > 0.00");

      migrationBuilder.AddForeignKey(
          name: "FK_products_history_product_formats_product_format_id_product_~",
          schema: "market",
          table: "products_history",
          columns: ProductHistoryFormatColumns,
          principalSchema: "market",
          principalTable: "product_formats",
          principalColumns: ProductHistoryIdentityColumns,
          onDelete: ReferentialAction.Cascade);

      migrationBuilder.AddForeignKey(
          name: "FK_shopping_items_products_history_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items",
          column: "ProductsHistoryDbEntityId",
          principalSchema: "market",
          principalTable: "products_history",
          principalColumn: "id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropForeignKey(
          name: "FK_products_history_product_formats_product_format_id_product_~",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_products_history_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "IX_shopping_items_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropUniqueConstraint(
          name: "ak_product_history_id_product_id",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropIndex(
          name: "idx_product_history_id_product_id",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropIndex(
          name: "idx_product_history_product_format_id",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropIndex(
          name: "IX_products_history_product_format_id_product_id",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropCheckConstraint(
          name: "chk_positive_price",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropColumn(
          name: "ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "product_format_id",
          schema: "market",
          table: "products_history");

      migrationBuilder.AlterTable(
          name: "products_history",
          schema: "market",
          comment: "Historical price and quantity data for products, tracking changes over time",
          oldComment: "Historical price and format data for products, tracking changes over time.");

      migrationBuilder.AlterColumn<decimal>(
          name: "price",
          schema: "market",
          table: "products_history",
          type: "numeric(18,2)",
          precision: 18,
          scale: 2,
          nullable: false,
          oldClrType: typeof(decimal),
          oldType: "numeric(10,2)",
          oldPrecision: 10,
          oldScale: 2);

      migrationBuilder.AddColumn<int>(
          name: "ProductFormatDbEntityId",
          schema: "market",
          table: "products_history",
          type: "integer",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "image_url",
          schema: "market",
          table: "products_history",
          type: "text",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "quantity",
          schema: "market",
          table: "products_history",
          type: "text",
          nullable: false,
          defaultValue: "");

      migrationBuilder.CreateIndex(
          name: "IX_products_history_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history",
          column: "ProductFormatDbEntityId");

      migrationBuilder.AddCheckConstraint(
          name: "chk_positive_price",
          schema: "market",
          table: "products_history",
          sql: "price >= 0.00");

      migrationBuilder.AddForeignKey(
          name: "FK_products_history_product_formats_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history",
          column: "ProductFormatDbEntityId",
          principalSchema: "market",
          principalTable: "product_formats",
          principalColumn: "id");
    }
  }
}