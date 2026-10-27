using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlowerShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PetalColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliverySlot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Flowers = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    PetalColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Featured = table.Column<bool>(type: "boolean", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name", "PetalColor", "Slug" },
                values: new object[,]
                {
                    { 1, "Bó hoa cho sinh nhật, kỷ niệm và những lời muốn nói.", "Hoa bó", "#d9506f", "hoa-bo" },
                    { 2, "Hoa cắm sẵn, đặt lên bàn là đẹp.", "Giỏ & hộp hoa", "#8fa8e8", "gio-hop" },
                    { 3, "Kệ hoa chúc mừng, nổi bật trước cửa hàng.", "Hoa khai trương", "#ee8a33", "khai-truong" },
                    { 4, "Hoa cầm tay và phụ kiện cho ngày trọng đại.", "Hoa cưới", "#e9c9d2", "hoa-cuoi" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "Address", "CreatedAt", "CustomerName", "DeliveryDate", "DeliverySlot", "Message", "Phone", "Status" },
                values: new object[,]
                {
                    { 1, "15 Trần Hưng Đạo, Quận 5", new DateTime(2026, 10, 23, 3, 0, 0, 0, DateTimeKind.Utc), "Hoàng Đức Long", new DateOnly(2026, 10, 25), "08:00 – 10:00", null, "0900 000 007", "Completed" },
                    { 2, "7 Võ Văn Tần, Quận 3", new DateTime(2026, 10, 23, 3, 0, 0, 0, DateTimeKind.Utc), "Đặng Khánh Linh", new DateOnly(2026, 10, 25), "19:00 – 21:00", null, "0900 000 006", "Cancelled" },
                    { 3, "102 Nguyễn Thị Minh Khai, Quận 3", new DateTime(2026, 10, 24, 3, 0, 0, 0, DateTimeKind.Utc), "Võ Anh Tuấn", new DateOnly(2026, 10, 26), "13:00 – 15:00", null, "0900 000 005", "Completed" },
                    { 4, "33 Lý Tự Trọng, Quận 1", new DateTime(2026, 10, 24, 3, 0, 0, 0, DateTimeKind.Utc), "Phạm Gia Hân", new DateOnly(2026, 10, 26), "08:00 – 10:00", null, "0900 000 004", "Completed" },
                    { 5, "9 Pasteur, Quận 1", new DateTime(2026, 10, 26, 3, 0, 0, 0, DateTimeKind.Utc), "Lê Bảo Châu", new DateOnly(2026, 10, 27), "17:00 – 19:00", null, "0900 000 003", "Preparing" },
                    { 6, "250 Điện Biên Phủ, Quận Bình Thạnh", new DateTime(2026, 10, 26, 3, 0, 0, 0, DateTimeKind.Utc), "Trần Minh Quân", new DateOnly(2026, 10, 27), "10:00 – 12:00", null, "0900 000 002", "Delivering" },
                    { 7, "21 Nguyễn Du, Quận 1", new DateTime(2026, 10, 27, 3, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Thu Hà", new DateOnly(2026, 10, 27), "15:00 – 17:00", null, "0900 000 001", "New" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Featured", "Flowers", "Name", "PetalColor", "Price", "Stock" },
                values: new object[,]
                {
                    { 1, 1, "Mười một bông hồng đỏ gói giấy kraft, thích hợp tặng người thương nhân dịp kỷ niệm hoặc Valentine.", true, "Hồng đỏ Ecuador, baby trắng, lá bạc", "Bó hồng đỏ Tình Yêu", "#c2274b", 450000m, 12 },
                    { 2, 1, "Sắc vàng rực rỡ mang năng lượng tích cực, hợp tặng bạn bè, đồng nghiệp hoặc chúc mừng tốt nghiệp.", true, "Hướng dương, cúc tana, lá bạc", "Bó hướng dương Nắng Sớm", "#f2b01e", 380000m, 8 },
                    { 3, 1, "Tulip nhập theo mùa, tông hồng phấn dịu dàng. Số lượng có hạn mỗi tuần.", true, "Tulip hồng, hoa mận, lá eucalyptus", "Bó tulip Hồng Pastel", "#f09ab5", 520000m, 4 },
                    { 4, 2, "Giỏ mây cắm sẵn với cẩm tú cầu đầy đặn, thêm thiệp viết tay theo yêu cầu.", true, "Cẩm tú cầu xanh, hồng phấn, cúc mẫu đơn", "Giỏ hoa Cẩm Tú Cầu", "#8fa8e8", 650000m, 6 },
                    { 5, 2, "Hộp tròn cắm hoa tươi nhiều tầng màu, gọn nhẹ, dễ mang đi tặng.", false, "Hồng spray, cát tường, hoa lan tỏi", "Hộp hoa Mùa Xuân", "#e58bb8", 590000m, 3 },
                    { 6, 3, "Kệ hoa hai tầng cao 1,6 m chúc mừng khai trương. Giao và dựng tại cửa hàng.", false, "Đồng tiền cam, hồng vàng, lan vũ nữ", "Kệ hoa Phát Tài", "#ee8a33", 1200000m, 5 },
                    { 7, 3, "Kệ hoa lớn tông đỏ – vàng, cao 1,8 m, phù hợp lễ khai trương, kỷ niệm thành lập.", false, "Hồng đỏ, lay ơn, hướng dương, lá phát tài", "Kệ hoa Thành Công", "#d63f3f", 1500000m, 2 },
                    { 8, 4, "Bó hoa cưới tông trắng kem, kèm hoa cài áo cho chú rể. Hẹn thử trước 3 ngày.", false, "Hồng trắng, lan Mokara, baby, lá bạc", "Hoa cầm tay Thiên Thần", "#efd9de", 850000m, 7 }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "OrderId", "ProductId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, 7, 1, 1500000m },
                    { 2, 2, 5, 1, 590000m },
                    { 3, 3, 2, 3, 380000m },
                    { 4, 4, 8, 1, 850000m },
                    { 5, 5, 3, 1, 520000m },
                    { 6, 5, 4, 1, 650000m },
                    { 7, 6, 6, 2, 1200000m },
                    { 8, 7, 1, 1, 450000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
