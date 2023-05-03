using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryApplication.Migrations
{
    public partial class Fifth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnTime",
                table: "Borrows",
                newName: "ReturnDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnDate",
                table: "Borrows",
                newName: "ReturnTime");
        }
    }
}
