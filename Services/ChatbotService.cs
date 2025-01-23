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
        public ChatbotService(
        HttpClient httpClient,
        IConfiguration configuration,
        ApplicationDbContext context,
        ILogger<ChatbotService> logger,
        LocationService locationService) // Inject LocationService properly
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["GoogleAI:ApiKey"]
                      ?? throw new InvalidOperationException("❌ Missing Google AI API Key. Ensure it is set in appsettings.json.");
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService)); // ✅ Ensure LocationService is injected
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // ✅ Ensure logger is not null
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
            _logger.LogInformation($"🌍 Extracted City from AI: '{queryParameters.City}'");

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
                                Extract structured data from this query and return **ONLY** JSON. No text explanation.
                                - Convert pet type to singular (e.g., 'dogs' → 'dog').
                                - If the user requests 'all pets', set 'PetType' to empty.
                                - If a specific name or ChipId is mentioned, extract it.
                                Return JSON ONLY:
                                {{""PetType"": """", ""City"": """", ""Date"": """", ""Name"": """", ""ChipId"": """"}}
                                Query: '{userMessage}'
                            " }
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
                _logger.LogInformation($"🔎 Searching for lost {parameters.PetType ?? "pets"} in {parameters.City}...");

                var query = _context.LostPets
                    .Include(l => l.PetType)
                    .Include(l => l.User)
                    .AsQueryable();

                bool searchAllPets = string.IsNullOrWhiteSpace(parameters.PetType) || parameters.PetType.ToLower() == "all";

                if (!searchAllPets)
                {
                    string safePetType = parameters.PetType.Trim().ToLower();
                    query = query.Where(l => l.PetType != null && l.PetType.Type.ToLower().Contains(safePetType));
                }

                if (!string.IsNullOrWhiteSpace(parameters.Name))
                {
                    string safeName = parameters.Name.Trim().ToLower();
                    query = query.Where(l => l.Name.ToLower().Contains(safeName));
                }

                if (!string.IsNullOrWhiteSpace(parameters.ChipId))
                {
                    string safeChipId = parameters.ChipId.Trim();
                    query = query.Where(l => l.ChipId == safeChipId);
                }

                var lostPets = await query.ToListAsync();
                if (!lostPets.Any())
                {
                    _logger.LogInformation("🚫 No lost pets found.");
                    return "🚫 No lost pets found.";
                }

                Dictionary<(double, double), string> cityCache = new();
                foreach (var pet in lostPets)
                {
                    if (double.TryParse(pet.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude) &&
                        double.TryParse(pet.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
                    {
                        if (!cityCache.TryGetValue((latitude, longitude), out string cityName))
                        {
                            cityName = await _locationService.GetCityFromCoordinates(latitude, longitude) ?? "Unknown";
                            cityCache[(latitude, longitude)] = cityName;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(parameters.City))
                {
                    _logger.LogInformation($"🌍 Expanding search around: {parameters.City}");

                    string normalizedCityQuery = parameters.City.Trim().ToLower();
                    List<Lost> filteredLostPets = new();

                    foreach (var pet in lostPets)
                    {
                        if (double.TryParse(pet.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                            double.TryParse(pet.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                        {
                            if (cityCache.TryGetValue((lat, lon), out string cityName))
                            {
                                if (cityName.ToLower().Contains(normalizedCityQuery) ||
                                    await _locationService.IsNearbyCity(cityName, normalizedCityQuery)) // ✅ Uses new method
                                {
                                    filteredLostPets.Add(pet);
                                }
                            }
                        }
                    }

                    lostPets = filteredLostPets;
                }

                _logger.LogInformation($"✅ Found {lostPets.Count} matching pets.");
                return FormatLostPets(lostPets, cityCache);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Database Query Error: {ex.Message}");
                return "❌ Database Query Error.";
            }
        }



        private string FormatLostPets(List<Lost> lostPets, Dictionary<(double, double), string> cityCache)
        {
            var response = new StringBuilder();
            foreach (var pet in lostPets)
            {
                string cityName = "Unknown Location";

                if (double.TryParse(pet.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude) &&
                    double.TryParse(pet.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
                {
                    cityName = cityCache.GetValueOrDefault((latitude, longitude), "Unknown Location");
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
                        <div><b>Phone Number:</b> {pet.User?.PhoneNumber ?? "Not Available"}</div>
                    </div>
                    {(pet.ChipId != null ? $@"
                    <div class='text-muted' style='margin-bottom: 5px'>
                        <div><b>Chip ID:</b> {pet.ChipId}</div>
                    </div>" : "")}
                </div>
            </div>
        ");
            }

            return response.ToString();
        }







    }
}
