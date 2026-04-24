using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public enum MuscleGroup
{
    Chest       = 1,   // Pecho
    Back        = 2,   // Espalda
    Shoulders   = 3,   // Hombros
    Biceps      = 4,
    Triceps     = 5,
    Quadriceps  = 7,   // Cuádriceps
    Hamstrings  = 8,   // Femorales
    Calves      = 9,   // Pantorrillas
    Glutes      = 10,  // Glúteos
    Abs         = 11,  // Abdominales
    Cardio      = 12,
    FullBody    = 13,  // Cuerpo Completo
    Other       = 99
}

public enum ExerciseType
{
    Strength    = 1,  // Fuerza
    Cardio      = 2,
    Flexibility = 3,  // Flexibilidad
    Balance     = 4,  // Equilibrio
    Functional  = 5,  // Funcional
    Other       = 99
}

public class Exercise
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public MuscleGroup MuscleGroup { get; set; } = MuscleGroup.Other;

    public ExerciseType ExerciseType { get; set; } = ExerciseType.Strength;

    /// <summary>false = predefinido por el sistema, true = creado por el usuario</summary>
    public bool IsCustom { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
}
