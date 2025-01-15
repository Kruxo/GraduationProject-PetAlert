using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Models;
using GraduationProject.ViewModels;

namespace GraduationProject.Controllers;

public class LostController : Controller
{
    private readonly ApplicationDbContext _context;

    public LostController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new LostIndexVm
        {
            LostPets = await _context.LostPets.ToListAsync(),
            PetTypes = await _context.PetTypes.ToListAsync()
        };
        return View(vm);
    }

    public IActionResult Create()
    {
        var vm = new LostCreateVm
        {
            LostPet = new Lost(),
            PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> CreateAsync(LostCreateVm createVm)
    {
        if (ModelState.IsValid)
        {
            _context.LostPets.Add(createVm.LostPet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        createVm.PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList(); //When validation fails we repopulate category dropdown with values again
        return View(createVm);
    }

    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> EditAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var lostPet = await _context.LostPets.FindAsync(id);

        if (lostPet == null)
        {
            return NotFound();
        }

        var viewModel = new LostEditVm
        {
            LostPet = lostPet,
            PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> Edit(LostEditVm lostVm)
    {
        var lostPet = lostVm.LostPet;

        if (ModelState.IsValid)
        {
            if (!LostPetExists(lostPet.Id))
            {
                return NotFound();
            }

            _context.Update(lostPet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(lostVm);
    }

    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var lostPet = await _context.LostPets.FirstOrDefaultAsync(m => m.Id == id);

        if (lostPet == null)
        {
            return NotFound();
        }

        var viewModel = new LostDeleteVm
        {
            LostPet = lostPet,
            PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList()
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lostPet = await _context.LostPets.FindAsync(id);
        if (lostPet == null)
        {
            return NotFound();
        }

        _context.LostPets.Remove(lostPet);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LostPetExists(int id)
    {
        return _context.LostPets.Any(x => x.Id == id);
    }
}
