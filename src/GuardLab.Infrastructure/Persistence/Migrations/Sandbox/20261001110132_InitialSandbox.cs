using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuardLab.Infrastructure.Persistence.Migrations.Sandbox
{
    /// <inheritdoc />
    public partial class InitialSandbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sandbox");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "sandbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                schema: "sandbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    masked_value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.id);
                    table.ForeignKey(
                        name: "FK_api_keys_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "sandbox",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_alias",
                schema: "sandbox",
                table: "accounts",
                column: "alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_account_id",
                schema: "sandbox",
                table: "api_keys",
                column: "account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_keys",
                schema: "sandbox");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "sandbox");
        }
    }
}
