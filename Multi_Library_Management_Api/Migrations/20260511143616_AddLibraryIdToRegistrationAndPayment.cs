using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multi_Library_Management_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryIdToRegistrationAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LibraryId",
                table: "StudentRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LibraryId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_LibraryId",
                table: "StudentRegistrations",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LibraryId",
                table: "Payments",
                column: "LibraryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Libraries_LibraryId",
                table: "Payments",
                column: "LibraryId",
                principalTable: "Libraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Libraries_LibraryId",
                table: "StudentRegistrations",
                column: "LibraryId",
                principalTable: "Libraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Libraries_LibraryId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Libraries_LibraryId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_LibraryId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Payments_LibraryId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LibraryId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "LibraryId",
                table: "Payments");
        }
    }
}
