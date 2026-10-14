using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewBalanceShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ShortsAndSocksPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Color", "Description", "ImageUrl" },
                values: new object[] { "Чорний", "Легкі бігові шорти з логотипом New Balance і вологовідвідною тканиною.", "/images/nb-shorts.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Color", "Description", "ImageUrl", "Name" },
                values: new object[] { "Смугастий", "Теплі шкарпетки з вовни в смужку для прогулянок і холодної погоди.", "/images/nb-socks.jpg", "NB Knit Wool Socks" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Color", "Description", "ImageUrl" },
                values: new object[] { "Сірий", "Шорти для тренувань із вологовідвідною тканиною.", "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Color", "Description", "ImageUrl", "Name" },
                values: new object[] { "Білий", "Три пари спортивних шкарпеток із посиленою п'ятою.", "", "NB Performance Socks (3 pack)" });
        }
    }
}
