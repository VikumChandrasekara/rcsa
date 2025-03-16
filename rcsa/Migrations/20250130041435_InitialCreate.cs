using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rcsa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HeadingText",
                table: "Headings",
                newName: "MainHeading");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MainHeading",
                table: "Headings",
                newName: "HeadingText");
        }
    }
}
