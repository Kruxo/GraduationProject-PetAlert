using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace GraduationProject.Controllers;

public class LostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LostController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var lostPets = await _context.LostPets
.Include(f => f.User)
.ToListAsync();

        var vm = new LostIndexVm
        {
            LostPets = lostPets,
            PetTypes = await _context.PetTypes.ToListAsync()
        };
        return View(vm);
    }

    [Authorize]
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
    public async Task<IActionResult> CreateAsync(LostCreateVm createVm)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User); // Get the logged-in user's Id
            createVm.LostPet.UserId = userId; // Set the UserId field

            _context.LostPets.Add(createVm.LostPet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        createVm.PetTypes = _context.PetTypes.OrderBy(c => c.Type).ToList(); //When validation fails we repopulate category dropdown with values again
        return View(createVm);
    }

    [Authorize]
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
    public async Task<IActionResult> Edit(LostEditVm lostVm)
    {
        var lostPet = lostVm.LostPet;

        if (ModelState.IsValid)
        {
            if (!LostPetExists(lostPet.Id))
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User); // Get the logged-in user's Id
            lostVm.LostPet.UserId = userId; // Set the UserId field
            _context.Update(lostPet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(lostVm);
    }

    [Authorize]
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
