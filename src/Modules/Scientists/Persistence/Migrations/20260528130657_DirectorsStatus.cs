using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Scientists.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DirectorsStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Directors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Directors");
        }
    }
}
