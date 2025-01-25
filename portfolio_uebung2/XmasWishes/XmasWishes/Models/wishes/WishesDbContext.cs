using Microsoft.EntityFrameworkCore;

namespace XmasWishes.Models.wishes
{
    public class WishesDbContext : DbContext
    {
        public WishesDbContext(DbContextOptions<WishesDbContext> options) : base(options) { }

        public DbSet<Wish> Wishes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Wish>();

            entity.Property(w => w.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(w => w.FileName)
                .HasMaxLength(100);

            entity.Property(w => w.Status)
                .IsRequired();
        }
    }
}