using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RushHour.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedForAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "Email", "FullName", "Password", "Role", "Salt", "Username" },
                values: new object[] { 1, "admin@prime.com", "Administrator", "703883388914CE4175C73C7ED3EBF62E0C3D44515F90CB6A06F32110E0443EE942EB9A0ED19495111501A2D4B16A9A7F1F6192A3B6B858C94309C12600F7D79C", 1, new byte[] { 64, 215, 132, 195, 127, 34, 219, 158, 179, 138, 120, 204, 209, 204, 255, 137, 254, 167, 60, 72, 91, 226, 66, 110, 131, 126, 79, 222, 229, 43, 172, 64, 219, 19, 250, 167, 204, 158, 209, 124, 233, 20, 42, 108, 13, 126, 136, 109, 175, 46, 237, 17, 192, 122, 155, 218, 204, 187, 62, 151, 237, 205, 102, 3 }, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Accounts");
        }
    }
}
