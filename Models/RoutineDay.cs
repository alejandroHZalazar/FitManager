using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class RoutineDay
{
    public int Id { get; set; }

    public int RoutineId { get; set; }
    public Routine Routine { get; set; } = null!;

    /// <summary>Número de día (1, 2, 3…)</summary>
    public int DayNumber { get; set; }

    /// <summary>Etiqueta libre, ej: "Pecho / Tríceps"</summary>
    [MaxLength(150)]
    public string? Name { get; set; }

    /// <summary>true = día de descanso (sin ejercicios)</summary>
    public bool IsRestDay { get; set; } = false;

    public int OrderIndex { get; set; }

    public ICollection<RoutineExercise> Exercises { get; set; } = new List<RoutineExercise>();
}
