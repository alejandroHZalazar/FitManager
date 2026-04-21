using FitManager.Data;
using FitManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class MenuService : IMenuService
{
    private readonly ApplicationDbContext _db;

    public MenuService(ApplicationDbContext db) => _db = db;

    public async Task<List<MenuItem>> GetMenuForRolesAsync(IEnumerable<string> roles)
    {
        var roleList = roles.ToList();

        // Get allowed menu item IDs for these roles
        var allowedIds = await _db.RoleMenuPermissions
            .Where(p => roleList.Contains(p.Role.Name!) && p.IsEnabled)
            .Select(p => p.MenuItemId)
            .Distinct()
            .ToListAsync();

        // Load parent items with their allowed children
        var items = await _db.MenuItems
            .Where(m => m.IsActive && allowedIds.Contains(m.Id))
            .Include(m => m.Children.Where(c => c.IsActive && allowedIds.Contains(c.Id)))
            .Where(m => m.ParentId == null)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync();

        // Sort children
        foreach (var item in items)
            item.Children = item.Children.OrderBy(c => c.OrderIndex).ToList();

        return items;
    }
}
