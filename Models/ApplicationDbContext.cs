using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Models
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Found> FoundPets { get; set; }
        public DbSet<Lost> LostPets { get; set; }
        public DbSet<PetType> PetTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Found>()
                .HasOne(f => f.User)
                .WithMany() // Assuming no navigation property on `IdentityUser`
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Define delete behavior
        }
    }
}