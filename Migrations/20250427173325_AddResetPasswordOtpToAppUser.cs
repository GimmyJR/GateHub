using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateHub.Migrations
{
    /// <inheritdoc />
    public partial class AddResetPasswordOtpToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResetPasswordOTPUsed",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordOTP",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordOTPExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResetPasswordOTPUsed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOTP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOTPExpiry",
                table: "AspNetUsers");
        }
    }
}
