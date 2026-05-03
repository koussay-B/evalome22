using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowNavigationBack",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPartialScore",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowTabSwitch",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CooldownBetweenAttemptsMinutes",
                table: "campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GlobalDurationMinutes",
                table: "campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PassingScorePercent",
                table: "campaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RandomizeChoices",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RandomizeQuestions",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScoringMode",
                table: "campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectAfterEach",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowQuestionScore",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowResultsImmediately",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTimer",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TabSwitchMaxCount",
                table: "campaigns",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowNavigationBack",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "AllowPartialScore",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "AllowTabSwitch",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "CooldownBetweenAttemptsMinutes",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "GlobalDurationMinutes",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "PassingScorePercent",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "RandomizeChoices",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "RandomizeQuestions",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "ScoringMode",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAfterEach",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "ShowQuestionScore",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "ShowResultsImmediately",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "ShowTimer",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "TabSwitchMaxCount",
                table: "campaigns");
        }
    }
}
