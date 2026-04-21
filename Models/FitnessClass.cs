using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitManager.Models;

public enum ClassCategory
{
    Spinning    = 1,
    Yoga        = 2,
    Pilates     = 3,
    Zumba       = 4,
    CrossFit    = 5,
    Functional  = 6,
    Kickboxing  = 7,
    Natacion    = 8,
    Musculacion = 9,
    Other       = 99
}

public class FitnessClass
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ClassCategory Category { get; set; } = ClassCategory.Other;

    [Required, MaxLength(150)]
    public string InstructorName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Location { get; set; }

    /// <summary>Color hex para el calendario (ej: #ff6b35)</summary>
    [MaxLength(7)]
    public string Color { get; set; } = "#ff6b35";

    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }

    /// <summary>null = cupos ilimitados</summary>
    public int? MaxCapacity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string? CreatedBy { get; set; }

    public ICollection<ClassSchedule>  Schedules   { get; set; } = new List<ClassSchedule>();
    public ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();

    /// <summary>Cantidad de inscriptos activos</summary>
    [NotMapped]
    public int ActiveEnrollments => Enrollments.Count(e => e.Status == EnrollmentStatus.Active);

    /// <summary>True si hay cupo disponible o si es ilimitado</summary>
    [NotMapped]
    public bool HasCapacity => !MaxCapacity.HasValue || ActiveEnrollments < MaxCapacity.Value;
}
