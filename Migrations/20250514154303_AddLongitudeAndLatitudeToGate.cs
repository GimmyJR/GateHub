using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateHub.Migrations
{
    /// <inheritdoc />
    public partial class AddLongitudeAndLatitudeToGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Gates",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Gates",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Gates");
        }
    }
}
