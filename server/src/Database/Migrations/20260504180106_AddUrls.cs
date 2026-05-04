using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class AddUrls : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.AddColumn<string>(
          name: "logo_url",
          schema: "market",
          table: "super_markets",
          type: "text",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "image_url",
          schema: "market",
          table: "products_history",
          type: "text",
          nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropColumn(
          name: "logo_url",
          schema: "market",
          table: "super_markets");

      migrationBuilder.DropColumn(
          name: "image_url",
          schema: "market",
          table: "products_history");
    }
  }
}