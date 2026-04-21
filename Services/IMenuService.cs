using FitManager.Models;

namespace FitManager.Services;

public interface IMenuService
{
    Task<List<MenuItem>> GetMenuForRolesAsync(IEnumerable<string> roles);
}
