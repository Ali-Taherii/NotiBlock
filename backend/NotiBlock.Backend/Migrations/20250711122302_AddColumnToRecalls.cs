using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotiBlock.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnToRecalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionRequired",
                table: "Recalls",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Consumers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionRequired",
                table: "Recalls");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Consumers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
