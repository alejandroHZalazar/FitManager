using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public enum EnrollmentStatus
{
    Active     = 1,
    Cancelled  = 2,
    Waitlisted = 3
}

public class ClassEnrollment
{
    public int Id { get; set; }

    public int FitnessClassId { get; set; }
    public FitnessClass FitnessClass { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(256)]
    public string? EnrolledBy { get; set; }
}
