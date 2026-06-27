using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class ReferencingHistoryForPriceAndFormat : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropForeignKey(
          name: "FK_purchases_products_product_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropForeignKey(
          name: "FK_purchases_registered_items_registered_item_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropForeignKey(
          name: "FK_purchases_super_markets_super_market_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_products_history_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropTable(
          name: "registered_items",
          schema: "shopping");

      migrationBuilder.DropIndex(
          name: "IX_shopping_items_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropCheckConstraint(
          name: "chk_shopping_item_positive_price",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "idx_purchase_product_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropIndex(
          name: "idx_purchase_registered_item_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropCheckConstraint(
          name: "chk_positive_price_paid",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropColumn(
          name: "ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "name",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "price",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "quantity",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "price_paid",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropColumn(
          name: "product_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropColumn(
          name: "quantity",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropColumn(
          name: "registered_item_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.RenameColumn(
          name: "super_market_id",
          schema: "shopping",
          table: "purchases",
          newName: "shopping_list_id");

      migrationBuilder.RenameIndex(
          name: "idx_purchase_user_uid",
          schema: "shopping",
          table: "purchases",
          newName: "idx_purchases_user_uid");

      migrationBuilder.RenameIndex(
          name: "idx_purchase_super_market_id",
          schema: "shopping",
          table: "purchases",
          newName: "idx_purchases_shopping_list_id");

      migrationBuilder.RenameIndex(
          name: "idx_purchase_purchased_at",
          schema: "shopping",
          table: "purchases",
          newName: "idx_purchases_purchased_at");

      migrationBuilder.AlterTable(
          name: "shopping_items",
          schema: "shopping",
          comment: "Items that belong to a shopping list, representing planned purchases.\r\nEach line points to an existing market product and the exact product history\r\nrow used when the item was added, so price and format are explicit.",
          oldComment: "Items that belong to a shopping list, representing planned purchases");

      migrationBuilder.AlterTable(
          name: "purchases",
          schema: "shopping",
          comment: "Purchase receipt header. It records who bought, when, and optionally which\r\nshopping list was checked out. Product lines live in purchase_items.",
          oldComment: "Records the actual act of buying an item — links shopping, registered\r\nitems, and the market. Core of savings analytics.");

      migrationBuilder.AlterColumn<bool>(
          name: "is_checked",
          schema: "shopping",
          table: "shopping_items",
          type: "boolean",
          nullable: false,
          defaultValue: false,
          oldClrType: typeof(bool),
          oldType: "boolean");

      migrationBuilder.AddColumn<int>(
          name: "amount",
          schema: "shopping",
          table: "shopping_items",
          type: "integer",
          nullable: false,
          defaultValue: 1,
          comment: "How many units/packages of the product are planned");

      migrationBuilder.AddColumn<int>(
          name: "product_history_id",
          schema: "shopping",
          table: "shopping_items",
          type: "integer",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.AddColumn<int>(
          name: "product_id",
          schema: "shopping",
          table: "shopping_items",
          type: "integer",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.CreateTable(
          name: "purchase_items",
          schema: "shopping",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            purchase_id = table.Column<int>(type: "integer", nullable: false),
            product_id = table.Column<int>(type: "integer", nullable: false),
            product_history_id = table.Column<int>(type: "integer", nullable: false),
            amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "How many units/packages were bought")
          },
          constraints: table => {
            table.PrimaryKey("pk_purchase_item", x => x.id);
            table.CheckConstraint("chk_purchase_item_positive_amount", "amount > 0");
            table.ForeignKey(
                      name: "FK_purchase_items_products_history_product_history_id_product_~",
                      columns: x => new { x.product_history_id, x.product_id },
                      principalSchema: "market",
                      principalTable: "products_history",
                      principalColumns: ["id", "product_id"],
                      onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                      name: "FK_purchase_items_products_product_id",
                      column: x => x.product_id,
                      principalSchema: "market",
                      principalTable: "products",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                      name: "FK_purchase_items_purchases_purchase_id",
                      column: x => x.purchase_id,
                      principalSchema: "shopping",
                      principalTable: "purchases",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Cascade);
          },
          comment: "Immutable purchase receipt lines. Each line references the exact market\r\nproduct history row used for analytics, which provides the exact price and\r\nformat paid at purchase time.");

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
          name: "idx_shopping_item_product_id",
          schema: "shopping",
          table: "shopping_items",
          column: "product_id");

      migrationBuilder.CreateIndex(
          name: "IX_shopping_items_product_history_id_product_id",
          schema: "shopping",
          table: "shopping_items",
          columns: ["product_history_id", "product_id"]);

      migrationBuilder.AddCheckConstraint(
          name: "chk_shopping_item_positive_amount",
          schema: "shopping",
          table: "shopping_items",
          sql: "amount > 0");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_item_product_history_id",
          schema: "shopping",
          table: "purchase_items",
          column: "product_history_id");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_item_product_id",
          schema: "shopping",
          table: "purchase_items",
          column: "product_id");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_item_purchase_id",
          schema: "shopping",
          table: "purchase_items",
          column: "purchase_id");

      migrationBuilder.CreateIndex(
          name: "IX_purchase_items_product_history_id_product_id",
          schema: "shopping",
          table: "purchase_items",
          columns: ["product_history_id", "product_id"]);

      migrationBuilder.AddForeignKey(
          name: "FK_purchases_shopping_lists_shopping_list_id",
          schema: "shopping",
          table: "purchases",
          column: "shopping_list_id",
          principalSchema: "shopping",
          principalTable: "shopping_lists",
          principalColumn: "id",
          onDelete: ReferentialAction.SetNull);

      migrationBuilder.AddForeignKey(
          name: "FK_shopping_items_products_history_product_history_id_product_~",
          schema: "shopping",
          table: "shopping_items",
          columns: ["product_history_id", "product_id"],
          principalSchema: "market",
          principalTable: "products_history",
          principalColumns: ["id", "product_id"],
          onDelete: ReferentialAction.Restrict);

      migrationBuilder.AddForeignKey(
          name: "FK_shopping_items_products_product_id",
          schema: "shopping",
          table: "shopping_items",
          column: "product_id",
          principalSchema: "market",
          principalTable: "products",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropForeignKey(
          name: "FK_purchases_shopping_lists_shopping_list_id",
          schema: "shopping",
          table: "purchases");

      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_products_history_product_history_id_product_~",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_products_product_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropTable(
          name: "purchase_items",
          schema: "shopping");

      migrationBuilder.DropIndex(
          name: "idx_shopping_item_list_product",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "idx_shopping_item_product_history_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "idx_shopping_item_product_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropIndex(
          name: "IX_shopping_items_product_history_id_product_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropCheckConstraint(
          name: "chk_shopping_item_positive_amount",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "amount",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "product_history_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.DropColumn(
          name: "product_id",
          schema: "shopping",
          table: "shopping_items");

      migrationBuilder.RenameColumn(
          name: "shopping_list_id",
          schema: "shopping",
          table: "purchases",
          newName: "super_market_id");

      migrationBuilder.RenameIndex(
          name: "idx_purchases_user_uid",
          schema: "shopping",
          table: "purchases",
          newName: "idx_purchase_user_uid");

      migrationBuilder.RenameIndex(
          name: "idx_purchases_shopping_list_id",
          schema: "shopping",
          table: "purchases",
          newName: "idx_purchase_super_market_id");

      migrationBuilder.RenameIndex(
          name: "idx_purchases_purchased_at",
          schema: "shopping",
          table: "purchases",
          newName: "idx_purchase_purchased_at");

      migrationBuilder.AlterTable(
          name: "shopping_items",
          schema: "shopping",
          comment: "Items that belong to a shopping list, representing planned purchases",
          oldComment: "Items that belong to a shopping list, representing planned purchases.\r\nEach line points to an existing market product and the exact product history\r\nrow used when the item was added, so price and format are explicit.");

      migrationBuilder.AlterTable(
          name: "purchases",
          schema: "shopping",
          comment: "Records the actual act of buying an item — links shopping, registered\r\nitems, and the market. Core of savings analytics.",
          oldComment: "Purchase receipt header. It records who bought, when, and optionally which\r\nshopping list was checked out. Product lines live in purchase_items.");

      migrationBuilder.AlterColumn<bool>(
          name: "is_checked",
          schema: "shopping",
          table: "shopping_items",
          type: "boolean",
          nullable: false,
          oldClrType: typeof(bool),
          oldType: "boolean",
          oldDefaultValue: false);

      migrationBuilder.AddColumn<int>(
          name: "ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items",
          type: "integer",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "name",
          schema: "shopping",
          table: "shopping_items",
          type: "text",
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<decimal>(
          name: "price",
          schema: "shopping",
          table: "shopping_items",
          type: "numeric(18,2)",
          precision: 18,
          scale: 2,
          nullable: false,
          defaultValue: 0m);

      migrationBuilder.AddColumn<string>(
          name: "quantity",
          schema: "shopping",
          table: "shopping_items",
          type: "text",
          nullable: true);

      migrationBuilder.AddColumn<decimal>(
          name: "price_paid",
          schema: "shopping",
          table: "purchases",
          type: "numeric(18,2)",
          precision: 18,
          scale: 2,
          nullable: false,
          defaultValue: 0m);

      migrationBuilder.AddColumn<int>(
          name: "product_id",
          schema: "shopping",
          table: "purchases",
          type: "integer",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "quantity",
          schema: "shopping",
          table: "purchases",
          type: "text",
          nullable: true);

      migrationBuilder.AddColumn<int>(
          name: "registered_item_id",
          schema: "shopping",
          table: "purchases",
          type: "integer",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.CreateTable(
          name: "registered_items",
          schema: "shopping",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            user_uid = table.Column<Guid>(type: "uuid", nullable: false),
            last_known_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
            name = table.Column<string>(type: "text", nullable: false),
            quantity = table.Column<string>(type: "text", nullable: true)
          },
          constraints: table => {
            table.PrimaryKey("pk_registered_item", x => x.id);
            table.CheckConstraint("chk_positive_last_known_price", "last_known_price >= 0.00");
            table.ForeignKey(
                      name: "FK_registered_items_users_user_uid",
                      column: x => x.user_uid,
                      principalSchema: "identity",
                      principalTable: "users",
                      principalColumn: "uid",
                      onDelete: ReferentialAction.Cascade);
          },
          comment: "Items that users have registered as purchased or planned to purchase");

      migrationBuilder.CreateIndex(
          name: "IX_shopping_items_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items",
          column: "ProductsHistoryDbEntityId");

      migrationBuilder.AddCheckConstraint(
          name: "chk_shopping_item_positive_price",
          schema: "shopping",
          table: "shopping_items",
          sql: "price >= 0.00");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_product_id",
          schema: "shopping",
          table: "purchases",
          column: "product_id");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_registered_item_id",
          schema: "shopping",
          table: "purchases",
          column: "registered_item_id");

      migrationBuilder.AddCheckConstraint(
          name: "chk_positive_price_paid",
          schema: "shopping",
          table: "purchases",
          sql: "price_paid >= 0.00");

      migrationBuilder.CreateIndex(
          name: "idx_registered_item_user_uid",
          schema: "shopping",
          table: "registered_items",
          column: "user_uid");

      migrationBuilder.AddForeignKey(
          name: "FK_purchases_products_product_id",
          schema: "shopping",
          table: "purchases",
          column: "product_id",
          principalSchema: "market",
          principalTable: "products",
          principalColumn: "id",
          onDelete: ReferentialAction.SetNull);

      migrationBuilder.AddForeignKey(
          name: "FK_purchases_registered_items_registered_item_id",
          schema: "shopping",
          table: "purchases",
          column: "registered_item_id",
          principalSchema: "shopping",
          principalTable: "registered_items",
          principalColumn: "id",
          onDelete: ReferentialAction.Cascade);

      migrationBuilder.AddForeignKey(
          name: "FK_purchases_super_markets_super_market_id",
          schema: "shopping",
          table: "purchases",
          column: "super_market_id",
          principalSchema: "market",
          principalTable: "super_markets",
          principalColumn: "id",
          onDelete: ReferentialAction.SetNull);

      migrationBuilder.AddForeignKey(
          name: "FK_shopping_items_products_history_ProductsHistoryDbEntityId",
          schema: "shopping",
          table: "shopping_items",
          column: "ProductsHistoryDbEntityId",
          principalSchema: "market",
          principalTable: "products_history",
          principalColumn: "id");
    }
  }
}