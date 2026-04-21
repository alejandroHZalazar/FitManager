using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitManager.Models;

public enum PaymentStatus
{
    Paid = 1,
    Pending = 2,
    Overdue = 3,
    Cancelled = 4
}

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    Transfer = 3,
    Other = 4
}

public class Payment
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

    [MaxLength(100)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? ReceiptNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // ── Plan / Caja ───────────────────────────────────────────────────────────
    public int? PlanId { get; set; }
    public Plan? Plan { get; set; }

    public int? MemberPlanId { get; set; }
    public MemberPlan? MemberPlan { get; set; }

    public int? CashRegisterId { get; set; }
    public CashRegister? CashRegister { get; set; }
}
