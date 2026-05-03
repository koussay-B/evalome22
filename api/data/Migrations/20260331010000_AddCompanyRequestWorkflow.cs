using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace api.data.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20260331010000_AddCompanyRequestWorkflow")]
    public partial class AddCompanyRequestWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterEmail",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequesterName",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Companies",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "Companies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "Companies"
                SET "Status" = 1,
                    "IsActive" = TRUE
                WHERE "Enable" = TRUE;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RequesterEmail",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RequesterName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Companies");
        }
    }
}
