using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewBalanceShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsAdmin",
                value: false);

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "City", "Country", "Email", "FullName", "GoogleUid", "IsAdmin", "IsBlocked", "PasswordHash", "Phone" },
                values: new object[] { 2, "", "", "admin@newbalanceshop.com", "Адміністратор", null, true, false, "100000.F143Lucn3B8Aosfv7xB7fw==.2DJjkGv3OvBUOCcnZWmuo0Fd0cg4FgHHCECXf17sCFc=", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Customers");
        }
    }
}
