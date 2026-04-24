using System.ComponentModel.DataAnnotations;

namespace FitManager.ViewModels;

public class ClassCategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [MaxLength(7)]
    [Display(Name = "Color")]
    public string Color { get; set; } = "#6c757d";

    [MaxLength(60)]
    [Display(Name = "Ícono (Font Awesome)")]
    public string Icon { get; set; } = "fa-dumbbell";

    [Display(Name = "Orden")]
    public int OrderIndex { get; set; } = 99;

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    // Read-only, for display in list
    public int ClassCount { get; set; }
}
