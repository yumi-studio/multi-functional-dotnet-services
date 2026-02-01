using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace UserApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, comment: "Primary key for the User table"),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "Username of the user"),
                    email = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Email address of the user"),
                    first_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "First name of the user"),
                    last_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "Last name of the user"),
                    gender = table.Column<int>(type: "int", nullable: false, comment: "Gender of the user"),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Birth date of the user"),
                    bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Short biography of the user"),
                    avatar = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "URL or path to the user's avatar image"),
                    password_hash = table.Column<string>(type: "longtext", nullable: false, comment: "Hashed password of the user"),
                    is_system_admin = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "Indicates if the user is a system administrator"),
                    joined_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false, comment: "Date and time when the user joined"),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                },
                comment: "Table containing user information")
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    expired_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_externals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    provider = table.Column<int>(type: "int", nullable: false),
                    provider_user_id = table.Column<string>(type: "longtext", nullable: false),
                    linked_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_externals", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_externals_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_user_id",
                table: "tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_externals_user_id",
                table: "user_externals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "user_externals");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
