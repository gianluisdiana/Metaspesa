using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  [DbContext(typeof(MainContext))]
  [Migration("20260628120000_ShoppingItemsReferenceProductFormat")]
  public partial class ShoppingItemsReferenceProductFormat : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_products_history_product_history_id_product_~",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "idx_shopping_item_list_product",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "idx_shopping_item_product_history_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "IX_shopping_items_product_history_id_product_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.RenameColumn(
          name: "product_history_id",
          schema: "shopping",
          table: "shopping_items",
          newName: "product_format_id");

      migrationBuilder.Sql("""
        UPDATE shopping.shopping_items AS si
        SET product_format_id = ph.product_format_id
        FROM market.products_history AS ph
        WHERE si.product_format_id = ph.id;
        """);

      migrationBuilder.AlterTable(
          name: "shopping_items",
          schema: "shopping",
          comment: "Items that belong to a shopping list, representing planned purchases.\r\nEach line points to an existing product format. Price resolves from the\r\nlatest product history row for that format when the list is read or bought.",
          oldComment: "Items that belong to a shopping list, representing planned purchases.\r\nEach line points to an existing market product and the exact product history\r\nrow used when the item was added, so price and format are explicit.");

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_list_product_format",
          schema: "shopping",
          table: "shopping_items",
          columns: ["shopping_list_id", "product_format_id"],
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_product_format_id",
          schema: "shopping",
          table: "shopping_items",
          column: "product_format_id");

      migrationBuilder.CreateIndex(
          name: "IX_shopping_items_product_format_id_product_id",
          schema: "shopping",
          table: "shopping_items",
          columns: ["product_format_id", "product_id"]);

      migrationBuilder.AddForeignKey(
          name: "FK_shopping_items_product_formats_product_format_id_product_id",
          schema: "shopping",
          table: "shopping_items",
          columns: ["product_format_id", "product_id"],
          principalSchema: "market",
          principalTable: "product_formats",
          principalColumns: ["id", "product_id"],
          onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_product_formats_product_format_id_product_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "idx_shopping_item_list_product_format",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "idx_shopping_item_product_format_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "IX_shopping_items_product_format_id_product_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.RenameColumn(
          name: "product_format_id",
          schema: "shopping",
          table: "shopping_items",
          newName: "product_history_id");

      migrationBuilder.Sql("""
        UPDATE shopping.shopping_items AS si
        SET product_history_id = latest.id
        FROM (
          SELECT DISTINCT ON (product_format_id)
            id,
            product_format_id
          FROM market.products_history
          ORDER BY product_format_id, created_at DESC, id DESC
        ) AS latest
        WHERE si.product_history_id = latest.product_format_id;
        """);

      migrationBuilder.AlterTable(
          name: "shopping_items",
          schema: "shopping",
          comment: "Items that belong to a shopping list, representing planned purchases.\r\nEach line points to an existing market product and the exact product history\r\nrow used when the item was added, so price and format are explicit.",
          oldComment: "Items that belong to a shopping list, representing planned purchases.\r\nEach line points to an existing product format. Price resolves from the\r\nlatest product history row for that format when the list is read or bought.");

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_list_product",
          schema: "shopping",
          table: "shopping_items",
          columns: ["shopping_list_id", "product_id"],
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_product_history_id",
          schema: "shopping",
          table: "shopping_items",
          column: "product_history_id");

      migrationBuilder.CreateIndex(
          name: "IX_shopping_items_product_history_id_product_id",
          schema: "shopping",
          table: "shopping_items",
          columns: ["product_history_id", "product_id"]);

      migrationBuilder.AddForeignKey(
          name: "FK_shopping_items_products_history_product_history_id_product_~",
          schema: "shopping",
          table: "shopping_items",
          columns: ["product_history_id", "product_id"],
          principalSchema: "market",
          principalTable: "products_history",
          principalColumns: ["id", "product_id"],
          onDelete: ReferentialAction.Restrict);
    }
  }
}