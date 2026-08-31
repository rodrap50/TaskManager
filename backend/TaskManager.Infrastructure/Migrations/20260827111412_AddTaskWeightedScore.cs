using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskWeightedScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WeightedScore",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows with a real computed score (PRI01.2's formula) rather
            // than leaving them at the placeholder 0 default above. TaskVotes is always
            // empty at this point in the migration history (its own migration only just
            // preceded this one), so every row's AvgUserVote falls back to its own
            // PriorityNumeric, matching WeightedScoreCalculator.ComputeAvgUserVote's rule.
            migrationBuilder.Sql(
                """
                UPDATE "Tasks" t
                SET "WeightedScore" = LEAST(10, GREATEST(1,
                    CEIL(
                        (2.0 * CASE t."Priority"
                            WHEN 0 THEN 3
                            WHEN 1 THEN 7
                            WHEN 2 THEN 9
                            WHEN 3 THEN 10
                        END + p."CriticalityScore") / 3.0
                    )
                ))
                FROM "Projects" p
                WHERE p."Id" = t."ProjectId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightedScore",
                table: "Tasks");
        }
    }
}
