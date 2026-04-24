using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class RoutineExercise
{
    public int Id { get; set; }

    public int RoutineDayId { get; set; }
    public RoutineDay RoutineDay { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    /// <summary>Número de series</summary>
    public int Sets { get; set; } = 3;

    /// <summary>Repeticiones — puede ser "10", "8-12", "al fallo", "30 seg"</summary>
    [MaxLength(30)]
    public string Reps { get; set; } = "10";

    /// <summary>Peso en kg (opcional)</summary>
    public decimal? Weight { get; set; }

    /// <summary>Descanso entre series en segundos</summary>
    public int? RestSeconds { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }

    public int OrderIndex { get; set; }
}
