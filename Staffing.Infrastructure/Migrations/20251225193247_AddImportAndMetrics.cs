using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Staffing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportAndMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Divisions",
                table: "Divisions");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "position");

            migrationBuilder.RenameTable(
                name: "Divisions",
                newName: "division");

            migrationBuilder.AlterColumn<string>(
                name: "ReportTemplateKey",
                table: "position",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "position",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NameOriginal",
                table: "division",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "division",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTotalLevel",
                table: "division",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "division",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_position",
                table: "position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_division",
                table: "division",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "report_import",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SourceFileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PositionId = table.Column<long>(type: "bigint", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TemplateVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DqErrorsCount = table.Column<int>(type: "integer", nullable: false),
                    DqWarningsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_import", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_import_position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "position",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffingMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImportId = table.Column<long>(type: "bigint", nullable: false),
                    DivisionId = table.Column<long>(type: "bigint", nullable: false),
                    PositionId = table.Column<long>(type: "bigint", nullable: false),
                    MetricKey = table.Column<string>(type: "text", nullable: false),
                    MetricValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MetricUnit = table.Column<int>(type: "integer", nullable: false),
                    MetricDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValueSource = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffingMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffingMetrics_division_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffingMetrics_position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "position",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffingMetrics_report_import_ImportId",
                        column: x => x.ImportId,
                        principalTable: "report_import",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_position_Name",
                table: "position",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_position_ReportTemplateKey",
                table: "position",
                column: "ReportTemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_division_Code",
                table: "division",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_division_NameOriginal",
                table: "division",
                column: "NameOriginal");

            migrationBuilder.CreateIndex(
                name: "IX_division_ParentId",
                table: "division",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_report_import_PositionId_ReportDate_SourceFileHash_Template~",
                table: "report_import",
                columns: new[] { "PositionId", "ReportDate", "SourceFileHash", "TemplateVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_import_UploadedAt",
                table: "report_import",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingMetrics_DivisionId",
                table: "StaffingMetrics",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingMetrics_ImportId",
                table: "StaffingMetrics",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingMetrics_PositionId",
                table: "StaffingMetrics",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_division_division_ParentId",
                table: "division",
                column: "ParentId",
                principalTable: "division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_division_division_ParentId",
                table: "division");

            migrationBuilder.DropTable(
                name: "StaffingMetrics");

            migrationBuilder.DropTable(
                name: "report_import");

            migrationBuilder.DropPrimaryKey(
                name: "PK_position",
                table: "position");

            migrationBuilder.DropIndex(
                name: "IX_position_Name",
                table: "position");

            migrationBuilder.DropIndex(
                name: "IX_position_ReportTemplateKey",
                table: "position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_division",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_Code",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_NameOriginal",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_ParentId",
                table: "division");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "division");

            migrationBuilder.DropColumn(
                name: "IsTotalLevel",
                table: "division");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "division");

            migrationBuilder.RenameTable(
                name: "position",
                newName: "Positions");

            migrationBuilder.RenameTable(
                name: "division",
                newName: "Divisions");

            migrationBuilder.AlterColumn<string>(
                name: "ReportTemplateKey",
                table: "Positions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Positions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameOriginal",
                table: "Divisions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Divisions",
                table: "Divisions",
                column: "Id");
        }
    }
}
