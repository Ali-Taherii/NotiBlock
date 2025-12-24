using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotiBlock.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedShadowState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResellerTickets_Regulators_ApprovedById1",
                table: "ResellerTickets");

            migrationBuilder.DropIndex(
                name: "IX_ResellerTickets_ApprovedById1",
                table: "ResellerTickets");

            migrationBuilder.DropColumn(
                name: "ApprovedById1",
                table: "ResellerTickets");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "ResellerTickets");

            // Recreate with correct type
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById",
                table: "ResellerTickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResellerTickets_ApprovedById",
                table: "ResellerTickets",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ResellerTickets_Regulators_ApprovedById",
                table: "ResellerTickets",
                column: "ApprovedById",
                principalTable: "Regulators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResellerTickets_Regulators_ApprovedById",
                table: "ResellerTickets");

            migrationBuilder.DropIndex(
                name: "IX_ResellerTickets_ApprovedById",
                table: "ResellerTickets");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "ResellerTickets");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "ResellerTickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById1",
                table: "ResellerTickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResellerTickets_ApprovedById1",
                table: "ResellerTickets",
                column: "ApprovedById1");

            migrationBuilder.AddForeignKey(
                name: "FK_ResellerTickets_Regulators_ApprovedById1",
                table: "ResellerTickets",
                column: "ApprovedById1",
                principalTable: "Regulators",
                principalColumn: "Id");
        }
    }
}