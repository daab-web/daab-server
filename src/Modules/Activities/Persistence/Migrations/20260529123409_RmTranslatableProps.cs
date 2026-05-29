using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Activities.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RmTranslatableProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "news_title_idx",
                table: "News");

            migrationBuilder.DropColumn(
                name: "EditorState",
                table: "News");

            migrationBuilder.DropColumn(
                name: "Excerpt",
                table: "News");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "News");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EditorState",
                table: "News",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Excerpt",
                table: "News",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "News",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "news_title_idx",
                table: "News",
                column: "Title");
        }
    }
}
