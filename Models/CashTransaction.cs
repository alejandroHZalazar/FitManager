using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitManager.Models;

public enum TransactionType
{
    Income     = 1,
    Expense    = 2,
    Adjustment = 3
}

public class CashTransaction
{
    public int Id { get; set; }

    public int CashRegisterId { get; set; }
    public CashRegister CashRegister { get; set; } = null!;

    public TransactionType Type { get; set; } = TransactionType.Income;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public PaymentMethod? Method { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;
}
