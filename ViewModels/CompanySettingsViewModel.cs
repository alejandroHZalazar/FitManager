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

    // ── Notificaciones WhatsApp ───────────────────────────────────────────────
    [Display(Name = "Habilitar notificaciones por WhatsApp")]
    public bool WhatsAppEnabled { get; set; }

    [MaxLength(200)]
    [Display(Name = "API Key")]
    public string? WhatsAppApiKey { get; set; }

    [MaxLength(300)]
    [Display(Name = "URL de la API")]
    public string? WhatsAppApiUrl { get; set; }

    /// <summary>Lista de eventos configurados (se carga para mostrar en la UI).</summary>
    public List<WhatsAppEventViewModel> WhatsAppEvents { get; set; } = new();

    // ── Recepción / kiosco ───────────────────────────────────────────────────
    [Range(3, 60, ErrorMessage = "Debe estar entre 3 y 60 segundos")]
    [Display(Name = "Segundos de visualización del modal de recepción")]
    public int ReceptionModalSeconds { get; set; } = 8;
}

public class WhatsAppEventViewModel
{
    public int    Id          { get; set; }
    public string Label       { get; set; } = string.Empty;
    public string EventName   { get; set; } = string.Empty;
    public int    SystemEvent { get; set; } = 1;
    public bool   IsActive    { get; set; } = false;
}
