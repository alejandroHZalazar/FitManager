using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitManager.Models;

public enum PlanType
{
    Daily   = 1,
    Weekly  = 2,
    Monthly = 3,
    Custom  = 4
}

public class Plan
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public PlanType PlanType { get; set; } = PlanType.Monthly;

    /// <summary>Duration in days (1=diario, 7=semanal, 30=mensual, custom)</summary>
    public int DurationDays { get; set; } = 30;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MemberPlan> MemberPlans { get; set; } = new List<MemberPlan>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
