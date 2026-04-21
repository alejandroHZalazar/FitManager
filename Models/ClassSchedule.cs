using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

/// <summary>
/// Tipo de repetición del horario.
/// </summary>
public enum ScheduleType
{
    Weekly   = 1,   // Cada semana en los días marcados  (ej: todos los Lun/Mié/Vie)
    Biweekly = 2,   // Cada dos semanas en los días marcados
    Monthly  = 3,   // Un día fijo del mes (ej: el día 15 de cada mes)
    OneTime  = 4    // Fecha única / clase suelta
}

/// <summary>
/// Máscara de bits para los días de la semana seleccionados.
/// </summary>
[Flags]
public enum DaysOfWeekFlags
{
    None      = 0,
    Sunday    = 1,
    Monday    = 2,
    Tuesday   = 4,
    Wednesday = 8,
    Thursday  = 16,
    Friday    = 32,
    Saturday  = 64
}

public class ClassSchedule
{
    public int Id { get; set; }

    public int FitnessClassId { get; set; }
    public FitnessClass FitnessClass { get; set; } = null!;

    public ScheduleType ScheduleType { get; set; } = ScheduleType.Weekly;

    // ── Weekly / Biweekly ────────────────────────────────────────────────────
    public DaysOfWeekFlags DaysOfWeek { get; set; } = DaysOfWeekFlags.None;

    // ── Monthly ──────────────────────────────────────────────────────────────
    /// <summary>Día del mes (1-31) para ScheduleType.Monthly</summary>
    public int? DayOfMonth { get; set; }

    // ── OneTime ──────────────────────────────────────────────────────────────
    /// <summary>Fecha puntual para ScheduleType.OneTime</summary>
    public DateTime? SpecificDate { get; set; }

    // ── Horario ──────────────────────────────────────────────────────────────
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime   { get; set; }

    // ── Vigencia ─────────────────────────────────────────────────────────────
    /// <summary>Desde cuándo aplica este horario</summary>
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;

    /// <summary>Hasta cuándo aplica (null = sin vencimiento)</summary>
    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(300)]
    public string? Notes { get; set; }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Descripción legible del horario para mostrar en la UI.</summary>
    public string Description
    {
        get
        {
            var time = $"{StartTime:hh\\:mm} – {EndTime:hh\\:mm}";
            return ScheduleType switch
            {
                ScheduleType.Weekly   => $"{DaysDescription()} · {time}",
                ScheduleType.Biweekly => $"{DaysDescription()} (quincenal) · {time}",
                ScheduleType.Monthly  => $"Día {DayOfMonth} de cada mes · {time}",
                ScheduleType.OneTime  => $"{SpecificDate:dd/MM/yyyy} · {time}",
                _ => time
            };
        }
    }

    private string DaysDescription()
    {
        var parts = new List<string>();
        if ((DaysOfWeek & DaysOfWeekFlags.Monday)    != 0) parts.Add("Lun");
        if ((DaysOfWeek & DaysOfWeekFlags.Tuesday)   != 0) parts.Add("Mar");
        if ((DaysOfWeek & DaysOfWeekFlags.Wednesday) != 0) parts.Add("Mié");
        if ((DaysOfWeek & DaysOfWeekFlags.Thursday)  != 0) parts.Add("Jue");
        if ((DaysOfWeek & DaysOfWeekFlags.Friday)    != 0) parts.Add("Vie");
        if ((DaysOfWeek & DaysOfWeekFlags.Saturday)  != 0) parts.Add("Sáb");
        if ((DaysOfWeek & DaysOfWeekFlags.Sunday)    != 0) parts.Add("Dom");
        return string.Join("/", parts);
    }
}
