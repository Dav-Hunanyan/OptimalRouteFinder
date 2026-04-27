using Microsoft.EntityFrameworkCore;
using OptimalRouteFinder.Data.Entities;

namespace OptimalRouteFinder.Data
{
    public class MapDbContext : DbContext
    {
        public DbSet<Template> Templates { get; set; }
        public DbSet<CityEntity> Cities { get; set; }
        public DbSet<RoadEntity> Roads { get; set; }
        public DbSet<User> Users { get; set; }

        public MapDbContext(DbContextOptions<MapDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Template
            modelBuilder.Entity<User>()
       .HasIndex(u => u.Username)
       .IsUnique();

            // -----------------------------
            // Template
            // -----------------------------
            modelBuilder.Entity<Template>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // Link Template → User
            modelBuilder.Entity<Template>()
                .HasOne(t => t.User)
                .WithMany(u => u.Templates)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // if user deleted, templates deleted

            // -----------------------------
            // City
            // -----------------------------
            modelBuilder.Entity<CityEntity>()
                .HasIndex(c => new { c.TemplateId, c.Name })
                .IsUnique();

            modelBuilder.Entity<CityEntity>()
                .HasOne(c => c.Template)
                .WithMany(t => t.Cities)
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------
            // Road
            // -----------------------------
            modelBuilder.Entity<RoadEntity>()
                .HasOne(r => r.Template)
                .WithMany(t => t.Roads)
                .HasForeignKey(r => r.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoadEntity>()
                .HasOne(r => r.FromCity)
                .WithMany()
                .HasForeignKey(r => r.FromCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoadEntity>()
                .HasOne(r => r.ToCity)
                .WithMany()
                .HasForeignKey(r => r.ToCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoadEntity>()
                .HasIndex(r => new { r.TemplateId, r.FromCityId, r.ToCityId })
                .IsUnique();
        }
    }
}
