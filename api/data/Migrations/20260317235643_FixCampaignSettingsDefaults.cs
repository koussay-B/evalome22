using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.data.Migrations
{
    /// <inheritdoc />
    public partial class FixCampaignSettingsDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix existing campaigns: apply correct C# defaults for newly added columns
            migrationBuilder.Sql(@"
                UPDATE campaigns SET
                    ""AllowNavigationBack""    = TRUE,
                    ""AllowPartialScore""      = TRUE,
                    ""ShowTimer""              = TRUE,
                    ""ShowResultsImmediately"" = TRUE,
                    ""MaxAttempts""            = 1,
                    ""PassingScorePercent""    = 60
            ");

            // Also update the column defaults so future bare INSERTs get correct values
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""AllowNavigationBack""    SET DEFAULT TRUE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""AllowPartialScore""      SET DEFAULT TRUE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""ShowTimer""              SET DEFAULT TRUE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""ShowResultsImmediately"" SET DEFAULT TRUE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""MaxAttempts""            SET DEFAULT 1");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""PassingScorePercent""    SET DEFAULT 60");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""AllowNavigationBack""    SET DEFAULT FALSE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""AllowPartialScore""      SET DEFAULT FALSE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""ShowTimer""              SET DEFAULT FALSE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""ShowResultsImmediately"" SET DEFAULT FALSE");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""MaxAttempts""            SET DEFAULT 0");
            migrationBuilder.Sql(@"ALTER TABLE campaigns ALTER COLUMN ""PassingScorePercent""    SET DEFAULT 0.0");
        }
    }
}
