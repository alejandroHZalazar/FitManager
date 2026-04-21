using System.ComponentModel.DataAnnotations;
using FitManager.Models;

namespace FitManager.ViewModels;

public class CashRegisterDetailViewModel
{
    public CashRegister Register { get; set; } = null!;

    // Payments of the day grouped by method
    public List<Payment> Payments { get; set; } = new();
    public decimal PaymentsCash     { get; set; }
    public decimal PaymentsCard     { get; set; }
    public decimal PaymentsTransfer { get; set; }
    public decimal PaymentsOther    { get; set; }
    public decimal PaymentsTotal    { get; set; }

    // Manual transactions
    public List<CashTransaction> Transactions { get; set; } = new();
    public decimal ManualIncome  { get; set; }
    public decimal ManualExpense { get; set; }

    // Totals
    public decimal GrossIncome  { get; set; }
    public decimal NetBalance   { get; set; }
    public decimal Discrepancy  { get; set; }   // ClosingBalance - NetBalance

    // For adding manual transaction
    public CashTransactionFormViewModel NewTransaction { get; set; } = new();
}

public class CashTransactionFormViewModel
{
    public int CashRegisterId { get; set; }

    [Required]
    [Display(Name = "Tipo")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required]
    [Range(0.01, 9999999)]
    [Display(Name = "Monto")]
    public decimal Amount { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Método")]
    public PaymentMethod? Method { get; set; }

    [MaxLength(300)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }
}

public class OpenCashRegisterViewModel
{
    [Display(Name = "Saldo Inicial (efectivo en caja)")]
    [Range(0, 9999999)]
    public decimal OpeningBalance { get; set; } = 0;

    [MaxLength(500)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }
}

public class CloseCashRegisterViewModel
{
    public int RegisterId { get; set; }
    public decimal CalculatedBalance { get; set; }

    [Display(Name = "Saldo Final Real (conteo físico)")]
    [Range(0, 9999999)]
    public decimal ClosingBalance { get; set; }

    [MaxLength(500)]
    [Display(Name = "Notas de Cierre")]
    public string? Notes { get; set; }
}

public class ReopenCashRegisterViewModel
{
    public int RegisterId { get; set; }

    [Required(ErrorMessage = "Ingresá el motivo de la reapertura")]
    [MaxLength(500)]
    [Display(Name = "Motivo de la Reapertura")]
    public string Reason { get; set; } = string.Empty;
}
