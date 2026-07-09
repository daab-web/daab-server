using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Translation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SourceHash = table.Column<string>(type: "TEXT", nullable: false),
                    SourceText = table.Column<string>(type: "TEXT", nullable: false),
                    TargetText = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    BlockType = table.Column<string>(type: "TEXT", nullable: false),
                    SourceLocale = table.Column<string>(type: "TEXT", nullable: false),
                    TargetLocale = table.Column<string>(type: "TEXT", nullable: false),
                    Embedding = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TM_ExactLookup",
                table: "Entries",
                columns: new[] { "SourceHash", "ContentType", "BlockType", "SourceLocale", "TargetLocale" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries");
        }
    }
}
