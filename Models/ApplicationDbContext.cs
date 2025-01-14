
namespace GraduationProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Found> FoundPets { get; set; }
    public DbSet<Lost> LostPets { get; set; }
    public DbSet<PetType> PetTypes { get; set; }
}

