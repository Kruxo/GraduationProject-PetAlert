using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Models;
using GraduationProject.ViewModels;

namespace GraduationProject.Controllers;
[Authorize(Roles = RoleConstants.Administrator)] //With this only Admin User can access this page
public class PetTypeController : Controller
{
    private readonly ApplicationDbContext _context;

    public PetTypeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var petType = await _context.PetTypes.ToListAsync();
        var viewModel = new PetTypeIndexVm
        {
            PetTypes = petType
        };
        return View(viewModel);
    }

    public IActionResult Create()
    {
        return View(new PetTypeCreateVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PetTypeCreateVm createVm)
    {
        if (ModelState.IsValid)
        {
            var petType = new PetType
            {
                Type = createVm.Type,
                Image = createVm.Image,
            };
            _context.Add(petType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(createVm);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var petType = await _context.PetTypes.FindAsync(id);

        if (petType == null)
        {
            return NotFound();
        }

        var viewModel = new PetTypeEditVm
        {
            Id = petType.Id,
            Type = petType.Type,
            Image = petType.Image,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PetTypeEditVm editVm)
    {
        if (ModelState.IsValid)
        {
            var petType = await _context.PetTypes.FindAsync(editVm.Id);

            if (petType == null)
            {
                return NotFound();
            }

            petType.Type = editVm.Type;
            petType.Image = editVm.Image;

            _context.Update(petType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(editVm);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var petType = await _context.PetTypes.FindAsync(id);

        if (petType == null)
        {
            return NotFound();
        }

        var viewModel = new PetTypeDeleteVm
        {
            Id = petType.Id,
            Type = petType.Type,
            Image = petType.Image,
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var petType = await _context.PetTypes.FindAsync(id);

        if (petType == null)
        {
            return NotFound();
        }

        _context.PetTypes.Remove(petType);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}



