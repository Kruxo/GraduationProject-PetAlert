using GraduationProject.Models;

namespace GraduationProject.ViewModels;

public class FoundLostIndexVm
{

    public List<Lost> LostPets { get; set; } = [];
    public List<Found> FoundPets { get; set; } = [];

    // public List<PetType> PetTypes { get; set; } = [];


    public FoundLostIndexVm()
    {
        LostPets = new List<Lost>();
        FoundPets = new List<Found>();
        // PetTypes = new List<PetType>();
    }

}