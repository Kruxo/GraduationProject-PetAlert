using GraduationProject.Models;

namespace GraduationProject.ViewModels;

public class LostIndexVm
{
    public List<Lost> LostPets { get; set; } = [];
    public List<PetType> PetTypes { get; set; } = [];
}

public class LostEditVm
{
    public required Lost LostPet { get; set; }
    public List<PetType> PetTypes { get; set; } = [];
}

public class LostDeleteVm
{
    public required Lost LostPet { get; set; }
    public List<PetType> PetTypes { get; set; } = [];
}

public class LostCreateVm
{
    public required Lost LostPet { get; set; }
    public List<PetType> PetTypes { get; set; } = [];
}