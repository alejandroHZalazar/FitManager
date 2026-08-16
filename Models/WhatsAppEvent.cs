using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

/// <summary>
/// Tipo de evento del sistema. Definido por el desarrollador, no configurable por el usuario.
/// Determina el comportamiento interno al dispararse.
/// </summary>
public enum WhatsAppSystemEvent
{
    /// <summary>Confirmación de pago — se dispara al registrar un pago.</summary>
    PaymentConfirmation = 1,

    /// <summary>Aviso de vencimiento — se dispara junto al anterior; la API externa gestiona el horario.</summary>
    DueDateReminder = 2,

    /// <summary>Aviso de deuda — se dispara manualmente desde la pantalla de vencimientos ya ocurridos.</summary>
    DebtNotice = 3
}

/// <summary>
/// Evento de WhatsApp pre-definido por el desarrollador.
/// El usuario solo puede activar/desactivar y cambiar el nombre del evento en la plataforma.
/// </summary>
public class WhatsAppEvent
{
    public int Id { get; set; }

    /// <summary>Siempre 1 (singleton CompanySettings).</summary>
    public int CompanySettingsId { get; set; } = 1;

    /// <summary>Nombre descriptivo para mostrar en la UI — definido por el desarrollador.</summary>
    [Required, MaxLength(100)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Nombre del evento en la plataforma WhatsApp — configurable por el usuario.</summary>
    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>Tipo de evento del sistema. Determina el comportamiento interno.</summary>
    public WhatsAppSystemEvent SystemEvent { get; set; } = WhatsAppSystemEvent.PaymentConfirmation;

    public bool IsActive { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CompanySettings? Company { get; set; }
}
