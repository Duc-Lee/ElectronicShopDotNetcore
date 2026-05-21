using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectronicShopMVC.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ISBN",
                table: "Products",
                newName: "Brand");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentStatus",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherCode",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VoucherDiscount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VoucherId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiscountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxUses = table.Column<int>(type: "int", nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedBy", "DisplayOrder", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, false, "Laptop", null, null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedBy", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Điện thoại", null, null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "CreatedBy", "DisplayOrder", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, false, "Linh kiện PC", null, null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "CreatedBy", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Phụ kiện", null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Brand", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "Price", "Price100", "Price50", "SKU", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[] { "ASUS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cấu hình mạnh mẽ với RTX 4060, Intel Core i7 thế hệ mới nhất, màn hình 165Hz mượt mà.", false, 35000000m, 34000000m, 34500000m, "ES-LP-001", "Laptop Gaming ASUS ROG Strix", null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Brand", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "Price", "Price100", "Price50", "SKU", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[] { "Apple", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Thiết kế vỏ Titan bền bỉ, chip A17 Pro siêu mạnh, hệ thống camera chuyên nghiệp.", false, 29000000m, 28000000m, 28500000m, "ES-PH-002", "iPhone 15 Pro Max 256GB", null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Brand", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "Price", "Price100", "Price50", "SKU", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[] { "MSI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Hiệu năng đồ họa đỉnh cao cho gaming 2K và 4K, tản nhiệt 3 quạt siêu mát.", false, 22000000m, 21000000m, 21500000m, "ES-PC-003", "Card màn hình MSI RTX 4070 Ti", null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Brand", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "Price", "Price100", "Price50", "SKU", "Stock", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[] { "Logitech", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Trọng lượng siêu nhẹ, cảm biến HERO 25K chính xác vượt trội cho game thủ chuyên nghiệp.", false, 2500000m, 2300000m, 2400000m, "ES-AC-004", 15, "Chuột Logitech G Pro X Superlight", null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Brand", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "Price", "Price100", "Price50", "SKU", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[] { "Samsung", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bút S-Pen tiện lợi, camera 200MP zoom siêu xa, màn hình phẳng độ sáng cực cao.", false, 26000000m, 25000000m, 25500000m, "ES-PH-005", "Samsung Galaxy S24 Ultra", null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Brand", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "Price", "Price100", "Price50", "SKU", "Stock", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[] { "Akko", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kết nối không dây, switch Akko độc quyền, thiết kế nhỏ gọn đầy đủ phím số.", false, 1800000m, 1700000m, 1750000m, "ES-AC-006", 20, "Bàn phím Akko 3098B Multi-mode", null, null });

            migrationBuilder.InsertData(
                table: "Vouchers",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "DiscountType", "DiscountValue", "EndDate", "IsActive", "IsDeleted", "MaxDiscountAmount", "MaxUses", "MinOrderAmount", "StartDate", "Title", "UpdatedAt", "UpdatedBy", "UsedCount" },
                values: new object[,]
                {
                    { 1, "ESHOP10", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ưu đãi chiết khấu 10% cho toàn bộ giá trị giỏ hàng trước thuế.", "Percentage", 10m, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, false, null, 1000, 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Giảm giá 10% tổng hóa đơn", null, null, 0 },
                    { 2, "WELCOME50", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Chiết khấu 50.000đ trực tiếp vào hóa đơn thanh toán.", "FixedAmount", 50000m, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, false, null, 500, 100000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Giảm ngay 50.000đ cho đơn hàng", null, null, 0 },
                    { 3, "FREESHIP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Miễn phí 100% phí giao hàng cho tất cả các đơn hàng.", "FreeShipping", 0m, new DateTime(2026, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, false, null, 2000, 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Miễn phí vận chuyển toàn quốc", null, null, 0 }
                });

            // Clean up empty or duplicate SKUs to avoid Unique Index violations during migration
            migrationBuilder.Sql("UPDATE [Products] SET [SKU] = 'SKU-' + CAST([Id] AS VARCHAR(20)) WHERE [SKU] IS NULL OR [SKU] = '' OR [SKU] IN (SELECT [SKU] FROM [Products] GROUP BY [SKU] HAVING COUNT(*) > 1)");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VoucherId",
                table: "Orders",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId",
                table: "OrderStatusHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_OrderId",
                table: "PaymentTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Code",
                table: "Vouchers",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Vouchers_VoucherId",
                table: "Orders",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Vouchers_VoucherId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Products_SKU",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_VoucherId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FulfillmentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VoucherCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VoucherDiscount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "Products",
                newName: "ISBN");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DisplayOrder", "Name" },
                values: new object[] { 3, "Hành động" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Kịch tính");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DisplayOrder", "Name" },
                values: new object[] { 1, "Kinh dị" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Khoa học viễn tưởng");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Author", "Description", "ISBN", "Price", "Price100", "Price50", "Title" },
                values: new object[] { "Nguyễn Nhật Ánh", "Cuốn tiểu thuyết hành động với những pha giao tranh nghẹt thở giữa các nhân vật mạnh mẽ trong bối cảnh lịch sử Việt Nam.", "VN0001001", 120m, 100m, 110m, "Huyền Thoại Rồng Lửa" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Author", "Description", "ISBN", "Price", "Price100", "Price50", "Title" },
                values: new object[] { "Trần Anh Tuấn", "Một tác phẩm kịch tính xoay quanh những mối quan hệ phức tạp và những bí mật gia đình không thể giấu được.", "VN0001002", 100m, 90m, 95m, "Mê Cung Tình Yêu" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Author", "Description", "ISBN", "Price", "Price100", "Price50", "Title" },
                values: new object[] { "Phạm Minh Đức", "Cuốn sách kinh dị mang đến những tràng đêm rùng rợn với những hiện tượng siêu nhiên và bí ẩn chưa lời giải.", "VN0001003", 110m, 100m, 105m, "Bóng Ma Trong Đêm" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Author", "Description", "ISBN", "Price", "Price100", "Price50", "Stock", "Title" },
                values: new object[] { "Lê Hoàng Nam", "Một hành trình xuyên qua không gian và thời gian, khám phá những bí mật của vũ trụ trong cuốn tiểu thuyết khoa học viễn tưởng.", "VN0001004", 130m, 120m, 125m, 5, "Vũ Trụ Huyền Bí" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Author", "Description", "ISBN", "Price", "Price100", "Price50", "Title" },
                values: new object[] { "Ngô Thị Mai", "Tác phẩm văn học đặc sắc khắc họa cuộc sống thường nhật qua lăng kính tinh tế và đầy cảm hứng.", "VN0001005", 90m, 80m, 85m, "Sắc Màu Đời Thường" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Author", "Description", "ISBN", "Price", "Price100", "Price50", "Stock", "Title" },
                values: new object[] { "Vũ Minh Khang", "Cuốn tiểu thuyết khoa học viễn tưởng đưa người đọc vào một thế giới tương lai với công nghệ tiên tiến và những thách thức không ngờ.", "VN0001006", 140m, 130m, 135m, 9, "Hành Trình Tương Lai" });
        }
    }
}
