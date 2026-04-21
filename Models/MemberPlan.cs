using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public enum MemberPlanStatus
{
    Active    = 1,
    Expired   = 2,
    Cancelled = 3
}

public class MemberPlan
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; }

    public MemberPlanStatus Status { get; set; } = MemberPlanStatus.Active;

    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Computed ─────────────────────────────────────────────────────────────
    public int DaysUntilExpiry => (EndDate.Date - DateTime.Today).Days;
    public bool IsExpired => DateTime.Today > EndDate.Date;
    public bool IsExpiringSoon => DaysUntilExpiry >= 0 && DaysUntilExpiry <= 5;
}
