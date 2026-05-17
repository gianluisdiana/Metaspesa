using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class UrlPerProductFormat : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropColumn(
          name: "image",
          schema: "market",
          table: "products");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.AddColumn<string>(
          name: "image",
          schema: "market",
          table: "products",
          type: "text",
          nullable: true);
    }
  }
}