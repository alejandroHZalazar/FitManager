using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public enum NutritionGoal
{
    WeightLoss  = 1,
    MassGain    = 2,
    Maintenance = 3,
    Definition  = 4,
    Endurance   = 5,
    Other       = 99
}

public enum MealType
{
    Breakfast     = 1,
    MorningSnack  = 2,
    Lunch         = 3,
    AfternoonSnack = 4,
    Dinner        = 5,
    EveningSnack  = 6
}

// ─────────────────────────────────────────────────────────────────────────────

public class NutritionPlan
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public NutritionGoal Goal { get; set; } = NutritionGoal.WeightLoss;

    /// <summary>Duración en días (7 = semanal, 30 = mensual, etc.)</summary>
    public int DurationDays { get; set; } = 30;

    /// <summary>Recomendaciones generales: hidratación, restricciones, etc.</summary>
    [MaxLength(1000)]
    public string? GeneralNotes { get; set; }

    /// <summary>true = plantilla reutilizable; false = personalizada.</summary>
    public bool IsTemplate { get; set; } = true;

    public bool   IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    public ICollection<NutritionMeal>       Meals       { get; set; } = new List<NutritionMeal>();
    public ICollection<MemberNutritionPlan> Assignments { get; set; } = new List<MemberNutritionPlan>();
}

// ─────────────────────────────────────────────────────────────────────────────

public class NutritionMeal
{
    public int Id { get; set; }

    public int           NutritionPlanId { get; set; }
    public NutritionPlan NutritionPlan   { get; set; } = null!;

    public MealType MealType { get; set; }

    /// <summary>Nombre personalizado (ej: "Colación post-entreno").</summary>
    [MaxLength(100)]
    public string? CustomName { get; set; }

    /// <summary>Descripción del contenido: "Pollo + ensalada + 1 fruta".</summary>
    [MaxLength(2000)]
    public string? Content { get; set; }

    /// <summary>Cantidades aproximadas: "200g pollo, 1 taza arroz".</summary>
    [MaxLength(500)]
    public string? Quantities { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public int OrderIndex { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────

public class MemberNutritionPlan
{
    public int Id { get; set; }

    public int    MemberId { get; set; }
    public Member Member   { get; set; } = null!;

    public int           NutritionPlanId { get; set; }
    public NutritionPlan NutritionPlan   { get; set; } = null!;

    public DateTime  AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate  { get; set; }
    public DateTime? EndDate    { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(256)]
    public string? AssignedBy { get; set; }
}
