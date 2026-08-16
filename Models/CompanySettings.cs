using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

/// <summary>
/// Configuración global de la empresa — singleton (siempre Id = 1).
/// </summary>
public class CompanySettings
{
    public int Id { get; set; } = 1;

    [Required, MaxLength(150)]
    public string Name { get; set; } = "FitManager";

    [MaxLength(200)]
    public string? Slogan { get; set; }

    /// <summary>Ruta relativa dentro de wwwroot, ej: uploads/company/logo.png</summary>
    [MaxLength(300)]
    public string? LogoPath { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>CUIT / RUT / NIF o equivalente</summary>
    [MaxLength(30)]
    public string? TaxId { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(30)]
    public string? Phone2 { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(150)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Notificaciones WhatsApp ───────────────────────────────────────────────
    /// <summary>Activa/desactiva todas las notificaciones WhatsApp de la empresa.</summary>
    public bool WhatsAppEnabled { get; set; } = false;

    /// <summary>API Key de la plataforma (ej. nf_xxxxxxxxxxxx).</summary>
    [MaxLength(200)]
    public string? WhatsAppApiKey { get; set; }

    /// <summary>URL base de la API (ej. https://localhost:7275).</summary>
    [MaxLength(300)]
    public string? WhatsAppApiUrl { get; set; }

    /// <summary>Eventos configurados — cada uno tiene su propio disparador y nombre de evento.</summary>
    public ICollection<WhatsAppEvent> WhatsAppEvents { get; set; } = new List<WhatsAppEvent>();

    // ── Recepción / kiosco ───────────────────────────────────────────────────
    /// <summary>Segundos que permanece visible el modal de bienvenida en /Reception.</summary>
    [Range(3, 60)]
    public int ReceptionModalSeconds { get; set; } = 8;
}
