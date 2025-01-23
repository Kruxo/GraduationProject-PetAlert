using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PetAlert.Services
{
    public class LocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<LocationService> _logger;

        public LocationService(HttpClient httpClient, IConfiguration configuration, ILogger<LocationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiKey = configuration["OpenCage:ApiKey"] 
                      ?? throw new InvalidOperationException("❌ Missing OpenCage API Key. Ensure it is set in appsettings.json.");
        }

        /// <summary>
        /// Get Coordinates (Latitude, Longitude) from City Name using OpenCage Geocoder API.
        /// </summary>
        public async Task<(double Latitude, double Longitude)?> GetCoordinatesFromCity(string cityName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cityName))
                {
                    _logger.LogWarning("❌ City name is empty or null.");
                    return null;
                }

                string url = $"https://api.opencagedata.com/geocode/v1/json?q={Uri.EscapeDataString(cityName)}&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                
                _logger.LogInformation($"🌍 Geocoding request sent: {url}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ OpenCage API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"📡 RAW API RESPONSE:\n{responseBody}");

                using var jsonDocument = JsonDocument.Parse(responseBody);

                // Check API rate limits
                if (jsonDocument.RootElement.TryGetProperty("rate", out var rate) &&
                    rate.TryGetProperty("remaining", out var remaining))
                {
                    _logger.LogInformation($"🔄 API Requests Remaining: {remaining.GetInt32()}");
                }

                if (jsonDocument.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];

                    if (firstResult.TryGetProperty("geometry", out var geometry) &&
                        geometry.TryGetProperty("lat", out var latValue) &&
                        geometry.TryGetProperty("lng", out var lngValue))
                    {
                        double latitude = latValue.GetDouble();
                        double longitude = lngValue.GetDouble();

                        _logger.LogInformation($"📍 Coordinates for {cityName}: ({latitude}, {longitude})");
                        return (latitude, longitude);
                    }
                }

                _logger.LogWarning($"❌ No coordinates found for city: {cityName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error fetching coordinates for city {cityName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get City Name from Coordinates using OpenCage Reverse Geocoding.
        /// </summary>
        public async Task<string> GetCityFromCoordinates(double latitude, double longitude)
        {
            try
            {
                string url = $"https://api.opencagedata.com/geocode/v1/json?q={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                
                _logger.LogInformation($"🔍 Reverse Geocode Request: {url}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ OpenCage API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return "Unknown Location";
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"📡 RAW API RESPONSE:\n{responseBody}");

                using var jsonDocument = JsonDocument.Parse(responseBody);

                if (jsonDocument.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var components = results[0].GetProperty("components");

                    // ✅ Prefer actual city names first
                    if (components.TryGetProperty("city", out var cityValue))
                        return cityValue.GetString();

                    if (components.TryGetProperty("town", out var townValue))
                        return townValue.GetString();

                    if (components.TryGetProperty("village", out var villageValue))
                        return villageValue.GetString();

                    // 🚨 If city is missing, fallback to county
                    if (components.TryGetProperty("county", out var countyValue))
                        return countyValue.GetString();
                }

                _logger.LogWarning($"❌ No city found for coordinates ({latitude}, {longitude}).");
                return "Unknown Location";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error fetching city from coordinates ({latitude}, {longitude}): {ex.Message}");
                return "Unknown Location";
            }
        }

        /// <summary>
        /// Check if a city is considered "near" a main city (Stockholm region, Gothenburg region, etc.).
        /// </summary>
        public async Task<bool> IsNearbyCity(string cityName, string mainCity, double maxDistanceKm = 50)
        {
            var mainCityCoords = await GetCoordinatesFromCity(mainCity);
            var cityCoords = await GetCoordinatesFromCity(cityName);

            if (mainCityCoords == null || cityCoords == null)
                return false; // Cannot compare if no coordinates

            double distance = HaversineDistance(mainCityCoords.Value.Latitude, mainCityCoords.Value.Longitude,
                                                cityCoords.Value.Latitude, cityCoords.Value.Longitude);

            return distance <= maxDistanceKm; // ✅ Expands search to 50km radius
        }

        /// <summary>
        /// Calculates distance between two coordinates using the Haversine formula.
        /// </summary>
        private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            double dLat = (lat2 - lat1) * (Math.PI / 180);
            double dLon = (lon2 - lon1) * (Math.PI / 180);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // ✅ Returns distance in kilometers
        }
    }
}
