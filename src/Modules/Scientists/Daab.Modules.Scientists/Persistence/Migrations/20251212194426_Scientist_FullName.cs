using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Scientists.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Scientist_FullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Scientists",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Scientists");
        }
    }
}
