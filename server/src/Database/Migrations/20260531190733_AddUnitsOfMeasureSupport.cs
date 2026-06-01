using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class AddUnitsOfMeasureSupport : Migration {
    private static readonly string[] ProductFormatIdentityColumns = [
      "product_id",
      "quantity",
      "unit_of_measure_id"
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.EnsureSchema(
          name: "identity");

      migrationBuilder.RenameTable(
          name: "users",
          schema: "shopping",
          newName: "users",
          newSchema: "identity");

      migrationBuilder.RenameTable(
          name: "roles",
          schema: "shopping",
          newName: "roles",
          newSchema: "identity");

      migrationBuilder.RenameColumn(
          name: "id",
          schema: "identity",
          table: "users",
          newName: "uid");

      migrationBuilder.RenameIndex(
          name: "IX_users_role_id",
          schema: "identity",
          table: "users",
          newName: "idx_user_role_id");

      migrationBuilder.AddColumn<int>(
          name: "ProductFormatDbEntityId",
          schema: "market",
          table: "products_history",
          type: "integer",
          nullable: true);

      migrationBuilder.AlterColumn<string>(
          name: "description",
          schema: "identity",
          table: "roles",
          type: "character varying(255)",
          maxLength: 255,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "character varying(500)",
          oldMaxLength: 500);

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
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_product_formats_units_of_measure_unit_of_measure_id",
                      column: x => x.unit_of_measure_id,
                      principalSchema: "market",
                      principalTable: "units_of_measure",
                      principalColumn: "id",
                      onDelete: ReferentialAction.Cascade);
          },
          comment: "Different package formats for the same product. For example, a soda can be sold\r\nin 330 ml cans, 500 ml bottles, or 1 l bottles.");

      migrationBuilder.CreateIndex(
          name: "IX_products_history_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history",
          column: "ProductFormatDbEntityId");

      migrationBuilder.CreateIndex(
          name: "idx_product_format_identity",
          schema: "market",
          table: "product_formats",
          columns: ProductFormatIdentityColumns,
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
          name: "idx_unit_of_measure_code",
          schema: "market",
          table: "units_of_measure",
          column: "code",
          unique: true);

      migrationBuilder.AddForeignKey(
          name: "FK_products_history_product_formats_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history",
          column: "ProductFormatDbEntityId",
          principalSchema: "market",
          principalTable: "product_formats",
          principalColumn: "id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropForeignKey(
          name: "FK_products_history_product_formats_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropTable(
          name: "product_formats",
          schema: "market");

      migrationBuilder.DropTable(
          name: "units_of_measure",
          schema: "market");

      migrationBuilder.DropIndex(
          name: "IX_products_history_ProductFormatDbEntityId",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropColumn(
          name: "ProductFormatDbEntityId",
          schema: "market",
          table: "products_history");

      migrationBuilder.RenameTable(
          name: "users",
          schema: "identity",
          newName: "users",
          newSchema: "shopping");

      migrationBuilder.RenameTable(
          name: "roles",
          schema: "identity",
          newName: "roles",
          newSchema: "shopping");

      migrationBuilder.RenameColumn(
          name: "uid",
          schema: "shopping",
          table: "users",
          newName: "id");

      migrationBuilder.RenameIndex(
          name: "idx_user_role_id",
          schema: "shopping",
          table: "users",
          newName: "IX_users_role_id");

      migrationBuilder.AlterColumn<string>(
          name: "description",
          schema: "shopping",
          table: "roles",
          type: "character varying(500)",
          maxLength: 500,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "character varying(255)",
          oldMaxLength: 255);
    }
  }
}