using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.data.Migrations
{
    /// <inheritdoc />
    public partial class MakeCampaignQuestionnaireNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_campaigns_questionnaires_QuestionnaireId",
                table: "campaigns");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionnaireId",
                table: "campaigns",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_campaigns_questionnaires_QuestionnaireId",
                table: "campaigns",
                column: "QuestionnaireId",
                principalTable: "questionnaires",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_campaigns_questionnaires_QuestionnaireId",
                table: "campaigns");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionnaireId",
                table: "campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_campaigns_questionnaires_QuestionnaireId",
                table: "campaigns",
                column: "QuestionnaireId",
                principalTable: "questionnaires",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
