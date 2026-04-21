using Microsoft.AspNetCore.Identity;

namespace FitManager.Models;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public bool IsSystem { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RoleMenuPermission> MenuPermissions { get; set; } = new List<RoleMenuPermission>();
}
