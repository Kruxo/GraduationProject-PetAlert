namespace PetAlert.Services
{
    public partial class ChatbotService
    {
        private class QueryParameters
        {
            public string? Name { get; set; }
            public string? PetType { get; set; }
            public string? City { get; set; }   
            public string? ChipId { get; set; } 
            public string? Date { get; set; }
            public string? Latitude { get; set; }
            public string? Longitude { get; set; }
        }
    }
}
