using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using GraduationProject.ViewModels;

namespace GraduationProject.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {

        var list = new FoundLostIndexVm
        {
            LostPets = await _context.LostPets.ToListAsync(),
            FoundPets = await _context.FoundPets.ToListAsync(),
        };

        var lostPetMarkers = await _context.LostPets
     .Include(f => f.PetType) // Include related PetType
     .Include(f => f.User)    // Include related User
     .Select(f => new
     {
         f.Name,
         f.Description,
         f.Image,
         f.Latitude,
         f.Longitude,
         PetType = f.PetType != null ? f.PetType.Type : "Unknown", // Include PetType if available
         PhoneNumber = f.User != null ? f.User.PhoneNumber : "Not Provided", // Include User's PhoneNumber if available
         ChipId = f.ChipId // Specific to Lost pets
     })
     .ToListAsync();

        var foundPetMarkers = await _context.FoundPets
            .Include(f => f.PetType)
            .Include(f => f.User)
            .Select(f => new
            {
                f.Name,
                f.Description,
                f.Image,
                f.Latitude,
                f.Longitude,
                PetType = f.PetType != null ? f.PetType.Type : "Unknown",
                PhoneNumber = f.User != null ? f.User.PhoneNumber : "Not Provided",
                ChipId = (string?)null // Found pets do not have a ChipId, so set it as null
            })
            .ToListAsync();

        var combinedMarkers = lostPetMarkers.Concat(foundPetMarkers).ToList();

        ViewBag.Markers = combinedMarkers;
        return View(list);
    }

}
