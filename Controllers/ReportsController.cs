using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
using GraduationProject.ViewModels;

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

        var list = new FoundLostIndexVm
        {
            LostPets = await _context.LostPets.ToListAsync(),
            FoundPets = await _context.FoundPets.ToListAsync(),
            // PetTypes = await _context.PetTypes.ToListAsync(),

        };

        var LostPets = await _context.LostPets
      .Select(lp => new
      {
          Name = lp.Name,
          Description = lp.Description,
          Image = lp.Image,
          Id = lp.Id
      })
      .ToListAsync();

        var FoundPets = await _context.FoundPets
      .Select(fp => new
      {
          Name = fp.Name,
          Description = fp.Description,
          Image = fp.Image,
          Id = fp.Id
      })
      .ToListAsync();



        var AllPets = LostPets.Concat(FoundPets).OrderByDescending(p => p.Id).ToList();


        return View(list);
    }
}