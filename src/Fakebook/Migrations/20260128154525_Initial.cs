using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Fakebook.Migrations
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
                name: "fakebook_profiles",
                columns: table => new
                {
                    profile_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    avatar = table.Column<string>(type: "longtext", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fakebook_profiles", x => x.profile_id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false),
                    visibility = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
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
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_reactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    target_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    reacted_by = table.Column<Guid>(type: "char(36)", nullable: false),
                    reaction_type = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
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
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_post_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: false),
                    post_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
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
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fakebook_post_media",
                columns: table => new
                {
                    post_media_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    post_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    content_type = table.Column<string>(type: "longtext", nullable: false),
                    path = table.Column<string>(type: "longtext", nullable: false),
                    size = table.Column<double>(type: "double", nullable: false)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_post_comments_created_by",
                table: "fakebook_post_comments",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_post_comments_post_id",
                table: "fakebook_post_comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_post_media_post_id",
                table: "fakebook_post_media",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_posts_created_by",
                table: "fakebook_posts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_reactions_reacted_by_target_type_target_id",
                table: "fakebook_reactions",
                columns: new[] { "reacted_by", "target_type", "target_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fakebook_reactions_target_type_target_id",
                table: "fakebook_reactions",
                columns: new[] { "target_type", "target_id" });
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
                name: "fakebook_profiles");
        }
    }
}
