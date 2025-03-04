using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateHub.Migrations
{
    /// <inheritdoc />
    public partial class addgatefeemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GateFees",
                columns: table => new
                {
                    GateId = table.Column<int>(type: "int", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateFees", x => new { x.GateId, x.VehicleType });
                    table.ForeignKey(
                        name: "FK_GateFees_Gates_GateId",
                        column: x => x.GateId,
                        principalTable: "Gates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GateFees");
        }
    }
}
