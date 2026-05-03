using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.data.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeToCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_ThemeId",
                table: "campaigns",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_campaigns_themes_ThemeId",
                table: "campaigns",
                column: "ThemeId",
                principalTable: "themes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_campaigns_themes_ThemeId",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "IX_campaigns_ThemeId",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "campaigns");
        }
    }
}
