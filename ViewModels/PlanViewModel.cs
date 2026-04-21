using System.ComponentModel.DataAnnotations;
using FitManager.Models;

namespace FitManager.ViewModels;

public class PlanViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nombre del Plan")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 9999999, ErrorMessage = "El precio debe ser mayor a 0")]
    [Display(Name = "Precio")]
    public decimal Price { get; set; }

    [Required]
    [Display(Name = "Tipo de Plan")]
    public PlanType PlanType { get; set; } = PlanType.Monthly;

    [Required]
    [Range(1, 3650, ErrorMessage = "Duración entre 1 y 3650 días")]
    [Display(Name = "Duración (días)")]
    public int DurationDays { get; set; } = 30;

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    // Stats (for index view)
    public int ActiveSubscriptions { get; set; }
}
