using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class RoleMenuPermission
{
    public int Id { get; set; }

    [Required]
    public string RoleId { get; set; } = string.Empty;
    public ApplicationRole Role { get; set; } = null!;

    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public bool IsEnabled { get; set; } = true;
}
