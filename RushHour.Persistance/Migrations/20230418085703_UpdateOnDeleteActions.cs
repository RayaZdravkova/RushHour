using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RushHour.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOnDeleteActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Providers_ProviderId",
                table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Providers_ProviderId",
                table: "Employees",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Providers_ProviderId",
                table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Providers_ProviderId",
                table: "Employees",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
