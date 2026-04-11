using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Metaspesa.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketSchemaAndChangePurchaseHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_registered_items_users_user_id",
                table: "registered_items");

            migrationBuilder.DropTable(
                name: "registered_items_history");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shopping_lists",
                table: "shopping_lists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shopping_list_ownerships",
                table: "shopping_list_ownerships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shopping_items",
                table: "shopping_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_registered_items",
                table: "registered_items");

            migrationBuilder.DropIndex(
                name: "IX_registered_items_name",
                table: "registered_items");

            migrationBuilder.DropColumn(
                name: "id",
                table: "shopping_list_ownerships");

            migrationBuilder.DropColumn(
                name: "last_time_used",
                table: "shopping_list_ownerships");

            migrationBuilder.EnsureSchema(
                name: "market");

            migrationBuilder.EnsureSchema(
                name: "shopping");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "shopping");

            migrationBuilder.RenameTable(
                name: "shopping_lists",
                newName: "shopping_lists",
                newSchema: "shopping");

            migrationBuilder.RenameTable(
                name: "shopping_list_ownerships",
                newName: "shopping_list_ownerships",
                newSchema: "shopping");

            migrationBuilder.RenameTable(
                name: "shopping_items",
                newName: "shopping_items",
                newSchema: "shopping");

            migrationBuilder.RenameTable(
                name: "registered_items",
                newName: "registered_items",
                newSchema: "shopping");

            migrationBuilder.RenameColumn(
                name: "uid",
                schema: "shopping",
                table: "users",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_users_username",
                schema: "shopping",
                table: "users",
                newName: "idx_user_username");

            migrationBuilder.RenameIndex(
                name: "IX_shopping_list_ownerships_user_uid",
                schema: "shopping",
                table: "shopping_list_ownerships",
                newName: "idx_shopping_list_ownership_user_uid");

            migrationBuilder.RenameIndex(
                name: "IX_shopping_list_ownerships_shopping_list_id",
                schema: "shopping",
                table: "shopping_list_ownerships",
                newName: "idx_shopping_list_ownership_shopping_list_id");

            migrationBuilder.RenameIndex(
                name: "IX_shopping_items_shopping_list_id",
                schema: "shopping",
                table: "shopping_items",
                newName: "idx_shopping_item_shopping_list_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "shopping",
                table: "registered_items",
                newName: "user_uid");

            migrationBuilder.RenameIndex(
                name: "IX_registered_items_user_id",
                schema: "shopping",
                table: "registered_items",
                newName: "idx_registered_item_user_uid");

            migrationBuilder.AlterTable(
                name: "users",
                schema: "shopping",
                comment: "Registered users of the shopping application");

            migrationBuilder.AlterTable(
                name: "shopping_lists",
                schema: "shopping",
                comment: "Shopping lists created by users, can be shared among multiple users");

            migrationBuilder.AlterTable(
                name: "shopping_list_ownerships",
                schema: "shopping",
                comment: "Associates users with shopping lists, allowing for shared lists");

            migrationBuilder.AlterTable(
                name: "shopping_items",
                schema: "shopping",
                comment: "Items that belong to a shopping list, representing planned purchases");

            migrationBuilder.AlterTable(
                name: "registered_items",
                schema: "shopping",
                comment: "Items that users have registered as purchased or planned to purchase");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                schema: "shopping",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "encrypted_password",
                schema: "shopping",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "shopping",
                table: "shopping_lists",
                type: "text",
                nullable: true,
                comment: "Missing name indicates a temporary shopping list, not saved by the user",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "shopping",
                table: "shopping_lists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "price",
                schema: "shopping",
                table: "shopping_items",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "shopping",
                table: "shopping_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "last_known_price",
                schema: "shopping",
                table: "registered_items",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddPrimaryKey(
                name: "pk_user",
                schema: "shopping",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shopping_list",
                schema: "shopping",
                table: "shopping_lists",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shopping_list_ownership",
                schema: "shopping",
                table: "shopping_list_ownerships",
                columns: ["user_uid", "shopping_list_id"]);

            migrationBuilder.AddPrimaryKey(
                name: "pk_shopping_item",
                schema: "shopping",
                table: "shopping_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_registered_item",
                schema: "shopping",
                table: "registered_items",
                column: "id");

            migrationBuilder.CreateTable(
                name: "product_brands",
                schema: "market",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_brand", x => x.id);
                },
                comment: "Brands of products available in the market");

            migrationBuilder.CreateTable(
                name: "super_markets",
                schema: "market",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_super_market", x => x.id);
                },
                comment: "Supermarkets where products are available");

            migrationBuilder.CreateTable(
                name: "products",
                schema: "market",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    super_market_id = table.Column<int>(type: "integer", nullable: false),
                    brand_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_product_brands_brand_id",
                        column: x => x.brand_id,
                        principalSchema: "market",
                        principalTable: "product_brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_super_markets_super_market_id",
                        column: x => x.super_market_id,
                        principalSchema: "market",
                        principalTable: "super_markets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Products available in the market");

            migrationBuilder.CreateTable(
                name: "products_history",
                schema: "market",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    price = table.Column<float>(type: "real", nullable: false),
                    quantity = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    product_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_history", x => x.id);
                    table.CheckConstraint("chk_positive_price", "price >= 0.00");
                    table.ForeignKey(
                        name: "FK_products_history_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "market",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Historical price and quantity data for products, tracking changes over time");

            migrationBuilder.CreateTable(
                name: "purchases",
                schema: "shopping",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    registered_item_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    super_market_id = table.Column<int>(type: "integer", nullable: true),
                    price_paid = table.Column<float>(type: "real", nullable: false),
                    quantity = table.Column<string>(type: "text", nullable: true),
                    purchased_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase", x => x.id);
                    table.CheckConstraint("chk_positive_price_paid", "price_paid >= 0.00");
                    table.ForeignKey(
                        name: "FK_purchases_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "market",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_purchases_registered_items_registered_item_id",
                        column: x => x.registered_item_id,
                        principalSchema: "shopping",
                        principalTable: "registered_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchases_super_markets_super_market_id",
                        column: x => x.super_market_id,
                        principalSchema: "market",
                        principalTable: "super_markets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_purchases_users_user_uid",
                        column: x => x.user_uid,
                        principalSchema: "shopping",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Records the actual act of buying an item — links shopping, registered\r\nitems, and the market. Core of savings analytics.");

            migrationBuilder.AddCheckConstraint(
                name: "chk_shopping_list_temporary_cannot_be_soft_deleted",
                schema: "shopping",
                table: "shopping_lists",
                sql: "name IS NOT NULL OR deleted_at IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "chk_shopping_item_positive_price",
                schema: "shopping",
                table: "shopping_items",
                sql: "price >= 0.00");

            migrationBuilder.AddCheckConstraint(
                name: "chk_positive_last_known_price",
                schema: "shopping",
                table: "registered_items",
                sql: "last_known_price >= 0.00");

            migrationBuilder.CreateIndex(
                name: "idx_product_brand_name",
                schema: "market",
                table: "product_brands",
                column: "name",
                unique: true);

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
                name: "idx_product_history_created_at",
                schema: "market",
                table: "products_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_product_history_product_id",
                schema: "market",
                table: "products_history",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_product_id",
                schema: "shopping",
                table: "purchases",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_purchased_at",
                schema: "shopping",
                table: "purchases",
                column: "purchased_at");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_registered_item_id",
                schema: "shopping",
                table: "purchases",
                column: "registered_item_id");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_super_market_id",
                schema: "shopping",
                table: "purchases",
                column: "super_market_id");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_user_uid",
                schema: "shopping",
                table: "purchases",
                column: "user_uid");

            migrationBuilder.CreateIndex(
                name: "idx_super_market_name",
                schema: "market",
                table: "super_markets",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_registered_items_users_user_uid",
                schema: "shopping",
                table: "registered_items",
                column: "user_uid",
                principalSchema: "shopping",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_registered_items_users_user_uid",
                schema: "shopping",
                table: "registered_items");

            migrationBuilder.DropTable(
                name: "products_history",
                schema: "market");

            migrationBuilder.DropTable(
                name: "purchases",
                schema: "shopping");

            migrationBuilder.DropTable(
                name: "products",
                schema: "market");

            migrationBuilder.DropTable(
                name: "product_brands",
                schema: "market");

            migrationBuilder.DropTable(
                name: "super_markets",
                schema: "market");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user",
                schema: "shopping",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shopping_list",
                schema: "shopping",
                table: "shopping_lists");

            migrationBuilder.DropCheckConstraint(
                name: "chk_shopping_list_temporary_cannot_be_soft_deleted",
                schema: "shopping",
                table: "shopping_lists");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shopping_list_ownership",
                schema: "shopping",
                table: "shopping_list_ownerships");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shopping_item",
                schema: "shopping",
                table: "shopping_items");

            migrationBuilder.DropCheckConstraint(
                name: "chk_shopping_item_positive_price",
                schema: "shopping",
                table: "shopping_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_registered_item",
                schema: "shopping",
                table: "registered_items");

            migrationBuilder.DropCheckConstraint(
                name: "chk_positive_last_known_price",
                schema: "shopping",
                table: "registered_items");

            migrationBuilder.DropColumn(
                name: "encrypted_password",
                schema: "shopping",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "shopping",
                table: "shopping_lists");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "shopping",
                table: "shopping_items");

            migrationBuilder.DropColumn(
                name: "last_known_price",
                schema: "shopping",
                table: "registered_items");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "shopping",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "shopping_lists",
                schema: "shopping",
                newName: "shopping_lists");

            migrationBuilder.RenameTable(
                name: "shopping_list_ownerships",
                schema: "shopping",
                newName: "shopping_list_ownerships");

            migrationBuilder.RenameTable(
                name: "shopping_items",
                schema: "shopping",
                newName: "shopping_items");

            migrationBuilder.RenameTable(
                name: "registered_items",
                schema: "shopping",
                newName: "registered_items");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "uid");

            migrationBuilder.RenameIndex(
                name: "idx_user_username",
                table: "users",
                newName: "IX_users_username");

            migrationBuilder.RenameIndex(
                name: "idx_shopping_list_ownership_user_uid",
                table: "shopping_list_ownerships",
                newName: "IX_shopping_list_ownerships_user_uid");

            migrationBuilder.RenameIndex(
                name: "idx_shopping_list_ownership_shopping_list_id",
                table: "shopping_list_ownerships",
                newName: "IX_shopping_list_ownerships_shopping_list_id");

            migrationBuilder.RenameIndex(
                name: "idx_shopping_item_shopping_list_id",
                table: "shopping_items",
                newName: "IX_shopping_items_shopping_list_id");

            migrationBuilder.RenameColumn(
                name: "user_uid",
                table: "registered_items",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "idx_registered_item_user_uid",
                table: "registered_items",
                newName: "IX_registered_items_user_id");

            migrationBuilder.AlterTable(
                name: "users",
                oldComment: "Registered users of the shopping application");

            migrationBuilder.AlterTable(
                name: "shopping_lists",
                oldComment: "Shopping lists created by users, can be shared among multiple users");

            migrationBuilder.AlterTable(
                name: "shopping_list_ownerships",
                oldComment: "Associates users with shopping lists, allowing for shared lists");

            migrationBuilder.AlterTable(
                name: "shopping_items",
                oldComment: "Items that belong to a shopping list, representing planned purchases");

            migrationBuilder.AlterTable(
                name: "registered_items",
                oldComment: "Items that users have registered as purchased or planned to purchase");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "shopping_lists",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Missing name indicates a temporary shopping list, not saved by the user");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "shopping_list_ownerships",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_time_used",
                table: "shopping_list_ownerships",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<float>(
                name: "price",
                table: "shopping_items",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "uid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shopping_lists",
                table: "shopping_lists",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shopping_list_ownerships",
                table: "shopping_list_ownerships",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shopping_items",
                table: "shopping_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_registered_items",
                table: "registered_items",
                column: "id");

            migrationBuilder.CreateTable(
                name: "registered_items_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    registered_item_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    price = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registered_items_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_registered_items_history_registered_items_registered_item_id",
                        column: x => x.registered_item_id,
                        principalTable: "registered_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registered_items_name",
                table: "registered_items",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registered_items_history_registered_item_id",
                table: "registered_items_history",
                column: "registered_item_id");

            migrationBuilder.AddForeignKey(
                name: "FK_registered_items_users_user_id",
                table: "registered_items",
                column: "user_id",
                principalTable: "users",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
