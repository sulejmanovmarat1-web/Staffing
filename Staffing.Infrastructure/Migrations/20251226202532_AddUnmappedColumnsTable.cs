using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Staffing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnmappedColumnsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "unmapped_columns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImportId = table.Column<long>(type: "bigint", nullable: false),
                    PositionId = table.Column<long>(type: "bigint", nullable: false),
                    SheetName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Col = table.Column<int>(type: "integer", nullable: false),
                    HeaderPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    HeaderPathNormalized = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unmapped_columns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_unmapped_columns_ImportId",
                table: "unmapped_columns",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_unmapped_columns_ImportId_SheetName_Col",
                table: "unmapped_columns",
                columns: new[] { "ImportId", "SheetName", "Col" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "unmapped_columns");
        }
    }
}
