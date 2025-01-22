using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetAlert.ViewModels;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace PetAlert.Services
{
    public partial class ChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ApplicationDbContext _context;
        private readonly LocationService _locationService;
        private readonly ILogger<ChatbotService> _logger;

        // Constructor injection for ILogger
        public ChatbotService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context, ILogger<ChatbotService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleAI:ApiKey"]
                    ?? throw new InvalidOperationException("❌ Missing Google AI API Key. Ensure it is set in appsettings.json.");
            _context = context;
            _locationService = new LocationService(httpClient, configuration); // Initialize Location Service
            _logger = logger; // Initialize the logger
        }

        public async Task<string> GetChatbotResponseAsync(string userMessage)
        {
            if (_httpClient == null)
            {
                _logger.LogError("HTTP Client is not initialized.");
                return "❌ Error: HTTP Client is not initialized.";
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("API Key is missing.");
                return "❌ Error: API Key is missing. Please configure it correctly.";
            }

            _logger.LogInformation($"🔍 Received User Message: {userMessage}");

            var queryParameters = await ExtractQueryParametersWithGeminiAi(userMessage);
            if (queryParameters == null)
            {
                _logger.LogWarning("Could not extract query parameters from the user message.");
                return "🤖 I couldn't understand your request. Please try again.";
            }

            // Ensure AI extracted valid values
            queryParameters.PetType ??= "";
            queryParameters.City ??= "";
            queryParameters.Date ??= "";
            queryParameters.Name ??= "";
            queryParameters.ChipId ??= "";

            _logger.LogInformation($"✅ Extracted Parameters: PetType={queryParameters.PetType}, City={queryParameters.City}, Date={queryParameters.Date}, Name={queryParameters.Name}, ChipId={queryParameters.ChipId}");

            var searchResults = await SearchLostPets(queryParameters);

            return !string.IsNullOrEmpty(searchResults) ? searchResults : "🚫 No matching lost pets found.";
        }


        private async Task<QueryParameters?> ExtractQueryParametersWithGeminiAi(string userMessage)
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
                                Query: '{userMessage}'" }
                        }
                    }
                }
            };

            // Convert to JSON
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();

            _logger.LogInformation("📡 Sending API Request to Google Gemini...");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}",
                jsonContent
            );

            // Check response status
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API Request failed with status code: {response.StatusCode}");
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("📡 RAW API RESPONSE FROM GOOGLE GEMINI:");
            _logger.LogDebug(responseBody);

            try
            {
                // Deserialize response
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

                _logger.LogWarning("AI response format was incorrect. No valid parameters found.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AI Parsing Error: {ex.Message}");
                return null;
            }
        }

        private async Task<string> SearchLostPets(QueryParameters parameters)
        {
            try
            {
                _logger.LogInformation($"🔎 Searching for lost {parameters.PetType ?? "pets"} in {parameters.City} (since {parameters.Date})...");

                var query = _context.LostPets
            .Include(l => l.PetType)
            .Include(l => l.User) // Include the User entity
            .AsQueryable();

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
                    _logger.LogInformation("No lost pets found.");
                    return "🚫 No lost pets found.";
                }

                // 🔥 Return a formatted HTML string for chatbot responses
                var response = new StringBuilder();
                string cityName = "";
                foreach (var pet in lostPets)
                {
                    // Convert latitude & longitude safely
                    if (double.TryParse(pet.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude) &&
                        double.TryParse(pet.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
                    {
                        // Fetch city name using OpenStreetMap API
                        cityName = await _locationService.GetCityFromCoordinates(latitude, longitude) ?? "Unknown Location";
                    }
                    else
                    {
                        _logger.LogWarning("Invalid latitude/longitude format for pet: " + pet.Name);
                        cityName = "Unknown Location";
                    }

                    response.Append($@"
    <div class='card mb-2' style='border-radius: 10px; overflow: hidden;'>
        <img src='{pet.Image}' alt='{pet.Name}' style='height: 160px; width: 100%; object-fit: cover;'>
        <div class='p-2'>
            <div class='d-flex align-items-center text-muted mb-2'>
                <i class='fas fa-map-marker-alt me-1'></i> Last seen at {cityName}
            </div>
            <h5 class='fw-bold text-dark d-flex align-items-center'>
                {pet.Name}
                <span class='badge bg-warning text-dark ms-2'>
                    {pet.PetType?.Type ?? "Unknown"}
                </span>
            </h5>

            <div class='text-muted' style='margin-bottom: 5px'>
                <b>Description:</b>
                <div>{pet.Description}</div>
            </div>
            <div class='text-muted' style='margin-bottom: 5px'>
                <div><b>Phone Number:</b>{pet.User?.PhoneNumber ?? "Not Available"}</div>
            </div>
            
            {(pet.ChipId != null ? $@"
            <div class='text-muted' style='margin-bottom: 5px'>
                
                <div><b>Chip ID:</b>{pet.ChipId}</div>
            </div>" : "")}
        </div>
    </div>
");

                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database Query Error: {ex.Message}");
                return "❌ Database Query Error.";
            }
        }
    }
}
