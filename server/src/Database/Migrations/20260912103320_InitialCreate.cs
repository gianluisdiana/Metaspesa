using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class InitialCreate : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.EnsureSchema(
          name: "market");

      migrationBuilder.EnsureSchema(
          name: "purchasing");

      migrationBuilder.EnsureSchema(
          name: "identity");

      migrationBuilder.EnsureSchema(
          name: "shopping");

      migrationBuilder.CreateTable(
          name: "product_brands",
          schema: "market",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("pk_product_brand", x => x.id);
          },
          comment: "Brands of products available in the market");

      migrationBuilder.CreateTable(
          name: "roles",
          schema: "identity",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("pk_role", x => x.id);
          },
          comment: "User roles for access control");

      migrationBuilder.CreateTable(
          name: "shopping_lists",
          schema: "shopping",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(type: "text", nullable: true),
            is_temporary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
            deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
          },
          constraints: table => {
            table.PrimaryKey("pk_shopping_list", x => x.id);
            table.CheckConstraint("chk_saved_shopping_list_requires_name", "is_temporary = true OR name IS NOT NULL");
            table.CheckConstraint("chk_shopping_list_temporary_cannot_be_soft_deleted", "is_temporary = false OR deleted_at IS NULL");
          },
          comment: "Shopping lists created by users, can be shared among multiple users");

      migrationBuilder.CreateTable(
          name: "super_markets",
          schema: "market",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(type: "text", nullable: false),
            logo_url = table.Column<string>(type: "text", nullable: true)
          },
          constraints: table => {
            table.PrimaryKey("pk_super_market", x => x.id);
          },
          comment: "Supermarkets where products are available");

      migrationBuilder.CreateTable(
          name: "units_of_measure",
          schema: "market",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, comment: "Stable unit code, e.g. ml, kg, piece"),
            name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("pk_unit_of_measure", x => x.id);
          },
          comment: "Units used to describe product package format, not shopping count.\r\nExamples: ml, l, g, kg, piece.");

      migrationBuilder.CreateTable(
          name: "users",
          schema: "identity",
          columns: table => new {
            uid = table.Column<Guid>(type: "uuid", nullable: false),
            username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
            encrypted_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
            role_id = table.Column<int>(type: "integer", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("pk_user", x => x.uid);
            table.ForeignKey(
                      name: "FK_users_roles_role_id",
                      column: x => x.role_id,
                      principalSchema: "identity",
                      principalTable: "roles",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
          },
          comment: "Registered users of the shopping application");

      migrationBuilder.CreateTable(
          name: "products",
          schema: "market",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            name = table.Column<string>(type: "text", nullable: false),
            super_market_id = table.Column<int>(type: "integer", nullable: false),
            brand_id = table.Column<int>(type: "integer", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("pk_product", x => x.id);
            table.ForeignKey(
                      name: "FK_products_product_brands_brand_id",
                      column: x => x.brand_id,
                      principalSchema: "market",
                      principalTable: "product_brands",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                      name: "FK_products_super_markets_super_market_id",
                      column: x => x.super_market_id,
                      principalSchema: "market",
                      principalTable: "super_markets",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
          },
          comment: "Products available in the market");

      migrationBuilder.CreateTable(
          name: "purchases",
          schema: "purchasing",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            user_uid = table.Column<Guid>(type: "uuid", nullable: true),
            shopping_list_id = table.Column<int>(type: "integer", nullable: true),
            purchased_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
          },
          constraints: table => {
            table.PrimaryKey("pk_purchase", x => x.id);
            table.ForeignKey(
                      name: "FK_purchases_shopping_lists_shopping_list_id",
                      column: x => x.shopping_list_id,
                      principalSchema: "shopping",
                      principalTable: "shopping_lists",
                      principalColumn: "id",
                      onDelete: ReferentialAction.SetNull);
            table.ForeignKey(
                      name: "FK_purchases_users_user_uid",
                      column: x => x.user_uid,
                      principalSchema: "identity",
                      principalTable: "users",
                      principalColumn: "uid",
                      onDelete: ReferentialAction.SetNull);
          },
          comment: "Purchase receipt header. It records who bought, when, and optionally which\r\nshopping list was checked out. Product lines live in purchase_items.");

      migrationBuilder.CreateTable(
          name: "shopping_list_ownerships",
          schema: "shopping",
          columns: table => new {
            user_uid = table.Column<Guid>(type: "uuid", nullable: false),
            shopping_list_id = table.Column<int>(type: "integer", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("pk_shopping_list_ownership", x => new { x.user_uid, x.shopping_list_id });
            table.ForeignKey(
                      name: "FK_shopping_list_ownerships_shopping_lists_shopping_list_id",
                      column: x => x.shopping_list_id,
                      principalSchema: "shopping",
                      principalTable: "shopping_lists",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_shopping_list_ownerships_users_user_uid",
                      column: x => x.user_uid,
                      principalSchema: "identity",
                      principalTable: "users",
                      principalColumn: "uid",
                      onDelete: ReferentialAction.Cascade);
          },
          comment: "Associates users with shopping lists, allowing for shared lists");

      migrationBuilder.CreateTable(
          name: "product_formats",
          schema: "market",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            product_id = table.Column<int>(type: "integer", nullable: false),
            quantity = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
            unit_of_measure_id = table.Column<int>(type: "integer", nullable: false),
            image_url = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("pk_product_format", x => x.id);
            table.UniqueConstraint("ak_product_format_id_product_id", x => new { x.id, x.product_id });
            table.CheckConstraint("chk_product_format_positive_quantity", "quantity > 0.000");
            table.ForeignKey(
                      name: "FK_product_formats_products_product_id",
                      column: x => x.product_id,
                      principalSchema: "market",
                      principalTable: "products",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                      name: "FK_product_formats_units_of_measure_unit_of_measure_id",
                      column: x => x.unit_of_measure_id,
                      principalSchema: "market",
                      principalTable: "units_of_measure",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
          },
          comment: "Different package formats for the same product. For example, a soda can be sold\r\nin 330 ml cans, 500 ml bottles, or 1 l bottles.");

      migrationBuilder.CreateTable(
          name: "price_snapshots",
          schema: "market",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            product_format_id = table.Column<int>(type: "integer", nullable: false),
            price_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
            currency_code = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "EUR"),
            observed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
          },
          constraints: table => {
            table.PrimaryKey("pk_price_snapshot", x => x.id);
            table.CheckConstraint("chk_price_snapshot_currency_code", "currency_code ~ '^[A-Z]{3}$'");
            table.CheckConstraint("chk_price_snapshot_positive_price", "price_amount > 0.00");
            table.ForeignKey(
                      name: "FK_price_snapshots_product_formats_product_format_id",
                      column: x => x.product_format_id,
                      principalSchema: "market",
                      principalTable: "product_formats",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
          },
          comment: "Historical price observations for product formats.");

      migrationBuilder.CreateTable(
          name: "shopping_items",
          schema: "shopping",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            shopping_list_id = table.Column<int>(type: "integer", nullable: false),
            product_format_id = table.Column<int>(type: "integer", nullable: false),
            amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "How many units/packages of the product are planned"),
            is_checked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
            deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
          },
          constraints: table => {
            table.PrimaryKey("pk_shopping_item", x => x.id);
            table.CheckConstraint("chk_shopping_item_positive_amount", "amount > 0");
            table.ForeignKey(
                      name: "FK_shopping_items_product_formats_product_format_id",
                      column: x => x.product_format_id,
                      principalSchema: "market",
                      principalTable: "product_formats",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                      name: "FK_shopping_items_shopping_lists_shopping_list_id",
                      column: x => x.shopping_list_id,
                      principalSchema: "shopping",
                      principalTable: "shopping_lists",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Cascade);
          },
          comment: "Items that belong to a shopping list, representing planned purchases.\r\nEach line points to an existing product format.");

      migrationBuilder.CreateTable(
          name: "purchase_items",
          schema: "purchasing",
          columns: table => new {
            id = table.Column<int>(type: "integer", nullable: false)
                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            purchase_id = table.Column<int>(type: "integer", nullable: false),
            price_snapshot_id = table.Column<int>(type: "integer", nullable: false),
            amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "How many units/packages were bought")
          },
          constraints: table => {
            table.PrimaryKey("pk_purchase_item", x => x.id);
            table.CheckConstraint("chk_purchase_item_positive_amount", "amount > 0");
            table.ForeignKey(
                      name: "FK_purchase_items_price_snapshots_price_snapshot_id",
                      column: x => x.price_snapshot_id,
                      principalSchema: "market",
                      principalTable: "price_snapshots",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                      name: "FK_purchase_items_purchases_purchase_id",
                      column: x => x.purchase_id,
                      principalSchema: "purchasing",
                      principalTable: "purchases",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Cascade);
          },
          comment: "Immutable purchase receipt lines. Each line references the exact market\r\nprice snapshot used at purchase time.");

      migrationBuilder.InsertData(
          schema: "identity",
          table: "roles",
          columns: ["id", "description", "name"],
          values: new object[,]
          {
                    { 1, "Regular user who manages shopping lists", "Shopper" },
                    { 2, "User who manages market products", "ProductManager" }
          });

      migrationBuilder.CreateIndex(
          name: "idx_price_snapshot_format_observed_at",
          schema: "market",
          table: "price_snapshots",
          columns: ["product_format_id", "observed_at"]);

      migrationBuilder.CreateIndex(
          name: "idx_price_snapshot_observed_at",
          schema: "market",
          table: "price_snapshots",
          column: "observed_at");

      migrationBuilder.CreateIndex(
          name: "idx_price_snapshot_product_format_id",
          schema: "market",
          table: "price_snapshots",
          column: "product_format_id");

      migrationBuilder.CreateIndex(
          name: "idx_product_brand_name",
          schema: "market",
          table: "product_brands",
          column: "name",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_product_format_identity",
          schema: "market",
          table: "product_formats",
          columns: ["product_id", "quantity", "unit_of_measure_id"],
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_product_format_product_id",
          schema: "market",
          table: "product_formats",
          column: "product_id");

      migrationBuilder.CreateIndex(
          name: "idx_product_format_unit_of_measure_id",
          schema: "market",
          table: "product_formats",
          column: "unit_of_measure_id");

      migrationBuilder.CreateIndex(
          name: "idx_product_brand_id",
          schema: "market",
          table: "products",
          column: "brand_id");

      migrationBuilder.CreateIndex(
          name: "idx_product_name_super_market_brand",
          schema: "market",
          table: "products",
          columns: ["name", "super_market_id", "brand_id"],
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_product_super_market_id",
          schema: "market",
          table: "products",
          column: "super_market_id");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_item_price_snapshot_id",
          schema: "purchasing",
          table: "purchase_items",
          column: "price_snapshot_id");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_item_purchase_id",
          schema: "purchasing",
          table: "purchase_items",
          column: "purchase_id");

      migrationBuilder.CreateIndex(
          name: "idx_purchases_purchased_at",
          schema: "purchasing",
          table: "purchases",
          column: "purchased_at");

      migrationBuilder.CreateIndex(
          name: "idx_purchases_shopping_list_id",
          schema: "purchasing",
          table: "purchases",
          column: "shopping_list_id");

      migrationBuilder.CreateIndex(
          name: "idx_purchases_user_uid",
          schema: "purchasing",
          table: "purchases",
          column: "user_uid");

      migrationBuilder.CreateIndex(
          name: "idx_role_name",
          schema: "identity",
          table: "roles",
          column: "name",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_list_product_format",
          schema: "shopping",
          table: "shopping_items",
          columns: ["shopping_list_id", "product_format_id"]);

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_list_product_format_active",
          schema: "shopping",
          table: "shopping_items",
          columns: ["shopping_list_id", "product_format_id"],
          unique: true,
          filter: "deleted_at IS NULL");

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_product_format_id",
          schema: "shopping",
          table: "shopping_items",
          column: "product_format_id");

      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_shopping_list_id",
          schema: "shopping",
          table: "shopping_items",
          column: "shopping_list_id");

      migrationBuilder.CreateIndex(
          name: "idx_shopping_list_ownership_shopping_list_id",
          schema: "shopping",
          table: "shopping_list_ownerships",
          column: "shopping_list_id");

      migrationBuilder.CreateIndex(
          name: "idx_shopping_list_ownership_user_uid",
          schema: "shopping",
          table: "shopping_list_ownerships",
          column: "user_uid");

      migrationBuilder.CreateIndex(
          name: "idx_super_market_name",
          schema: "market",
          table: "super_markets",
          column: "name",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_unit_of_measure_code",
          schema: "market",
          table: "units_of_measure",
          column: "code",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "idx_user_role_id",
          schema: "identity",
          table: "users",
          column: "role_id");

      migrationBuilder.CreateIndex(
          name: "idx_user_username",
          schema: "identity",
          table: "users",
          column: "username",
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(
          name: "purchase_items",
          schema: "purchasing");

      migrationBuilder.DropTable(
          name: "shopping_items",
          schema: "shopping");

      migrationBuilder.DropTable(
          name: "shopping_list_ownerships",
          schema: "shopping");

      migrationBuilder.DropTable(
          name: "price_snapshots",
          schema: "market");

      migrationBuilder.DropTable(
          name: "purchases",
          schema: "purchasing");

      migrationBuilder.DropTable(
          name: "product_formats",
          schema: "market");

      migrationBuilder.DropTable(
          name: "shopping_lists",
          schema: "shopping");

      migrationBuilder.DropTable(
          name: "users",
          schema: "identity");

      migrationBuilder.DropTable(
          name: "products",
          schema: "market");

      migrationBuilder.DropTable(
          name: "units_of_measure",
          schema: "market");

      migrationBuilder.DropTable(
          name: "roles",
          schema: "identity");

      migrationBuilder.DropTable(
          name: "product_brands",
          schema: "market");

      migrationBuilder.DropTable(
          name: "super_markets",
          schema: "market");
    }
  }
}