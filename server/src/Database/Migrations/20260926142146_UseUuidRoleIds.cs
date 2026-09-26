using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations;

/// <inheritdoc />
public partial class UseUuidRoleIds : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.Sql("""
      ALTER TABLE identity.users
        DROP CONSTRAINT "FK_users_roles_role_id";

      ALTER TABLE identity.roles
        ALTER COLUMN id TYPE uuid USING
          ('00000000-0000-7000-8000-' || lpad(to_hex(id), 12, '0'))::uuid;
      ALTER TABLE identity.users
        ALTER COLUMN role_id TYPE uuid USING
          ('00000000-0000-7000-8000-' || lpad(to_hex(role_id), 12, '0'))::uuid;

      ALTER TABLE identity.users
        ADD CONSTRAINT "FK_users_roles_role_id"
        FOREIGN KEY (role_id) REFERENCES identity.roles (id) ON DELETE RESTRICT;
      """);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) =>
    throw new NotSupportedException(
      "UUID role identifiers cannot be safely converted back to integers.");
}