using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Scientists.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DirectorsUniqueScientist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Directors_ScientistId",
                table: "Directors");

            migrationBuilder.CreateIndex(
                name: "IX_Directors_ScientistId",
                table: "Directors",
                column: "ScientistId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Directors_ScientistId",
                table: "Directors");

            migrationBuilder.CreateIndex(
                name: "IX_Directors_ScientistId",
                table: "Directors",
                column: "ScientistId");
        }
    }
}
