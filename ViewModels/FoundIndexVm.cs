using GraduationProject.Models;

namespace GraduationProject.ViewModels;

public class FoundIndexVm
{
    public List<Found> FoundPets { get; set; } = [];
    public List<PetType> PetTypes { get; set; } = [];
}

public class FoundEditVm
{
    public required Found FoundPet { get; set; }
    public List<PetType> PetTypes { get; set; } = [];
}

public class FoundDeleteVm
{
    public required Found FoundPet { get; set; }
    public List<PetType> PetTypes { get; set; } = [];
}

public class FoundCreateVm
{
    public required Found FoundPet { get; set; }
    public List<PetType> PetTypes { get; set; } = [];
}