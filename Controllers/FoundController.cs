using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models;

namespace GraduationProject.Controllers;

public class FoundController : Controller
{
    private readonly ApplicationDbContext _context;

    public FoundController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }
}
