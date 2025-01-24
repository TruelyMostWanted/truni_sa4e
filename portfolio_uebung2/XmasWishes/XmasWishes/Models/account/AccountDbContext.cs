using Microsoft.EntityFrameworkCore;

namespace XmasWishes.Models.account;

public class AccountDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Account>();
        
        entity.Property(a => a.Username)
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(a => a.Email)
            .HasMaxLength(64);

        entity.Property(a => a.Password)
            .HasMaxLength(256);

        entity.Property(a => a.Name)
            .HasMaxLength(64);
    }
}