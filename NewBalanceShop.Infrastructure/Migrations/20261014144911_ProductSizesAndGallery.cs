using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NewBalanceShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductSizesAndGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "Products",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "ExtraImages",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "OrderItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "/images/nb574-b.jpg,/images/nb574-c.jpg", "38,39,40,41,42,43,44,45" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "", "40,41,42,43,44,45" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "", "S,M,L,XL" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "", "36,37,38,39,40,41,42" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "", "40,41,42,43,44,45,46" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "", "38,39,40,41,42,43" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "", "41,42,43,44,45,46" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "ExtraImages",
                value: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "ExtraImages",
                value: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ExtraImages", "Size" },
                values: new object[] { "", "S,M,L,XL" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "Color", "Description", "ExtraImages", "ImageUrl", "Name", "Price", "Size", "Stock" },
                values: new object[,]
                {
                    { 11, "New Balance", 1, "Сірий", "Версія 1906 із темним сітчастим верхом і амортизацією ABZORB для щоденного бігу.", "", "/images/nb1906d.jpg", "New Balance 1906D", 5799m, "40,41,42,43,44,45", 14 },
                    { 12, "New Balance", 1, "Сірий", "Жіноча модель легендарної лінійки 990: замша, сітка, підтримка стопи.", "", "/images/nb990w.jpg", "New Balance 990 Women", 6499m, "36,37,38,39,40,41", 10 },
                    { 13, "New Balance", 1, "Білий", "Максимально м'яка бігова пара на піні Fresh Foam X.", "", "/images/nb-freshfoam.jpg", "New Balance Fresh Foam More", 4899m, "40,41,42,43,44,45,46", 16 },
                    { 14, "New Balance", 1, "Чорний", "Трейлові кросівки з агресивним протектором і піною Fresh Foam.", "", "/images/nb-trailmore.jpg", "New Balance Fresh Foam Trail", 4599m, "40,41,42,43,44,45,46", 11 },
                    { 15, "New Balance", 2, "Чорний", "Легка спортивна куртка на блискавці з кишенями.", "", "", "NB Track Jacket", 2499m, "S,M,L,XL,XXL", 22 },
                    { 16, "New Balance", 2, "Сірий", "Шорти для тренувань із вологовідвідною тканиною.", "", "", "NB Training Shorts", 1199m, "S,M,L,XL", 28 },
                    { 17, "New Balance", 3, "Чорний", "В'язана шапка з відворотом і вишитим логотипом.", "", "", "NB Knit Beanie", 699m, "One size", 35 },
                    { 18, "New Balance", 3, "Білий", "Три пари спортивних шкарпеток із посиленою п'ятою.", "", "", "NB Performance Socks (3 pack)", 549m, "S,M,L", 60 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DropColumn(
                name: "ExtraImages",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "Products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Size",
                value: "42");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Size",
                value: "43");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Size",
                value: "L");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Size",
                value: "40");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "Size",
                value: "44");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Size",
                value: "41");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "Size",
                value: "45");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "Size",
                value: "M");
        }
    }
}
