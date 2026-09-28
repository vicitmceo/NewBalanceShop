using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewBalanceShop.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleUidToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleUid",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                column: "GoogleUid",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleUid",
                table: "Customers");
        }
    }
}
