using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FitManager.ViewModels;

public class CompanySettingsViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nombre de la empresa")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Slogan / Lema")]
    public string? Slogan { get; set; }

    [MaxLength(250)]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Display(Name = "Ciudad")]
    public string? City { get; set; }

    [MaxLength(100)]
    [Display(Name = "Provincia / Estado")]
    public string? Province { get; set; }

    [MaxLength(100)]
    [Display(Name = "País")]
    public string? Country { get; set; }

    [MaxLength(30)]
    [Display(Name = "CUIT / RUT / NIF")]
    public string? TaxId { get; set; }

    [MaxLength(30)]
    [Display(Name = "Teléfono principal")]
    public string? Phone { get; set; }

    [MaxLength(30)]
    [Display(Name = "Teléfono alternativo")]
    public string? Phone2 { get; set; }

    [MaxLength(150)]
    [EmailAddress(ErrorMessage = "Ingresá un email válido")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(150)]
    [Display(Name = "Sitio web")]
    public string? Website { get; set; }

    [MaxLength(500)]
    [Display(Name = "Notas internas")]
    public string? Notes { get; set; }

    // Upload field — not persisted directly
    [Display(Name = "Logotipo")]
    public IFormFile? LogoFile { get; set; }

    // Populated from DB for display
    public string? CurrentLogoPath { get; set; }
}
