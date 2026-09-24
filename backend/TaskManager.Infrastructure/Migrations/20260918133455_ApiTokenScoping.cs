using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApiTokenScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "ApiTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReadOnly",
                table: "ApiTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RateLimitPerMinute",
                table: "ApiTokens",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "ApiTokens");

            migrationBuilder.DropColumn(
                name: "IsReadOnly",
                table: "ApiTokens");

            migrationBuilder.DropColumn(
                name: "RateLimitPerMinute",
                table: "ApiTokens");
        }
    }
}
