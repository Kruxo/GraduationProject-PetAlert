using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Models;
using GraduationProject.ViewModels;

namespace GraduationProject.Controllers;

public class FoundController : Controller
{
    private readonly ApplicationDbContext _context;

    public FoundController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new FoundIndexVm
        {
            FoundPets = await _context.FoundPets.ToListAsync(),
            PetTypes = await _context.PetTypes.ToListAsync()
        };
        return View(vm);
    }

    public IActionResult Create()
    {
        var vm = new FoundCreateVm
        {
            FoundPet = new Found(),
            PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> CreateAsync(FoundCreateVm createVm)
    {
        if (ModelState.IsValid)
        {
            _context.FoundPets.Add(createVm.FoundPet);
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

        var foundPet = await _context.FoundPets.FindAsync(id);

        if (foundPet == null)
        {
            return NotFound();
        }

        var viewModel = new FoundEditVm
        {
            FoundPet = foundPet,
            PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> Edit(FoundEditVm foundVm)
    {
        var foundPet = foundVm.FoundPet;

        if (ModelState.IsValid)
        {
            if (!FoundPetExists(foundPet.Id))
            {
                return NotFound();
            }

            _context.Update(foundPet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(foundVm);
    }

    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var foundPet = await _context.FoundPets.FirstOrDefaultAsync(m => m.Id == id);

        if (foundPet == null)
        {
            return NotFound();
        }

        var viewModel = new FoundDeleteVm
        {
            FoundPet = foundPet,
            PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList()
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    //[Authorize(Roles = RoleConstants.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var foundPet = await _context.FoundPets.FindAsync(id);
        if (foundPet == null)
        {
            return NotFound();
        }

        _context.FoundPets.Remove(foundPet);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FoundPetExists(int id)
    {
        return _context.FoundPets.Any(x => x.Id == id);
    }
}
