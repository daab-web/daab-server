using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Scientists.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Scientists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AcademicTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Institution = table.Column<string>(type: "TEXT", nullable: false),
                    Countries = table.Column<string>(type: "TEXT", nullable: false),
                    Areas = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scientists", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scientists");
        }
    }
}
