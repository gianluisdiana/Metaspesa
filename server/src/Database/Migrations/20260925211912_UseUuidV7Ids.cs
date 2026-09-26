using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations;

/// <inheritdoc />
public partial class UseUuidV7Ids : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.Sql("""
      CREATE FUNCTION pg_temp.legacy_uuid(value integer) RETURNS uuid
      LANGUAGE sql IMMUTABLE STRICT AS $$
        SELECT ('00000000-0000-7000-8000-' || lpad(to_hex(value), 12, '0'))::uuid
      $$;

      ALTER TABLE market.products
        DROP CONSTRAINT "FK_products_product_brands_brand_id",
        DROP CONSTRAINT "FK_products_super_markets_super_market_id";
      ALTER TABLE purchasing.purchases
        DROP CONSTRAINT "FK_purchases_shopping_lists_shopping_list_id";
      ALTER TABLE shopping.shopping_list_ownerships
        DROP CONSTRAINT "FK_shopping_list_ownerships_shopping_lists_shopping_list_id";
      ALTER TABLE market.product_formats
        DROP CONSTRAINT "FK_product_formats_products_product_id",
        DROP CONSTRAINT "FK_product_formats_units_of_measure_unit_of_measure_id";
      ALTER TABLE market.price_snapshots
        DROP CONSTRAINT "FK_price_snapshots_product_formats_product_format_id";
      ALTER TABLE shopping.shopping_items
        DROP CONSTRAINT "FK_shopping_items_product_formats_product_format_id",
        DROP CONSTRAINT "FK_shopping_items_shopping_lists_shopping_list_id";
      ALTER TABLE purchasing.purchase_items
        DROP CONSTRAINT "FK_purchase_items_price_snapshots_price_snapshot_id",
        DROP CONSTRAINT "FK_purchase_items_purchases_purchase_id";

      ALTER TABLE identity.roles ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE market.units_of_measure ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE market.units_of_measure ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE market.super_markets ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE market.super_markets ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE market.product_brands ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE market.product_brands ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE market.products ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE market.products ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE market.products ALTER COLUMN super_market_id TYPE uuid USING pg_temp.legacy_uuid(super_market_id);
      ALTER TABLE market.products ALTER COLUMN brand_id TYPE uuid USING pg_temp.legacy_uuid(brand_id);
      ALTER TABLE market.product_formats ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE market.product_formats ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE market.product_formats ALTER COLUMN product_id TYPE uuid USING pg_temp.legacy_uuid(product_id);
      ALTER TABLE market.product_formats ALTER COLUMN unit_of_measure_id TYPE uuid USING pg_temp.legacy_uuid(unit_of_measure_id);
      ALTER TABLE market.price_snapshots ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE market.price_snapshots ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE market.price_snapshots ALTER COLUMN product_format_id TYPE uuid USING pg_temp.legacy_uuid(product_format_id);
      ALTER TABLE shopping.shopping_lists ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE shopping.shopping_lists ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE shopping.shopping_list_ownerships ALTER COLUMN shopping_list_id TYPE uuid USING pg_temp.legacy_uuid(shopping_list_id);
      ALTER TABLE shopping.shopping_items ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE shopping.shopping_items ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE shopping.shopping_items ALTER COLUMN shopping_list_id TYPE uuid USING pg_temp.legacy_uuid(shopping_list_id);
      ALTER TABLE shopping.shopping_items ALTER COLUMN product_format_id TYPE uuid USING pg_temp.legacy_uuid(product_format_id);
      ALTER TABLE purchasing.purchases ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE purchasing.purchases ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE purchasing.purchases ALTER COLUMN shopping_list_id TYPE uuid USING pg_temp.legacy_uuid(shopping_list_id);
      ALTER TABLE purchasing.purchase_items ALTER COLUMN id DROP IDENTITY IF EXISTS;
      ALTER TABLE purchasing.purchase_items ALTER COLUMN id TYPE uuid USING pg_temp.legacy_uuid(id);
      ALTER TABLE purchasing.purchase_items ALTER COLUMN purchase_id TYPE uuid USING pg_temp.legacy_uuid(purchase_id);
      ALTER TABLE purchasing.purchase_items ALTER COLUMN price_snapshot_id TYPE uuid USING pg_temp.legacy_uuid(price_snapshot_id);

      ALTER TABLE market.products
        ADD CONSTRAINT "FK_products_product_brands_brand_id" FOREIGN KEY (brand_id) REFERENCES market.product_brands (id) ON DELETE RESTRICT,
        ADD CONSTRAINT "FK_products_super_markets_super_market_id" FOREIGN KEY (super_market_id) REFERENCES market.super_markets (id) ON DELETE RESTRICT;
      ALTER TABLE purchasing.purchases
        ADD CONSTRAINT "FK_purchases_shopping_lists_shopping_list_id" FOREIGN KEY (shopping_list_id) REFERENCES shopping.shopping_lists (id) ON DELETE SET NULL;
      ALTER TABLE shopping.shopping_list_ownerships
        ADD CONSTRAINT "FK_shopping_list_ownerships_shopping_lists_shopping_list_id" FOREIGN KEY (shopping_list_id) REFERENCES shopping.shopping_lists (id) ON DELETE CASCADE;
      ALTER TABLE market.product_formats
        ADD CONSTRAINT "FK_product_formats_products_product_id" FOREIGN KEY (product_id) REFERENCES market.products (id) ON DELETE RESTRICT,
        ADD CONSTRAINT "FK_product_formats_units_of_measure_unit_of_measure_id" FOREIGN KEY (unit_of_measure_id) REFERENCES market.units_of_measure (id) ON DELETE RESTRICT;
      ALTER TABLE market.price_snapshots
        ADD CONSTRAINT "FK_price_snapshots_product_formats_product_format_id" FOREIGN KEY (product_format_id) REFERENCES market.product_formats (id) ON DELETE RESTRICT;
      ALTER TABLE shopping.shopping_items
        ADD CONSTRAINT "FK_shopping_items_product_formats_product_format_id" FOREIGN KEY (product_format_id) REFERENCES market.product_formats (id) ON DELETE RESTRICT,
        ADD CONSTRAINT "FK_shopping_items_shopping_lists_shopping_list_id" FOREIGN KEY (shopping_list_id) REFERENCES shopping.shopping_lists (id) ON DELETE CASCADE;
      ALTER TABLE purchasing.purchase_items
        ADD CONSTRAINT "FK_purchase_items_price_snapshots_price_snapshot_id" FOREIGN KEY (price_snapshot_id) REFERENCES market.price_snapshots (id) ON DELETE RESTRICT,
        ADD CONSTRAINT "FK_purchase_items_purchases_purchase_id" FOREIGN KEY (purchase_id) REFERENCES purchasing.purchases (id) ON DELETE CASCADE;
      """);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) =>
    throw new NotSupportedException(
      "UUID identifiers cannot be safely converted back to integer identities.");
}