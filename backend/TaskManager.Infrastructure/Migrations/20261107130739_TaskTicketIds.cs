using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaskTicketIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Projects",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NextTaskNumber",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Number existing tasks 1, 2, 3… per project, oldest first, and continue each
            // project's counter after its highest number. Runs before the unique index below.
            // Project keys are backfilled at startup (DataSeeder.BackfillProjectKeysAsync).
            migrationBuilder.Sql("""
                UPDATE "Tasks" t SET "Number" = n.rn
                FROM (SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "ProjectId" ORDER BY "CreatedAt", "Id") AS rn
                      FROM "Tasks") n
                WHERE t."Id" = n."Id";

                UPDATE "Projects" p SET "NextTaskNumber" =
                    COALESCE((SELECT MAX(t."Number") FROM "Tasks" t WHERE t."ProjectId" = p."Id"), 0) + 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId_Number",
                table: "Tasks",
                columns: new[] { "ProjectId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Key",
                table: "Projects",
                column: "Key",
                unique: true,
                filter: "\"Key\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectId_Number",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Key",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "NextTaskNumber",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");
        }
    }
}
