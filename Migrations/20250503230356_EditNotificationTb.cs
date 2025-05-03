using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateHub.Migrations
{
    /// <inheritdoc />
    public partial class EditNotificationTb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_VehicleOwners_VehicleOwnerId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_VehicleOwnerId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "VehicleOwnerId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Statue",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Body",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "Statue");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Notifications",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "VehicleOwnerId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_VehicleOwnerId",
                table: "Notifications",
                column: "VehicleOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_VehicleOwners_VehicleOwnerId",
                table: "Notifications",
                column: "VehicleOwnerId",
                principalTable: "VehicleOwners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
