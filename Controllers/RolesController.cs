using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

[Authorize(Roles = "Administrador")]
public class RolesController : Controller
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public RolesController(RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
    }

    // GET: /Roles
    public async Task<IActionResult> Index()
    {
        var roles = await _db.Roles.ToListAsync();
        var vmList = new List<RoleViewModel>();
        foreach (var role in roles)
        {
            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            vmList.Add(new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                IsSystem = role.IsSystem,
                UserCount = users.Count
            });
        }
        return View(vmList);
    }

    // GET: /Roles/Create
    public IActionResult Create() => View(new RoleViewModel());

    // POST: /Roles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (await _roleManager.RoleExistsAsync(vm.Name))
        {
            ModelState.AddModelError(nameof(vm.Name), "Ya existe un rol con ese nombre.");
            return View(vm);
        }
        await _roleManager.CreateAsync(new ApplicationRole { Name = vm.Name, Description = vm.Description });
        TempData["Success"] = "Rol creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Roles/Edit/id
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();
        return View(new RoleViewModel { Id = role.Id, Name = role.Name!, Description = role.Description, IsSystem = role.IsSystem });
    }

    // POST: /Roles/Edit/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var role = await _roleManager.FindByIdAsync(vm.Id!);
        if (role == null) return NotFound();
        if (role.IsSystem)
        {
            TempData["Error"] = "No se puede editar un rol del sistema.";
            return RedirectToAction(nameof(Index));
        }
        role.Name = vm.Name;
        role.Description = vm.Description;
        await _roleManager.UpdateAsync(role);
        TempData["Success"] = "Rol actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Roles/Delete/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();
        if (role.IsSystem)
        {
            TempData["Error"] = "No se puede eliminar un rol del sistema.";
            return RedirectToAction(nameof(Index));
        }
        await _roleManager.DeleteAsync(role);
        TempData["Success"] = "Rol eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Roles/MenuPermissions/id
    public async Task<IActionResult> MenuPermissions(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var allItems = await _db.MenuItems.OrderBy(m => m.ParentId).ThenBy(m => m.OrderIndex).ToListAsync();
        var permissions = await _db.RoleMenuPermissions
            .Where(p => p.RoleId == id)
            .ToDictionaryAsync(p => p.MenuItemId, p => p.IsEnabled);

        var rows = allItems.Select(item => new MenuItemPermissionRow
        {
            MenuItemId = item.Id,
            Name = item.Name,
            Icon = item.Icon,
            ParentId = item.ParentId,
            ParentName = allItems.FirstOrDefault(p => p.Id == item.ParentId)?.Name,
            IsEnabled = permissions.TryGetValue(item.Id, out var enabled) && enabled
        }).ToList();

        var vm = new MenuPermissionViewModel
        {
            RoleId = id,
            RoleName = role.Name!,
            Items = rows
        };

        return View(vm);
    }

    // POST: /Roles/SaveMenuPermissions
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMenuPermissions(string roleId, int[] enabledItems)
    {
        var existing = await _db.RoleMenuPermissions.Where(p => p.RoleId == roleId).ToListAsync();
        _db.RoleMenuPermissions.RemoveRange(existing);

        var allItems = await _db.MenuItems.Select(m => m.Id).ToListAsync();
        var newPermissions = allItems.Select(id => new RoleMenuPermission
        {
            RoleId = roleId,
            MenuItemId = id,
            IsEnabled = enabledItems.Contains(id)
        });

        _db.RoleMenuPermissions.AddRange(newPermissions);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Permisos de menú actualizados.";
        return RedirectToAction(nameof(MenuPermissions), new { id = roleId });
    }
}
