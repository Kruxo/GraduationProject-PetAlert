using GraduationProject.Models;

namespace GraduationProject.ViewModels;

public class PetTypeIndexVm
{
    public List<PetType> PetTypes { get; set; } = [];
}

public class PetTypeCreateVm
{
    public string Type { get; set; } = "";
    public string Image { get; set; } = "";
}

public class PetTypeDeleteVm
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Image { get; set; } = "";
}

public class PetTypeEditVm
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Image { get; set; } = "";
}