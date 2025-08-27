using BarnManagementAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementAPI.Data
{
    public class BarnDbContext : DbContext
    {
        public BarnDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Barns> Barns => Set<Barns>();
        public DbSet<Animal> Animals => Set<Animal>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<User>(e =>
            {
                e.Property(x => x.Username).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.Username).IsUnique();
                e.Property(x => x.Role).IsRequired().HasMaxLength(20);
            });
            mb.Entity<Animal>()
            .HasOne(a => a.Barn)
            .WithMany(b => b.Animals)
            .HasForeignKey(a => a.BarnId)
            .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Product>(e =>
            {
                e.Property(x => x.ProductType).IsRequired().HasMaxLength(50);
                e.Property(x => x.Quantity).HasPrecision(18, 3);
                e.HasOne(x => x.Animal).WithMany().HasForeignKey(x => x.AnimaId).OnDelete(DeleteBehavior.Cascade);
            });
          
            mb.Entity<Transaction>()
                 .HasOne(x => x.Barn)
                 .WithMany()
                 .HasForeignKey(x => x.BarnId)
                 .OnDelete(DeleteBehavior.Cascade);
            
            mb.Entity<Barns>()
               .HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

