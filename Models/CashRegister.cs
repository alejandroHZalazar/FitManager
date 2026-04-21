using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitManager.Models;

public enum CashRegisterStatus
{
    Open   = 1,
    Closed = 2
}

public class CashRegister
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;

    [Column(TypeName = "decimal(10,2)")]
    public decimal OpeningBalance { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? ClosingBalance { get; set; }

    public CashRegisterStatus Status { get; set; } = CashRegisterStatus.Open;

    [MaxLength(256)]
    public string OpenedBy { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? ClosedBy { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // ── Reapertura ────────────────────────────────────────────────────────────
    public bool WasReopened { get; set; } = false;

    [MaxLength(256)]
    public string? ReopenedBy { get; set; }

    public DateTime? ReopenedAt { get; set; }

    [MaxLength(500)]
    public string? ReopenNotes { get; set; }

    public ICollection<CashTransaction> Transactions { get; set; } = new List<CashTransaction>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // ── Computed ─────────────────────────────────────────────────────────────
    public decimal TotalIncome    => Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
    public decimal TotalExpense   => Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    public decimal PaymentsTotal  => Payments.Sum(p => p.Amount);
    public decimal GrossIncome    => PaymentsTotal + TotalIncome;
    public decimal NetBalance     => OpeningBalance + GrossIncome - TotalExpense;
}
