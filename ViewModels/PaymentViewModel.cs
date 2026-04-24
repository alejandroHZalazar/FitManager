using System.ComponentModel.DataAnnotations;
using FitManager.Models;

namespace FitManager.ViewModels;

public class PaymentViewModel
{
    public int Id { get; set; }

    [Required]
    public int MemberId { get; set; }

    public string MemberName { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;

    [Display(Name = "Plan")]
    public int? PlanId { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, 999999, ErrorMessage = "Monto inválido")]
    [Display(Name = "Monto")]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Fecha de Pago")]
    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Fecha de Vencimiento")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddMonths(1);

    [Display(Name = "Estado")]
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;

    [Display(Name = "Método de Pago")]
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

    [MaxLength(100)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [MaxLength(500)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    [MaxLength(100)]
    [Display(Name = "Nº Recibo")]
    public string? ReceiptNumber { get; set; }
}

public class PaymentReceiptViewModel
{
    public FitManager.Models.CompanySettings Company { get; set; } = null!;
    public FitManager.Models.Member          Member  { get; set; } = null!;
    public FitManager.Models.Payment         Payment { get; set; } = null!;

    public string MethodLabel => Payment.Method switch
    {
        FitManager.Models.PaymentMethod.Cash     => "Efectivo",
        FitManager.Models.PaymentMethod.Card     => "Tarjeta",
        FitManager.Models.PaymentMethod.Transfer => "Transferencia",
        _                                        => "Otro"
    };

    public string StatusLabel => Payment.Status switch
    {
        FitManager.Models.PaymentStatus.Paid      => "Pagado",
        FitManager.Models.PaymentStatus.Pending   => "Pendiente",
        FitManager.Models.PaymentStatus.Overdue   => "Vencido",
        _                                          => "Cancelado"
    };
}

public class MemberPaymentsViewModel
{
    public MemberViewModel Member { get; set; } = null!;
    public List<Payment> Payments { get; set; } = new();
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public PaymentViewModel NewPayment { get; set; } = new();
}
