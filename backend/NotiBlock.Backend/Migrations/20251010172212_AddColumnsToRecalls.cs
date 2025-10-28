using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotiBlock.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToRecalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManufacturerId",
                table: "Recalls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TransactionHash",
                table: "Recalls",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recalls_ManufacturerId",
                table: "Recalls",
                column: "ManufacturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recalls_AppUsers_ManufacturerId",
                table: "Recalls",
                column: "ManufacturerId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recalls_AppUsers_ManufacturerId",
                table: "Recalls");

            migrationBuilder.DropIndex(
                name: "IX_Recalls_ManufacturerId",
                table: "Recalls");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "Recalls");

            migrationBuilder.DropColumn(
                name: "TransactionHash",
                table: "Recalls");
        }
    }
}
