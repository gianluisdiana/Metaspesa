using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer static readonly fields over constant array arguments

namespace Metaspesa.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "role_id",
                schema: "shopping",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "shopping",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                },
                comment: "User roles for access control");

            migrationBuilder.InsertData(
                schema: "shopping",
                table: "roles",
                columns: new[] { "id", "description", "name" },
                values: new object[,]
                {
                    { 1, "Regular user who manages shopping lists", "Shopper" },
                    { 2, "User who manages market products", "ProductManager" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                schema: "shopping",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_name",
                schema: "shopping",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_users_roles_role_id",
                schema: "shopping",
                table: "users",
                column: "role_id",
                principalSchema: "shopping",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_roles_role_id",
                schema: "shopping",
                table: "users");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "shopping");

            migrationBuilder.DropIndex(
                name: "IX_users_role_id",
                schema: "shopping",
                table: "users");

            migrationBuilder.DropColumn(
                name: "role_id",
                schema: "shopping",
                table: "users");
        }
    }
}
