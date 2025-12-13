using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YumiStudio.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, comment: "Primary key for the User table", collation: "ascii_general_ci"),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "Username of the user")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Email address of the user")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    first_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "First name of the user")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "Last name of the user")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender = table.Column<int>(type: "int", nullable: false, comment: "Gender of the user"),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Birth date of the user"),
                    bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, comment: "Short biography of the user")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "URL or path to the user's avatar image")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "longtext", nullable: false, comment: "Hashed password of the user")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_system_admin = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "Indicates if the user is a system administrator"),
                    joined_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "Date and time when the user joined"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                },
                comment: "Table containing user information")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_profiles",
                columns: table => new
                {
                    profile_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fakebook_profiles", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_fakebook_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "file_uploads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    path = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<int>(type: "int", nullable: false),
                    mime = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    aws_s3_key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uploaded_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    is_draft = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "New upload is marked as draft and be cleared if not change to true"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_uploads", x => x.id);
                    table.ForeignKey(
                        name: "FK_file_uploads_users_uploaded_by",
                        column: x => x.uploaded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    visibility = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fakebook_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_fakebook_posts_fakebook_profiles_created_by",
                        column: x => x.created_by,
                        principalTable: "fakebook_profiles",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_reactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    target_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    reacted_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    reaction_type = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fakebook_reactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_fakebook_reactions_fakebook_profiles_reacted_by",
                        column: x => x.reacted_by,
                        principalTable: "fakebook_profiles",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_post_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    post_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fakebook_post_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_fakebook_post_comments_fakebook_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "fakebook_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fakebook_post_comments_fakebook_profiles_created_by",
                        column: x => x.created_by,
                        principalTable: "fakebook_profiles",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_post_media",
                columns: table => new
                {
                    post_media_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    post_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    file_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fakebook_post_media", x => x.post_media_id);
                    table.ForeignKey(
                        name: "FK_fakebook_post_media_fakebook_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "fakebook_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fakebook_post_media_file_uploads_file_id",
                        column: x => x.file_id,
                        principalTable: "file_uploads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_post_comments_created_by",
                table: "fakebook_post_comments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_post_comments_post_id",
                table: "fakebook_post_comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "idx_post_id_file_id",
                table: "fakebook_post_media",
                columns: new[] { "post_id", "file_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_post_media_file_id",
                table: "fakebook_post_media",
                column: "file_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_posts_created_by",
                table: "fakebook_posts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_profiles_user_id",
                table: "fakebook_profiles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_reactions_reacted_by_target_type_target_id",
                table: "fakebook_reactions",
                columns: new[] { "reacted_by", "target_type", "target_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_reactions_target_type_target_id",
                table: "fakebook_reactions",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "IX_file_uploads_uploaded_by",
                table: "file_uploads",
                column: "uploaded_by");

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
                name: "fakebook_post_comments");

            migrationBuilder.DropTable(
                name: "fakebook_post_media");

            migrationBuilder.DropTable(
                name: "fakebook_reactions");

            migrationBuilder.DropTable(
                name: "fakebook_posts");

            migrationBuilder.DropTable(
                name: "file_uploads");

            migrationBuilder.DropTable(
                name: "fakebook_profiles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
