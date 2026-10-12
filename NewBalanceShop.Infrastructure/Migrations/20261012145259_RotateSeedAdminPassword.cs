using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewBalanceShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RotateSeedAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "100000.zCfnaQu3HCBj8/w8gPw5qA==.6s19ORGZadTHN1m0Mjl/POEaX90xYzcaQbk6lOKMUZc=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "100000.F143Lucn3B8Aosfv7xB7fw==.2DJjkGv3OvBUOCcnZWmuo0Fd0cg4FgHHCECXf17sCFc=");
        }
    }
}
