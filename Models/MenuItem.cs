using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Icon { get; set; }

    [MaxLength(200)]
    public string? Url { get; set; }

    [MaxLength(50)]
    public string? Controller { get; set; }

    [MaxLength(50)]
    public string? Action { get; set; }

    public int? ParentId { get; set; }
    public MenuItem? Parent { get; set; }

    public int OrderIndex { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
    public ICollection<RoleMenuPermission> RolePermissions { get; set; } = new List<RoleMenuPermission>();
}
