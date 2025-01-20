using GraduationProject.Models;
using Microsoft.AspNetCore.Identity;

namespace GraduationProject.ViewModels;

public class FoundLostIndexVm
{
    public List<Lost> LostPets { get; set; } = [];
    public List<Found> FoundPets { get; set; } = [];
    public List<PetType> PetTypes { get; set; } = [];
    public List<IdentityUser> Users { get; set; } = [];
}