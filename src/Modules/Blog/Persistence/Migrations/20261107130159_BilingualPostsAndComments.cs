using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Blog.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BilingualPostsAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. New per-language tables and the comment language column.
            migrationBuilder.CreateTable(
                name: "PostTranslations",
                columns: table => new
                {
                    PostId = table.Column<string>(type: "TEXT", nullable: false),
                    Locale = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Excerpt = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    EditorState = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTranslations", x => new { x.PostId, x.Locale });
                    table.ForeignKey(
                        name: "FK_PostTranslations_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentTexts",
                columns: table => new
                {
                    CommentId = table.Column<string>(type: "TEXT", nullable: false),
                    Locale = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentTexts", x => new { x.CommentId, x.Locale });
                    table.ForeignKey(
                        name: "FK_CommentTexts_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<string>(
                name: "SourceLocale",
                table: "Comments",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            // 2. Copy existing content: each post and comment keeps its original-language text;
            //    a comment's cached machine translations become its other-language texts.
            migrationBuilder.Sql(
                """
                INSERT INTO "PostTranslations" ("PostId", "Locale", "Title", "Excerpt", "EditorState")
                SELECT "Id", "Locale", "Title", "Excerpt", "EditorState" FROM "Posts";
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Comments" SET "SourceLocale" = COALESCE(
                    CASE WHEN "Language" IN ('en', 'az') THEN "Language" END,
                    (SELECT "Locale" FROM "Posts" WHERE "Posts"."Id" = "Comments"."PostId"),
                    'en');
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "CommentTexts" ("CommentId", "Locale", "Content")
                SELECT "Id", "SourceLocale", "Content" FROM "Comments";
                INSERT OR IGNORE INTO "CommentTexts" ("CommentId", "Locale", "Content")
                SELECT "CommentId", "Locale", "Content" FROM "CommentTranslations";
                """);

            // 3. Drop the single-language columns.
            migrationBuilder.DropTable(
                name: "CommentTranslations");

            migrationBuilder.DropColumn(
                name: "EditorState",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Excerpt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "Locale",
                table: "Posts",
                newName: "SourceLocale");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentTexts");

            migrationBuilder.DropTable(
                name: "PostTranslations");

            migrationBuilder.DropColumn(
                name: "SourceLocale",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "SourceLocale",
                table: "Posts",
                newName: "Locale");

            migrationBuilder.AddColumn<string>(
                name: "EditorState",
                table: "Posts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Excerpt",
                table: "Posts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Posts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Comments",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Comments",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommentTranslations",
                columns: table => new
                {
                    CommentId = table.Column<string>(type: "TEXT", nullable: false),
                    Locale = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentTranslations", x => new { x.CommentId, x.Locale });
                    table.ForeignKey(
                        name: "FK_CommentTranslations_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
