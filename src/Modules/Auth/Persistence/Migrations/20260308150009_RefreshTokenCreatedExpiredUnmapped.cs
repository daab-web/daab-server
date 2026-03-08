using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Daab.Modules.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenCreatedExpiredUnmapped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(
                    new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    new TimeSpan(0, 0, 0, 0, 0)
                )
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(
                    new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    new TimeSpan(0, 0, 0, 0, 0)
                )
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedAt", table: "RefreshTokens");

            migrationBuilder.DropColumn(name: "ExpiresAt", table: "RefreshTokens");
        }
    }
}
