using System.ComponentModel.DataAnnotations;

namespace FitManager.ViewModels;

public class RoleViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nombre del Rol")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    public bool IsSystem { get; set; }
    public int UserCount { get; set; }
}

public class MenuPermissionViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<MenuItemPermissionRow> Items { get; set; } = new();
}

public class MenuItemPermissionRow
{
    public int MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public bool IsEnabled { get; set; }
}
