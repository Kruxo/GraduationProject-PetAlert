using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PetAlert.Services
{
    public class LocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public LocationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // ✅ OpenCage API Key (Replace with your own)
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
                string url = $"https://api.opencagedata.com/geocode/v1/json?q={Uri.EscapeDataString(cityName)}&key={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                Console.WriteLine("🔍 API Response: " + response);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ OpenCage API Error: {response.StatusCode}");
                    return null;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);

                if (jsonDocument.RootElement.GetProperty("results").GetArrayLength() > 0)
                {
                    var firstResult = jsonDocument.RootElement.GetProperty("results")[0];
                    double latitude = firstResult.GetProperty("geometry").GetProperty("lat").GetDouble();
                    double longitude = firstResult.GetProperty("geometry").GetProperty("lng").GetDouble();

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

        /// <summary>
        /// Get City Name from Coordinates using OpenCage Reverse Geocoding.
        /// </summary>
        public async Task<string?> GetCityFromCoordinates(double latitude, double longitude)
        {
            try
            {

                string url = $"https://api.opencagedata.com/geocode/v1/json?q={latitude},{longitude}&key={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                Console.WriteLine("🔍 Reverse Geocode Response: " + response);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ OpenCage API Error: {response.StatusCode}");
                    return null;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);

                if (jsonDocument.RootElement.GetProperty("results").GetArrayLength() > 0)
                {
                    var components = jsonDocument.RootElement.GetProperty("results")[0].GetProperty("components");

                    // 🔹 Try multiple possible city fields
                    string city = components.TryGetProperty("city", out var cityValue) ? cityValue.GetString() :
                                  components.TryGetProperty("town", out var townValue) ? townValue.GetString() :
                                  components.TryGetProperty("village", out var villageValue) ? villageValue.GetString() :
                                  "Unknown Location";

                    return city;
                }

                Console.WriteLine("❌ No city found.");
                return "Unknown Location";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching city from coordinates: {ex.Message}");
                return "Unknown Location";
            }
        }
    }
}
