using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RushHour.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityEmployeeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Activities_ActivityId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ActivityId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Employees");

            migrationBuilder.CreateTable(
                name: "ActivityEmployees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityEmployees", x => new { x.ActivityId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_ActivityEmployees_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityEmployees_EmployeeId",
                table: "ActivityEmployees",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityEmployees");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ActivityId",
                table: "Employees",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Activities_ActivityId",
                table: "Employees",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id");
        }
    }
}
