using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Multi_Library_Management_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_TableSeats_SeatId",
                table: "StudentRegistrations");

            migrationBuilder.RenameColumn(
                name: "SeatId",
                table: "StudentRegistrations",
                newName: "TableSeatId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRegistrations_SeatId",
                table: "StudentRegistrations",
                newName: "IX_StudentRegistrations_TableSeatId");

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "StudentRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TableSeatId1",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LibraryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTime = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndTime = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(UTC_TIMESTAMP())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_BatchId",
                table: "StudentRegistrations",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_TableSeatId1",
                table: "StudentRegistrations",
                column: "TableSeatId1");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_LibraryId",
                table: "Batches",
                column: "LibraryId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Batches_BatchId",
                table: "StudentRegistrations",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_TableSeats_TableSeatId",
                table: "StudentRegistrations",
                column: "TableSeatId",
                principalTable: "TableSeats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_TableSeats_TableSeatId1",
                table: "StudentRegistrations",
                column: "TableSeatId1",
                principalTable: "TableSeats",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Batches_BatchId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_TableSeats_TableSeatId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_TableSeats_TableSeatId1",
                table: "StudentRegistrations");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_BatchId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_TableSeatId1",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "TableSeatId1",
                table: "StudentRegistrations");

            migrationBuilder.RenameColumn(
                name: "TableSeatId",
                table: "StudentRegistrations",
                newName: "SeatId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRegistrations_TableSeatId",
                table: "StudentRegistrations",
                newName: "IX_StudentRegistrations_SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_TableSeats_SeatId",
                table: "StudentRegistrations",
                column: "SeatId",
                principalTable: "TableSeats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
