using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GraduationProject.Models;

public class Found
{
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name needs to have 2-50 characters")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Description cannot be empty")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Max length is 200 characters")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Please provide an Image URL")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "The URL needs to have 10-200 characters")]
    public string Image { get; set; } = "";

    [Required(ErrorMessage = "Please provide latitude coordinates.")]
    public string Latitude { get; set; } = "";

    [Required(ErrorMessage = "Please provide longitude coordinates.")]
    public string Longitude { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual IdentityUser? User { get; set; }
    public string UserId { get; set; } = "";

    [Required(ErrorMessage = "Please pick an animal")]
    [ForeignKey("PetType")]
    public int? PetTypeId { get; set; } // Foreign key
    public PetType? PetType { get; set; } // Navigation property

}
