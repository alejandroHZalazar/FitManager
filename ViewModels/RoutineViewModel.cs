using System.ComponentModel.DataAnnotations;
using FitManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.ViewModels;

// ─────────────────────────────────────────────────────────────────────────────
// Ejercicios
// ─────────────────────────────────────────────────────────────────────────────

public class ExerciseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(120)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Grupo Muscular")]
    public MuscleGroup MuscleGroup { get; set; } = MuscleGroup.Other;

    [Display(Name = "Tipo")]
    public ExerciseType ExerciseType { get; set; } = ExerciseType.Strength;

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;
}

public class ExerciseListViewModel
{
    public int          Id           { get; set; }
    public string       Name         { get; set; } = string.Empty;
    public string       MuscleGroup  { get; set; } = string.Empty;
    public string       ExerciseType { get; set; } = string.Empty;
    public MuscleGroup  MuscleGroupEnum  { get; set; }
    public ExerciseType ExerciseTypeEnum { get; set; }
    public bool         IsCustom     { get; set; }
    public bool         IsActive     { get; set; }
    public int          UsageCount   { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Sub-forms (días y ejercicios dentro del formulario de rutina)
// ─────────────────────────────────────────────────────────────────────────────

public class RoutineExerciseFormVM
{
    public int      Id          { get; set; }
    public int      ExerciseId  { get; set; }
    public int      Sets        { get; set; } = 3;
    public string   Reps        { get; set; } = "10";
    public decimal? Weight      { get; set; }
    public int?     RestSeconds { get; set; }
    public string?  Notes       { get; set; }
    public int      OrderIndex  { get; set; }
}

public class RoutineDayFormVM
{
    public int      Id         { get; set; }
    public int      DayNumber  { get; set; }
    public string?  Name       { get; set; }
    public bool     IsRestDay  { get; set; }
    public List<RoutineExerciseFormVM> Exercises { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
// Create / Edit
// ─────────────────────────────────────────────────────────────────────────────

public class RoutineEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Objetivo")]
    public RoutineGoal Goal { get; set; } = RoutineGoal.Other;

    [Range(1, 52)]
    [Display(Name = "Duración (semanas)")]
    public int DurationWeeks { get; set; } = 4;

    [Range(1, 7)]
    [Display(Name = "Frecuencia semanal")]
    public int FrequencyPerWeek { get; set; } = 3;

    [Display(Name = "Tipo")]
    public bool IsGeneral { get; set; } = true;

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    public List<RoutineDayFormVM> Days { get; set; } = new();

    // Populated by controller — not submitted
    public List<ExerciseSelectItem> ExerciseOptions { get; set; } = new();
}

/// <summary>Ítem liviano para llenar el select de ejercicios en el form.</summary>
public class ExerciseSelectItem
{
    public int    Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
// Lista
// ─────────────────────────────────────────────────────────────────────────────

public class RoutineListViewModel
{
    public int    Id               { get; set; }
    public string Name             { get; set; } = string.Empty;
    public string GoalLabel        { get; set; } = string.Empty;
    public int    DurationWeeks    { get; set; }
    public int    FrequencyPerWeek { get; set; }
    public bool   IsGeneral        { get; set; }
    public int    DayCount         { get; set; }
    public int    AssignedCount    { get; set; }
    public bool   IsActive         { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Detalle (vista de la rutina con días y ejercicios)
// ─────────────────────────────────────────────────────────────────────────────

public class RoutineDetailViewModel
{
    public Routine Routine { get; set; } = null!;
    public string  GoalLabel { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
// Asignación a socios
// ─────────────────────────────────────────────────────────────────────────────

public class AssignRoutineViewModel
{
    [Required(ErrorMessage = "Seleccioná un socio")]
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Seleccioná una rutina")]
    public int RoutineId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de fin")]
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    // Not submitted — populated by controller
    public List<SelectListItem> RoutineOptions { get; set; } = new();
}

public class MemberRoutineListViewModel
{
    public int      Id           { get; set; }
    public int      MemberId     { get; set; }
    public string   MemberName   { get; set; } = string.Empty;
    public string   MemberNumber { get; set; } = string.Empty;
    public int      RoutineId    { get; set; }
    public string   RoutineName  { get; set; } = string.Empty;
    public string   GoalLabel    { get; set; } = string.Empty;
    public DateTime AssignedAt   { get; set; }
    public DateTime StartDate    { get; set; }
    public DateTime? EndDate     { get; set; }
    public bool     IsActive     { get; set; }
    public string?  Notes        { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Impresión
// ─────────────────────────────────────────────────────────────────────────────

public class RoutinePrintViewModel
{
    public FitManager.Models.CompanySettings Company    { get; set; } = null!;
    public Member       Member     { get; set; } = null!;
    public MemberRoutine Assignment { get; set; } = null!;
    public Routine      Routine    { get; set; } = null!;
    public string       GoalLabel  { get; set; } = string.Empty;
}
