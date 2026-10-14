using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewBalanceShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BetterSneakerPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Color", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Срібний", "Бігові кросівки зі сріблястим верхом і помаранчевими акцентами, амортизація ABZORB.", "/images/nb1062.jpg", "New Balance 1062", 4799m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Color", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Лавандовий", "Жіноча версія класичної 574 у ніжних пастельних відтінках.", "/images/nb574w.jpg", "New Balance 574 Pastel Women", 3999m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Color", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Синій", "Стабільні бігові кросівки з сріблясто-синім верхом і підтримкою стопи.", "/images/nb1225.jpg", "New Balance 1225", 4299m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Color", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Сірий", "Версія 1906 із темним сітчастим верхом і амортизацією ABZORB для щоденного бігу.", "/images/nb1906d.jpg", "New Balance 1906D", 5799m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Color", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Сірий", "Жіноча модель легендарної лінійки 990: замша, сітка, підтримка стопи.", "/images/nb990w.jpg", "New Balance 990 Women", 6499m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Color", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Білий", "Максимально м'яка бігова пара на піні Fresh Foam X.", "/images/nb-freshfoam.jpg", "New Balance Fresh Foam More", 4899m });
        }
    }
}
