using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NewBalanceShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "Color", "Description", "ImageUrl", "Name", "Price", "Size", "Stock" },
                values: new object[,]
                {
                    { 4, "New Balance", 1, "Бежевий", "Низькі кросівки в стилі баскетбольної класики 80-х: шкіра, масивна підошва.", "/images/nb480.jpg", "New Balance 480", 3799m, "40", 12 },
                    { 5, "New Balance", 1, "Срібний", "Технологічні кросівки з металізованими вставками й амортизацією ABZORB.", "/images/nb1906r.jpg", "New Balance 1906R", 5599m, "44", 8 },
                    { 6, "New Balance", 1, "Сливовий", "Замша та сітка, підошва N-ERGY: комфорт на цілий день.", "/images/nb2002r.jpg", "New Balance 2002R", 5999m, "41", 6 },
                    { 7, "New Balance", 1, "Коричневий", "Скейт-версія 480 із замшею й посиленою підошвою.", "/images/nb-numeric480.jpg", "New Balance Numeric 480", 3999m, "45", 9 },
                    { 8, "New Balance", 3, "Сірий", "Бейсболка з вишитим логотипом, регульований ремінець.", "", "NB Essentials Cap", 799m, "One size", 40 },
                    { 9, "New Balance", 3, "Чорний", "Рюкзак 25 л для тренувань і міста, відділення для ноутбука.", "", "NB Sport Backpack", 1599m, "One size", 18 },
                    { 10, "New Balance", 2, "Білий", "Бавовняна футболка прямого крою з логотипом на грудях.", "", "NB Classic Tee", 999m, "M", 30 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "/images/nb-hoodie.jpg");
        }
    }
}
