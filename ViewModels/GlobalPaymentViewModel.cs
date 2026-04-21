using System.ComponentModel.DataAnnotations;
using FitManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.ViewModels;

public class GlobalPaymentViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un socio")]
    [Display(Name = "Socio")]
    public int MemberId { get; set; }

    public string MemberName { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;

    [Display(Name = "Plan")]
    public int? PlanId { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, 9999999)]
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

    // Dropdowns (populated by controller)
    public IEnumerable<SelectListItem> Members { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Plans { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class PaymentFilterViewModel
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? MemberId { get; set; }
    public int? PlanId { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentMethod? Method { get; set; }
}
