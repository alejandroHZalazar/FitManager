using FitManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

[Authorize(Roles = "Administrador")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var u in users)
            userRoles[u.Id] = await _userManager.GetRolesAsync(u);
        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roleManager.Roles.Select(r => new SelectListItem(r.Name, r.Name)).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string firstName, string lastName, string userName,
        string email, string password, string role)
    {
        var user = new ApplicationUser
        {
            UserName = userName, Email = email,
            FirstName = firstName, LastName = lastName,
            IsActive = true, EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(role)) await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e.Description);

        ViewBag.Roles = await _roleManager.Roles.Select(r => new SelectListItem(r.Name, r.Name)).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = $"Usuario {(user.IsActive ? "activado" : "desactivado")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _userManager.ResetPasswordAsync(user, token, newPassword);
        TempData["Success"] = "Contraseña actualizada.";
        return RedirectToAction(nameof(Index));
    }
}
