using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace rcsa.Migrations
{
    /// <inheritdoc />
    public partial class newsss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainHeading",
                table: "Headings");

            migrationBuilder.DropColumn(
                name: "SubHeading",
                table: "Headings");

            migrationBuilder.RenameColumn(
                name: "SubHeading2",
                table: "Headings",
                newName: "Title");

            migrationBuilder.AlterColumn<int>(
                name: "Marks",
                table: "Assessments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MainCategory = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    SubCategory = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    SubCategory2 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentID",
                        column: x => x.ParentID,
                        principalTable: "Categories",
                        principalColumn: "ID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubHeadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title_Sub = table.Column<string>(type: "longtext", nullable: false),
                    HeadingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubHeadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubHeadings_Headings_HeadingId",
                        column: x => x.HeadingId,
                        principalTable: "Headings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentID",
                table: "Categories",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_SubHeadings_HeadingId",
                table: "SubHeadings",
                column: "HeadingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "SubHeadings");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Headings",
                newName: "SubHeading2");

            migrationBuilder.AddColumn<string>(
                name: "MainHeading",
                table: "Headings",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "SubHeading",
                table: "Headings",
                type: "longtext",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Marks",
                table: "Assessments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
