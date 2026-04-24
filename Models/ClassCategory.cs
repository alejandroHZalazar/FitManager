using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class ClassCategory
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>Color hex identificador de la categoría (ej: #ff6b35)</summary>
    [MaxLength(7)]
    public string Color { get; set; } = "#6c757d";

    /// <summary>Icono FontAwesome (ej: fa-bicycle). Sin el prefijo "fas ".</summary>
    [MaxLength(60)]
    public string Icon { get; set; } = "fa-dumbbell";

    public int OrderIndex { get; set; } = 99;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FitnessClass> Classes { get; set; } = new List<FitnessClass>();
}
