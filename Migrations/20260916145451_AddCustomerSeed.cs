using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewBalanceShop.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash" },
                values: new object[] { 1, "test.customer@example.com", "Тестовий Покупець", "seed-no-auth-yet" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
