using ElectronicShopMVC.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicShopMVC.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ShoppingCartItem> UserProductShoppingCarts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        private void ProcessSaveInterceptor()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                // Soft delete handling
                if (entry.State == EntityState.Deleted)
                {
                    if (entry.Entity is Product product)
                    {
                        entry.State = EntityState.Modified;
                        product.IsDeleted = true;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is Category category)
                    {
                        entry.State = EntityState.Modified;
                        category.IsDeleted = true;
                        category.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is Voucher voucher)
                    {
                        entry.State = EntityState.Modified;
                        voucher.IsDeleted = true;
                        voucher.UpdatedAt = DateTime.UtcNow;
                    }
                }
                
                // Audit metadata handling
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is Product p)
                    {
                        p.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is Category c)
                    {
                        c.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is Voucher v)
                    {
                        v.CreatedAt = DateTime.UtcNow;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is Product p)
                    {
                        p.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is Category c)
                    {
                        c.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is Voucher v)
                    {
                        v.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ProcessSaveInterceptor();

            var deletedProducts = ChangeTracker.Entries<Product>()
                .Where(e => e.State == EntityState.Deleted);

            foreach (var productEntry in deletedProducts)
            {
                var product = productEntry.Entity;
                var imagePath = product.ImageUrl;
                if (string.IsNullOrEmpty(imagePath))
                {
                    continue;
                }
                DeleteProductImage(imagePath);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ProcessSaveInterceptor();

            var deletedProducts = ChangeTracker.Entries<Product>()
                .Where(e => e.State == EntityState.Deleted);

            foreach (var productEntry in deletedProducts)
            {
                var product = productEntry.Entity;
                var imagePath = product.ImageUrl;
                if (string.IsNullOrEmpty(imagePath))
                {
                    continue;
                }
                DeleteProductImage(imagePath);
            }

            return base.SaveChanges();
        }

        private void DeleteProductImage(string? imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return;
            }
            File.Delete(imagePath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure precise decimal scales for all money columns
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // Production Database Indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            // Global Query Filters for Soft-Deleted Tables
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Voucher>().HasQueryFilter(v => !v.IsDeleted);

            // Category Seed Data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Laptop", DisplayOrder = 1, IsDeleted = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 2, Name = "Điện thoại", DisplayOrder = 2, IsDeleted = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 3, Name = "Linh kiện PC", DisplayOrder = 3, IsDeleted = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 4, Name = "Phụ kiện", DisplayOrder = 4, IsDeleted = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                );

            // Voucher Seed Data
            modelBuilder.Entity<Voucher>().HasData(
                new Voucher
                {
                    Id = 1,
                    Code = "ESHOP10",
                    Title = "Giảm giá 10% tổng hóa đơn",
                    Description = "Ưu đãi chiết khấu 10% cho toàn bộ giá trị giỏ hàng trước thuế.",
                    DiscountType = "Percentage",
                    DiscountValue = 10m,
                    MinOrderAmount = 0m,
                    MaxUses = 1000,
                    UsedCount = 0,
                    StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Voucher
                {
                    Id = 2,
                    Code = "WELCOME50",
                    Title = "Giảm ngay 50.000đ cho đơn hàng",
                    Description = "Chiết khấu 50.000đ trực tiếp vào hóa đơn thanh toán.",
                    DiscountType = "FixedAmount",
                    DiscountValue = 50000m,
                    MinOrderAmount = 100000m,
                    MaxUses = 500,
                    UsedCount = 0,
                    StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Voucher
                {
                    Id = 3,
                    Code = "FREESHIP",
                    Title = "Miễn phí vận chuyển toàn quốc",
                    Description = "Miễn phí 100% phí giao hàng cho tất cả các đơn hàng.",
                    DiscountType = "FreeShipping",
                    DiscountValue = 0m,
                    MinOrderAmount = 0m,
                    MaxUses = 2000,
                    UsedCount = 0,
                    StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<OrderItem>()
                .HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Order>()
                .HasMany(e => e.Items)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .IsRequired();

            modelBuilder.Entity<Product>()
               .HasOne(p => p.Category)
               .WithMany()
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Title = "Laptop Gaming ASUS ROG Strix",
                    Brand = "ASUS",
                    Description = "Cấu hình mạnh mẽ với RTX 4060, Intel Core i7 thế hệ mới nhất, màn hình 165Hz mượt mà.",
                    SKU = "ES-LP-001",
                    Stock = 10,
                    Price = 35000000,
                    Price50 = 34500000,
                    Price100 = 34000000,
                    CategoryId = 1, // Laptop
                    ImageUrl = "",
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 2,
                    Title = "iPhone 15 Pro Max 256GB",
                    Brand = "Apple",
                    Description = "Thiết kế vỏ Titan bền bỉ, chip A17 Pro siêu mạnh, hệ thống camera chuyên nghiệp.",
                    SKU = "ES-PH-002",
                    Stock = 8,
                    Price = 29000000,
                    Price50 = 28500000,
                    Price100 = 28000000,
                    CategoryId = 2, // Điện thoại
                    ImageUrl = "",
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 3,
                    Title = "Card màn hình MSI RTX 4070 Ti",
                    Brand = "MSI",
                    Description = "Hiệu năng đồ họa đỉnh cao cho gaming 2K và 4K, tản nhiệt 3 quạt siêu mát.",
                    SKU = "ES-PC-003",
                    Stock = 12,
                    Price = 22000000,
                    Price50 = 21500000,
                    Price100 = 21000000,
                    CategoryId = 3, // Linh kiện PC
                    ImageUrl = "",
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 4,
                    Title = "Chuột Logitech G Pro X Superlight",
                    Brand = "Logitech",
                    Description = "Trọng lượng siêu nhẹ, cảm biến HERO 25K chính xác vượt trội cho game thủ chuyên nghiệp.",
                    SKU = "ES-AC-004",
                    Stock = 15,
                    Price = 2500000,
                    Price50 = 2400000,
                    Price100 = 2300000,
                    CategoryId = 4, // Phụ kiện
                    ImageUrl = "",
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 5,
                    Title = "Samsung Galaxy S24 Ultra",
                    Brand = "Samsung",
                    Description = "Bút S-Pen tiện lợi, camera 200MP zoom siêu xa, màn hình phẳng độ sáng cực cao.",
                    SKU = "ES-PH-005",
                    Stock = 7,
                    Price = 26000000,
                    Price50 = 25500000,
                    Price100 = 25000000,
                    CategoryId = 2, // Điện thoại
                    ImageUrl = "",
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 6,
                    Title = "Bàn phím Akko 3098B Multi-mode",
                    Brand = "Akko",
                    Description = "Kết nối không dây, switch Akko độc quyền, thiết kế nhỏ gọn đầy đủ phím số.",
                    SKU = "ES-AC-006",
                    Stock = 20,
                    Price = 1800000,
                    Price50 = 1750000,
                    Price100 = 1700000,
                    CategoryId = 4, // Phụ kiện
                    ImageUrl = "",
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
              );
        }
    }
}
