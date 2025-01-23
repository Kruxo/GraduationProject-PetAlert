public class PetDisplayData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public DateTime Date { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public string Address { get; set; } = "Loading..."; // Default value
    public string ChipId { get; set; }
    public string ReportType { get; set; }
    public string PetType { get; set; }
    public string PhoneNumber { get; set; }
}