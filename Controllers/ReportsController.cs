using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using PetAlert.Services;

namespace GraduationProject.Controllers;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly LocationService _locationService;

    public ReportsController(ApplicationDbContext context, LocationService locationService)
    {
        _context = context;
        _locationService = locationService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new FoundLostIndexVm
        {
            LostPets = await _context.LostPets.ToListAsync(),
            FoundPets = await _context.FoundPets.ToListAsync(),
            PetTypes = await _context.PetTypes.ToListAsync(),
            Users = await _context.Users.ToListAsync(),
        };

        var allPets = viewModel.LostPets
            .Select(p => new PetDisplayData
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Image = p.Image,
                Date = p.Date,
                Longitude = p.Longitude,
                Latitude = p.Latitude,
                ChipId = p.ChipId,
                ReportType = "Lost",
                PetType = viewModel.PetTypes.FirstOrDefault(pt => pt.Id == p.PetTypeId)?.Type ?? "Unknown",
                PhoneNumber = viewModel.Users.FirstOrDefault(pn => pn.Id == p.UserId)?.PhoneNumber ?? "Not available"
            })
            .Concat(viewModel.FoundPets.Select(p => new PetDisplayData
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Image = p.Image,
                Date = p.Date,
                Longitude = p.Longitude,
                Latitude = p.Latitude,
                ChipId = "Not available",
                ReportType = "Found",
                PetType = viewModel.PetTypes.FirstOrDefault(pt => pt.Id == p.PetTypeId)?.Type ?? "Unknown",
                PhoneNumber = viewModel.Users.FirstOrDefault(pn => pn.Id == p.UserId)?.PhoneNumber ?? "Not available"
            }))
            .OrderByDescending(p => p.Date)
            .ToList();

        // Fetch addresses in parallel
        var tasks = allPets.Select(async pet =>
        {
            string address = "Unknown";

            if (double.TryParse(pet.Latitude, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var latitude) &&
                double.TryParse(pet.Longitude, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var longitude))
            {
                address = await _locationService.GetCityFromCoordinates(latitude, longitude) ?? "Unknown";
            }
            else
            {
                Console.WriteLine($"❌ Invalid coordinates for pet: {pet.Name}. Latitude: {pet.Latitude}, Longitude: {pet.Longitude}");
            }

            pet.Address = address; // Assign the fetched address
        });

        await Task.WhenAll(tasks);

        ViewBag.AllPets = allPets;
        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> YourReports()
    {
        var viewModel = new FoundLostIndexVm
        {
            LostPets = await _context.LostPets.ToListAsync(),
            FoundPets = await _context.FoundPets.ToListAsync(),
            PetTypes = await _context.PetTypes.ToListAsync(),
            Users = await _context.Users.ToListAsync(),
        };

        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> CreateReports()
    {

        var viewModel = new FoundLostIndexVm
        {
            LostPets = await _context.LostPets.ToListAsync(),
            FoundPets = await _context.FoundPets.ToListAsync(),
            PetTypes = await _context.PetTypes.ToListAsync(),
            Users = await _context.Users.ToListAsync(),
        };

        return View(viewModel);
    }
}