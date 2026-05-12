using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multi_Library_Management_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryIdToTableSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LibraryId",
                table: "TableSeats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TableSeats_LibraryId",
                table: "TableSeats",
                column: "LibraryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableSeats_Libraries_LibraryId",
                table: "TableSeats",
                column: "LibraryId",
                principalTable: "Libraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableSeats_Libraries_LibraryId",
                table: "TableSeats");

            migrationBuilder.DropIndex(
                name: "IX_TableSeats_LibraryId",
                table: "TableSeats");

            migrationBuilder.DropColumn(
                name: "LibraryId",
                table: "TableSeats");
        }
    }
}
