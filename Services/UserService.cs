using System.Security.Claims;
using GraduationProject.Models;
using Microsoft.AspNetCore.Identity;

namespace GraduationProject.Services;

public class UserService(UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
{
    public async Task<bool> IsCurrentUserAdministratorAsync()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }
        return await userManager.IsInRoleAsync(user, RoleConstants.Administrator);
    }
}
