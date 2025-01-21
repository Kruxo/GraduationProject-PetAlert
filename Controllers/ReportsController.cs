using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace GraduationProject.Controllers;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
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