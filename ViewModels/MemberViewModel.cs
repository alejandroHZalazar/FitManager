using System.ComponentModel.DataAnnotations;
using FitManager.Models;
using Microsoft.AspNetCore.Http;

namespace FitManager.ViewModels;

public class MemberViewModel
{
    public int Id { get; set; }

    [Display(Name = "Nº Socio")]
    public string MemberNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(200)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(30)]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    [MaxLength(300)]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [Display(Name = "Fecha de Nacimiento")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    [Display(Name = "DNI / Documento")]
    public string? DNI { get; set; }

    [Display(Name = "Estado")]
    public MemberStatus Status { get; set; } = MemberStatus.Active;

    [Display(Name = "Inicio de Membresía")]
    [DataType(DataType.Date)]
    public DateTime MembershipStartDate { get; set; } = DateTime.Today;

    [Display(Name = "Fin de Membresía")]
    [DataType(DataType.Date)]
    public DateTime? MembershipEndDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    public string? PhotoPath { get; set; }

    [Display(Name = "Foto")]
    public IFormFile? PhotoFile { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
