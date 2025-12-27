using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staffing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStaffingMetricTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffingMetrics_division_DivisionId",
                table: "StaffingMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffingMetrics_position_PositionId",
                table: "StaffingMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffingMetrics_report_import_ImportId",
                table: "StaffingMetrics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffingMetrics",
                table: "StaffingMetrics");

            migrationBuilder.DropIndex(
                name: "IX_StaffingMetrics_ImportId",
                table: "StaffingMetrics");

            migrationBuilder.RenameTable(
                name: "StaffingMetrics",
                newName: "staffing_metric");

            migrationBuilder.RenameIndex(
                name: "IX_StaffingMetrics_PositionId",
                table: "staffing_metric",
                newName: "IX_staffing_metric_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_StaffingMetrics_DivisionId",
                table: "staffing_metric",
                newName: "IX_staffing_metric_DivisionId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "staffing_metric",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MetricValue",
                table: "staffing_metric",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "MetricKey",
                table: "staffing_metric",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_staffing_metric",
                table: "staffing_metric",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_staffing_metric_ImportId_DivisionId_PositionId_MetricKey_Me~",
                table: "staffing_metric",
                columns: new[] { "ImportId", "DivisionId", "PositionId", "MetricKey", "MetricDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffing_metric_MetricDate",
                table: "staffing_metric",
                column: "MetricDate");

            migrationBuilder.CreateIndex(
                name: "IX_staffing_metric_MetricKey",
                table: "staffing_metric",
                column: "MetricKey");

            migrationBuilder.AddForeignKey(
                name: "FK_staffing_metric_division_DivisionId",
                table: "staffing_metric",
                column: "DivisionId",
                principalTable: "division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_staffing_metric_position_PositionId",
                table: "staffing_metric",
                column: "PositionId",
                principalTable: "position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_staffing_metric_report_import_ImportId",
                table: "staffing_metric",
                column: "ImportId",
                principalTable: "report_import",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staffing_metric_division_DivisionId",
                table: "staffing_metric");

            migrationBuilder.DropForeignKey(
                name: "FK_staffing_metric_position_PositionId",
                table: "staffing_metric");

            migrationBuilder.DropForeignKey(
                name: "FK_staffing_metric_report_import_ImportId",
                table: "staffing_metric");

            migrationBuilder.DropPrimaryKey(
                name: "PK_staffing_metric",
                table: "staffing_metric");

            migrationBuilder.DropIndex(
                name: "IX_staffing_metric_ImportId_DivisionId_PositionId_MetricKey_Me~",
                table: "staffing_metric");

            migrationBuilder.DropIndex(
                name: "IX_staffing_metric_MetricDate",
                table: "staffing_metric");

            migrationBuilder.DropIndex(
                name: "IX_staffing_metric_MetricKey",
                table: "staffing_metric");

            migrationBuilder.RenameTable(
                name: "staffing_metric",
                newName: "StaffingMetrics");

            migrationBuilder.RenameIndex(
                name: "IX_staffing_metric_PositionId",
                table: "StaffingMetrics",
                newName: "IX_StaffingMetrics_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_staffing_metric_DivisionId",
                table: "StaffingMetrics",
                newName: "IX_StaffingMetrics_DivisionId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "StaffingMetrics",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MetricValue",
                table: "StaffingMetrics",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "MetricKey",
                table: "StaffingMetrics",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffingMetrics",
                table: "StaffingMetrics",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingMetrics_ImportId",
                table: "StaffingMetrics",
                column: "ImportId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffingMetrics_division_DivisionId",
                table: "StaffingMetrics",
                column: "DivisionId",
                principalTable: "division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffingMetrics_position_PositionId",
                table: "StaffingMetrics",
                column: "PositionId",
                principalTable: "position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffingMetrics_report_import_ImportId",
                table: "StaffingMetrics",
                column: "ImportId",
                principalTable: "report_import",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
