using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models;

public class PetType
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Image { get; set; } = "";
}
