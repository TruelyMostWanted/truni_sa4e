using Microsoft.EntityFrameworkCore;

namespace XmasWishes.Models.persons;

public class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; } // Tabelle "Person" in der DB
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Person>();

        entity.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        entity.Property(p => p.Surname)
            .HasMaxLength(100)
            .IsRequired();
    }
}
