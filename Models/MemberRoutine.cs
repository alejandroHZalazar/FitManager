using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class MemberRoutine
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int RoutineId { get; set; }
    public Routine Routine { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime StartDate { get; set; } = DateTime.Today;

    public DateTime? EndDate { get; set; }

    /// <summary>Solo puede haber una rutina activa por socio a la vez</summary>
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(256)]
    public string? AssignedBy { get; set; }
}
