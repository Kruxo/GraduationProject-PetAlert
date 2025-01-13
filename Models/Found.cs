using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models;

public class Found
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public string Latitude { get; set; } = "";
    public string Longitude { get; set; } = "";
    public string User_Id { get; set; } = "";
    public Pet_Type? Pet_Type_Id { get; set; }

}
