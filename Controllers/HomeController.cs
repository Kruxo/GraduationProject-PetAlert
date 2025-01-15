using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

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
    
    return View();
}


    public async Task<IActionResult> Privacy()
    {
        var markers = await _context.FoundPets
        .Include(f => f.PetType) // Include related PetType
        .Select(f => new
        {
            f.Name,
            f.Description,
            f.Image,
            f.Latitude,
            f.Longitude,
            PetType = f.PetType != null ? f.PetType.Type : "Unknown", // Include PetType if available
        })
        .ToListAsync();

    ViewBag.Markers = markers;
        return View();


    }

    

}
