using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Activities.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewsSlugIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_News_Slug",
                table: "News",
                column: "Slug",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_News_Slug", table: "News");
        }
    }
}
