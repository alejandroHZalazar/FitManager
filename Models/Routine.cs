using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public enum RoutineGoal
{
    MassGain       = 1,   // Ganar Masa Muscular
    WeightLoss     = 2,   // Bajar de Peso
    Definition     = 3,   // Definición
    Strength       = 4,   // Fuerza
    Endurance      = 5,   // Resistencia
    Rehabilitation = 6,   // Rehabilitación
    Maintenance    = 7,   // Mantenimiento
    Flexibility    = 8,   // Flexibilidad
    Other          = 99
}

public class Routine
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public RoutineGoal Goal { get; set; } = RoutineGoal.Other;

    /// <summary>Duración sugerida en semanas</summary>
    public int DurationWeeks { get; set; } = 4;

    /// <summary>Días de entrenamiento por semana</summary>
    public int FrequencyPerWeek { get; set; } = 3;

    /// <summary>true = plantilla general; false = asignada a un socio específico</summary>
    public bool IsGeneral { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    public ICollection<RoutineDay>    Days           { get; set; } = new List<RoutineDay>();
    public ICollection<MemberRoutine> MemberRoutines { get; set; } = new List<MemberRoutine>();
}
