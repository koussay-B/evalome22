using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuestionnaireSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowNavigationBack",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "AllowPartialScore",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "AllowTabSwitch",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "CooldownBetweenAttemptsMinutes",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "GlobalDurationMinutes",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "PassingScorePercent",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "RandomizeChoices",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "RandomizeQuestions",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "ScoringMode",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAfterEach",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "ShowQuestionScore",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "ShowResultsImmediately",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "ShowTimer",
                table: "questionnaires");

            migrationBuilder.DropColumn(
                name: "TabSwitchMaxCount",
                table: "questionnaires");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowNavigationBack",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPartialScore",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowTabSwitch",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CooldownBetweenAttemptsMinutes",
                table: "questionnaires",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GlobalDurationMinutes",
                table: "questionnaires",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "questionnaires",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PassingScorePercent",
                table: "questionnaires",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RandomizeChoices",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RandomizeQuestions",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScoringMode",
                table: "questionnaires",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectAfterEach",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowQuestionScore",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowResultsImmediately",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTimer",
                table: "questionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TabSwitchMaxCount",
                table: "questionnaires",
                type: "integer",
                nullable: true);
        }
    }
}
