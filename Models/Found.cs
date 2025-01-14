using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Models;

public class Found
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public string Latitude { get; set; } = "";
    public string Longitude { get; set; } = "";
    public string UserId { get; set; } = "";

    [ForeignKey("PetType")]
    public int? PetTypeId { get; set; } // Foreign key
    public PetType? PetType { get; set; } // Navigation property

}
