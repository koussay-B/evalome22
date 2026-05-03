using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.data.Migrations
{
    /// <inheritdoc />
    public partial class MultiQuestionnairePerCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_campaigns_questionnaires_QuestionnaireId",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "IX_campaigns_QuestionnaireId",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "QuestionnaireId",
                table: "campaigns");

            migrationBuilder.AddColumn<int>(
                name: "QuestionnaireId",
                table: "candidate_attempts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "campaign_questionnaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    QuestionnaireId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Enable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaign_questionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_campaign_questionnaires_campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_campaign_questionnaires_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidate_attempts_QuestionnaireId",
                table: "candidate_attempts",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_campaign_questionnaires_CampaignId_QuestionnaireId",
                table: "campaign_questionnaires",
                columns: new[] { "CampaignId", "QuestionnaireId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaign_questionnaires_QuestionnaireId",
                table: "campaign_questionnaires",
                column: "QuestionnaireId");

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_attempts_questionnaires_QuestionnaireId",
                table: "candidate_attempts",
                column: "QuestionnaireId",
                principalTable: "questionnaires",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_candidate_attempts_questionnaires_QuestionnaireId",
                table: "candidate_attempts");

            migrationBuilder.DropTable(
                name: "campaign_questionnaires");

            migrationBuilder.DropIndex(
                name: "IX_candidate_attempts_QuestionnaireId",
                table: "candidate_attempts");

            migrationBuilder.DropColumn(
                name: "QuestionnaireId",
                table: "candidate_attempts");

            migrationBuilder.AddColumn<int>(
                name: "QuestionnaireId",
                table: "campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_QuestionnaireId",
                table: "campaigns",
                column: "QuestionnaireId");

            migrationBuilder.AddForeignKey(
                name: "FK_campaigns_questionnaires_QuestionnaireId",
                table: "campaigns",
                column: "QuestionnaireId",
                principalTable: "questionnaires",
                principalColumn: "Id");
        }
    }
}
