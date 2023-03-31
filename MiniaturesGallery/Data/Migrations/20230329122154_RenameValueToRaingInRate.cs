using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniaturesGallery.Data.Migrations
{
    public partial class RenameValueToRaingInRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Rates",
                newName: "Rating");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Rates",
                newName: "Value");
        }
    }
}
