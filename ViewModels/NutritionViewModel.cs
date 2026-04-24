using System.ComponentModel.DataAnnotations;
using FitManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.ViewModels;

// ── Sub-form por comida ───────────────────────────────────────────────────────

public class NutritionMealFormVM
{
    public int      Id        { get; set; }
    public MealType MealType  { get; set; }
    public bool     IsEnabled { get; set; }

    [MaxLength(100)]
    public string? CustomName { get; set; }

    [MaxLength(2000)]
    public string? Content { get; set; }

    [MaxLength(500)]
    public string? Quantities { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Helper de solo lectura para la vista
    public string DefaultLabel => MealType switch
    {
        MealType.Breakfast      => "Desayuno",
        MealType.MorningSnack   => "Colación Mañana",
        MealType.Lunch          => "Almuerzo",
        MealType.AfternoonSnack => "Merienda",
        MealType.Dinner         => "Cena",
        MealType.EveningSnack   => "Colación Noche",
        _                       => "Comida"
    };

    public string Icon => MealType switch
    {
        MealType.Breakfast      => "fas fa-sun",
        MealType.MorningSnack   => "fas fa-apple-alt",
        MealType.Lunch          => "fas fa-utensils",
        MealType.AfternoonSnack => "fas fa-coffee",
        MealType.Dinner         => "fas fa-moon",
        MealType.EveningSnack   => "fas fa-cookie",
        _                       => "fas fa-bowl-food"
    };

    public bool IsOptional => MealType is MealType.MorningSnack or MealType.EveningSnack;
}

// ── Create / Edit ─────────────────────────────────────────────────────────────

public class NutritionPlanEditViewModel
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
    public NutritionGoal Goal { get; set; } = NutritionGoal.WeightLoss;

    [Range(1, 365)]
    [Display(Name = "Duración (días)")]
    public int DurationDays { get; set; } = 30;

    [MaxLength(1000)]
    [Display(Name = "Notas / Recomendaciones Generales")]
    public string? GeneralNotes { get; set; }

    [Display(Name = "Plantilla reutilizable")]
    public bool IsTemplate { get; set; } = true;

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    /// <summary>Siempre 6 elementos, uno por MealType.</summary>
    public List<NutritionMealFormVM> Meals { get; set; } = CreateDefaultMeals();

    public static List<NutritionMealFormVM> CreateDefaultMeals() =>
    [
        new() { MealType = MealType.Breakfast,      IsEnabled = true  },
        new() { MealType = MealType.MorningSnack,   IsEnabled = false },
        new() { MealType = MealType.Lunch,          IsEnabled = true  },
        new() { MealType = MealType.AfternoonSnack, IsEnabled = true  },
        new() { MealType = MealType.Dinner,         IsEnabled = true  },
        new() { MealType = MealType.EveningSnack,   IsEnabled = false },
    ];
}

// ── Lista ─────────────────────────────────────────────────────────────────────

public class NutritionPlanListViewModel
{
    public int    Id              { get; set; }
    public string Name            { get; set; } = string.Empty;
    public string GoalLabel       { get; set; } = string.Empty;
    public int    DurationDays    { get; set; }
    public string DurationLabel   { get; set; } = string.Empty;
    public int    MealCount       { get; set; }
    public int    AssignedCount   { get; set; }
    public bool   IsTemplate      { get; set; }
    public bool   IsActive        { get; set; }
}

// ── Detalle ───────────────────────────────────────────────────────────────────

public class NutritionPlanDetailViewModel
{
    public NutritionPlan NutritionPlan { get; set; } = null!;
    public string        GoalLabel     { get; set; } = string.Empty;
    public string        DurationLabel { get; set; } = string.Empty;
}

// ── Asignación a socios ───────────────────────────────────────────────────────

public class AssignNutritionViewModel
{
    [Required(ErrorMessage = "Seleccioná un socio")]
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Seleccioná un plan")]
    public int NutritionPlanId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime? StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de fin")]
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    // Not submitted
    public List<SelectListItem> PlanOptions { get; set; } = new();
}

public class MemberNutritionListViewModel
{
    public int       Id               { get; set; }
    public int       MemberId         { get; set; }
    public string    MemberName       { get; set; } = string.Empty;
    public string    MemberNumber     { get; set; } = string.Empty;
    public int       NutritionPlanId  { get; set; }
    public string    PlanName         { get; set; } = string.Empty;
    public string    GoalLabel        { get; set; } = string.Empty;
    public DateTime  AssignedAt       { get; set; }
    public DateTime? StartDate        { get; set; }
    public DateTime? EndDate          { get; set; }
    public bool      IsActive         { get; set; }
    public string?   Notes            { get; set; }
}

// ── Impresión ─────────────────────────────────────────────────────────────────

public class NutritionPrintViewModel
{
    public CompanySettings      Company    { get; set; } = null!;
    public Member               Member     { get; set; } = null!;
    public MemberNutritionPlan  Assignment { get; set; } = null!;
    public NutritionPlan        Plan       { get; set; } = null!;
    public string               GoalLabel  { get; set; } = string.Empty;
    public string               DurationLabel { get; set; } = string.Empty;
}
