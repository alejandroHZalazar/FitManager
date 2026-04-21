using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public enum MemberStatus
{
    Active = 1,
    Inactive = 0
}

public class Member
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string MemberNumber { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? DNI { get; set; }

    [MaxLength(500)]
    public string? PhotoPath { get; set; }

    public MemberStatus Status { get; set; } = MemberStatus.Active;

    public DateTime MembershipStartDate { get; set; } = DateTime.Today;
    public DateTime? MembershipEndDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
