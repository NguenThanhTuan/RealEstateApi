using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Models;
using System.Xml;

namespace RealEstateApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users => Set<User>();
        //public DbSet<RealEstates> RealEstates => Set<RealEstates>();
        public DbSet<RealEstates> RealEstates { get; set; }
        public DbSet<RealEstateImages> RealEstateImages { get; set; }
        //public DbSet<RealEstateImages> RealEstateImages => Set<RealEstateImages>();
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<NotificationRead> NotificationReads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình khóa chính cho RealEstateImages nếu cần
            modelBuilder.Entity<RealEstateImages>().HasKey(x => x.imageId); // hoặc x.imageId nếu bạn đặt tên là vậy

            // Cấu hình quan hệ với RealEstates
            modelBuilder.Entity<RealEstateImages>()
                .HasOne(img => img.realEstate)
                .WithMany(re => re.images)
                .HasForeignKey(img => img.realEstateId);

            // Cấu hình khóa chính cho các bảng khác
            modelBuilder.Entity<Province>().HasKey(p => p.provinceId);
            modelBuilder.Entity<District>().HasKey(d => d.districtId);
            modelBuilder.Entity<Ward>().HasKey(w => w.wardId);
            modelBuilder.Entity<Notifications>().HasKey(w => w.notificationId);
            modelBuilder.Entity<NotificationRead>().HasKey(w => w.id);
            modelBuilder.Entity<NotificationRead>()
                .HasOne(nr => nr.Notifications)
                .WithMany(n => n.NotificationReads)
                .HasForeignKey(nr => nr.notificationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificationRead>()
                .HasOne(nr => nr.User)
                .WithMany(u => u.NotificationReads)
                .HasForeignKey(nr => nr.userId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<District>()
            //.HasOne(d => d.province)
            //.WithMany(p => p.districts)
            //.HasForeignKey(d => d.provinceId);

            //modelBuilder.Entity<Ward>()
            //.HasOne(w => w.district)
            //.WithMany(d => d.wards)
            //.HasForeignKey(w => w.districtId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
