using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PetAlert.ViewModels;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace PetAlert.Services
{
    public class ChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ApplicationDbContext _context;
        private readonly LocationService _locationService;

        public ChatbotService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleAI:ApiKey"]
                    ?? throw new InvalidOperationException("❌ Missing Google AI API Key. Ensure it is set in appsettings.json.");
            _context = context;
            _locationService = new LocationService(httpClient); // Initialize Location Service
        }

       public async Task<string> GetChatbotResponseAsync(string userMessage)
{
    if (_httpClient == null)
    {
        return "❌ Error: HTTP Client is not initialized.";
    }

    if (string.IsNullOrWhiteSpace(_apiKey))
    {
        return "❌ Error: API Key is missing. Please configure it correctly.";
    }

    Console.WriteLine($"🔑 Using API Key: {_apiKey}");

    var queryParameters = await ExtractQueryParameters(userMessage);
    if (queryParameters == null)
    {
        return "🤖 I couldn't understand your request. Please try again.";
    }

    // Ensure AI extracted valid values
    queryParameters.PetType ??= "";
    queryParameters.City ??= "";
    queryParameters.Date ??= "";
    queryParameters.Name ??= "";
    queryParameters.ChipId ??= "";

    Console.WriteLine($"✅ Extracted Parameters: PetType={queryParameters.PetType}, City={queryParameters.City}, Date={queryParameters.Date}, Name={queryParameters.Name}, ChipId={queryParameters.ChipId}");

    var searchResults = await SearchLostPets(queryParameters);

    return !string.IsNullOrEmpty(searchResults) ? searchResults : "🚫 No matching lost pets found.";
}


        private async Task<QueryParameters?> ExtractQueryParameters(string userMessage)
        {
            var requestBody = new
            {
                model = "gemini-pro",
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = $@"
                                Extract structured data from this query. 
                                - Convert pet type to singular (e.g., 'dogs' → 'dog'). 
                                - If the user requests 'all pets', set 'PetType' to empty to match any pet. 
                                - If a specific name or ChipId is mentioned, extract and include it as optional filters.
                                Return JSON format: 
                                {{""PetType"": """", ""City"": """", ""Date"": """", ""Name"": """", ""ChipId"": """"}}. 
                                Query: '{userMessage}'"}

                        }
                    }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}",
                jsonContent
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("📡 RAW API RESPONSE FROM GOOGLE GEMINI:");
            Console.WriteLine(responseBody);

            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

                if (result.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var extractedJson = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return JsonSerializer.Deserialize<QueryParameters>(extractedJson);
                }

                Console.WriteLine("❌ AI response format was incorrect. No valid parameters found.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AI Parsing Error: {ex.Message}");
                return null;
            }
        }

private async Task<string> SearchLostPets(QueryParameters parameters)
{
    try
    {
        Console.WriteLine($"🔎 Searching for lost {parameters.PetType ?? "pets"} in {parameters.City} (since {parameters.Date})...");

        var query = _context.LostPets.Include(l => l.PetType).AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.PetType) && parameters.PetType.ToLower() != "all")
        {
            string petTypeSingular = parameters.PetType.ToLower();
            query = query.Where(l => l.PetType != null && l.PetType.Type.ToLower().Contains(petTypeSingular));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            query = query.Where(l => l.Name.ToLower().Contains(parameters.Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(parameters.ChipId))
        {
            query = query.Where(l => l.ChipId == parameters.ChipId);
        }

        var lostPets = await query.ToListAsync();

        if (!lostPets.Any())
        {
            return "🚫 No lost pets found.";
        }

        // 🔥 Return a formatted HTML string for chatbot responses
        var response = new StringBuilder();
        foreach (var pet in lostPets)
        {
            response.Append($@"
                <div class='card mb-2' style='border-radius: 10px; overflow: hidden;'>
                    <img src='{pet.Image}' alt='{pet.Name}' style='height: 160px; width: 100%; object-fit: cover;'>
                    <div class='p-2'>
                        <div class='d-flex align-items-center text-muted small mb-2'>
                            <i class='fas fa-map-marker-alt me-1'></i> Last seen at {pet.Latitude}, {pet.Longitude}
                        </div>
                        <h5 class='fw-bold text-dark'>{pet.Name}</h5>
                        <div class='d-flex align-items-center'>
                            <span class='badge bg-warning text-dark'>
                                <i class='fas fa-paw'></i> {pet.PetType?.Type ?? "Unknown"}
                            </span>
                        </div>
                        <p class='text-muted mb-2'>{pet.Description}</p>
                        {(string.IsNullOrWhiteSpace(pet.ChipId) ? "" : $"<p class='text-muted mb-2'><b>Chip ID:</b> {pet.ChipId}</p>")}
                    </div>
                </div>
            ");
        }

        return response.ToString();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database Query Error: {ex.Message}");
        return "❌ Database Query Error.";
    }
}



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

    public class LocationService
    {
        private readonly HttpClient _httpClient;

        public LocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(double Latitude, double Longitude)?> GetCoordinatesFromCity(string cityName)
        {
            try
            {
                string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(cityName)}";

                var response = await _httpClient.GetAsync(url);
                Console.WriteLine("respone--- " + response);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API Error: {response.StatusCode}");
                    return null;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<List<JsonElement>>(responseBody);

                if (results != null && results.Count > 0)
                {
                    var firstResult = results[0];
                    double latitude = double.Parse(firstResult.GetProperty("lat").GetString());
                    double longitude = double.Parse(firstResult.GetProperty("lon").GetString());

                    return (latitude, longitude);
                }

                Console.WriteLine("❌ No coordinates found.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching coordinates: {ex.Message}");
                return null;
            }
        }
    }
}
