using System.ComponentModel.DataAnnotations;
using FitManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.ViewModels;

// ── Horario individual ────────────────────────────────────────────────────────

public class ClassScheduleViewModel
{
    public int Id { get; set; }

    [Display(Name = "Tipo de repetición")]
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Weekly;

    // ── Días de la semana (Weekly / Biweekly) ─────────────────────────────────
    public bool Monday    { get; set; }
    public bool Tuesday   { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday  { get; set; }
    public bool Friday    { get; set; }
    public bool Saturday  { get; set; }
    public bool Sunday    { get; set; }

    // ── Monthly ───────────────────────────────────────────────────────────────
    [Range(1, 31, ErrorMessage = "Ingrese un día entre 1 y 31")]
    [Display(Name = "Día del mes")]
    public int? DayOfMonth { get; set; }

    // ── OneTime ───────────────────────────────────────────────────────────────
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime? SpecificDate { get; set; }

    // ── Horario ───────────────────────────────────────────────────────────────
    [Required(ErrorMessage = "Hora de inicio requerida")]
    [Display(Name = "Desde")]
    public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

    [Required(ErrorMessage = "Hora de fin requerida")]
    [Display(Name = "Hasta")]
    public TimeSpan EndTime { get; set; } = new TimeSpan(9, 0, 0);

    // ── Vigencia ──────────────────────────────────────────────────────────────
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Vigente desde")]
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Vigente hasta")]
    public DateTime? EffectiveTo { get; set; }

    [MaxLength(300)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    // ── Helper: convierte bools → flags ──────────────────────────────────────
    public DaysOfWeekFlags ToDaysOfWeekFlags()
    {
        var flags = DaysOfWeekFlags.None;
        if (Monday)    flags |= DaysOfWeekFlags.Monday;
        if (Tuesday)   flags |= DaysOfWeekFlags.Tuesday;
        if (Wednesday) flags |= DaysOfWeekFlags.Wednesday;
        if (Thursday)  flags |= DaysOfWeekFlags.Thursday;
        if (Friday)    flags |= DaysOfWeekFlags.Friday;
        if (Saturday)  flags |= DaysOfWeekFlags.Saturday;
        if (Sunday)    flags |= DaysOfWeekFlags.Sunday;
        return flags;
    }

    // ── Helper: carga desde flags ──────────────────────────────────────────
    public static ClassScheduleViewModel FromModel(ClassSchedule s) => new()
    {
        Id            = s.Id,
        ScheduleType  = s.ScheduleType,
        Monday        = (s.DaysOfWeek & DaysOfWeekFlags.Monday)    != 0,
        Tuesday       = (s.DaysOfWeek & DaysOfWeekFlags.Tuesday)   != 0,
        Wednesday     = (s.DaysOfWeek & DaysOfWeekFlags.Wednesday) != 0,
        Thursday      = (s.DaysOfWeek & DaysOfWeekFlags.Thursday)  != 0,
        Friday        = (s.DaysOfWeek & DaysOfWeekFlags.Friday)    != 0,
        Saturday      = (s.DaysOfWeek & DaysOfWeekFlags.Saturday)  != 0,
        Sunday        = (s.DaysOfWeek & DaysOfWeekFlags.Sunday)    != 0,
        DayOfMonth    = s.DayOfMonth,
        SpecificDate  = s.SpecificDate,
        StartTime     = s.StartTime,
        EndTime       = s.EndTime,
        EffectiveFrom = s.EffectiveFrom,
        EffectiveTo   = s.EffectiveTo,
        Notes         = s.Notes
    };
}

// ── Clase (Create / Edit) ─────────────────────────────────────────────────────

public class FitnessClassViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Categoría")]
    public int? ClassCategoryId { get; set; }

    // Populated by the controller for the dropdown
    public List<SelectListItem> Categories { get; set; } = new();

    [Required(ErrorMessage = "El instructor es obligatorio")]
    [MaxLength(150)]
    [Display(Name = "Instructor / Docente")]
    public string InstructorName { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Sala / Ubicación")]
    public string? Location { get; set; }

    [MaxLength(7)]
    [Display(Name = "Color")]
    public string Color { get; set; } = "#4f88e3";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de fin")]
    public DateTime? EndDate { get; set; }

    [Range(1, 9999, ErrorMessage = "El cupo debe ser mayor a 0")]
    [Display(Name = "Cupo máximo")]
    public int? MaxCapacity { get; set; }

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    public List<ClassScheduleViewModel> Schedules { get; set; } = new() { new() };
}

// ── Lista ─────────────────────────────────────────────────────────────────────

public class FitnessClassListViewModel
{
    public int    Id              { get; set; }
    public string Name            { get; set; } = string.Empty;
    public string Category        { get; set; } = string.Empty;
    public string InstructorName  { get; set; } = string.Empty;
    public string? Location       { get; set; }
    public string Color           { get; set; } = "#ff6b35";
    public DateTime StartDate     { get; set; }
    public DateTime? EndDate      { get; set; }
    public int? MaxCapacity       { get; set; }
    public int ActiveEnrollments  { get; set; }
    public int ScheduleCount      { get; set; }
    public bool IsActive          { get; set; }
}

// ── Detalle ───────────────────────────────────────────────────────────────────

public class FitnessClassDetailViewModel
{
    public FitnessClass Class { get; set; } = null!;
    public List<ClassEnrollment> Enrollments { get; set; } = new();
    public int ActiveCount    { get; set; }
    public int WaitlistCount  { get; set; }

    // Para el formulario de inscripción rápida
    public int? NewMemberId   { get; set; }
    public string? NewNotes   { get; set; }
}

// ── Inscripción ───────────────────────────────────────────────────────────────

public class ClassEnrollmentViewModel
{
    [Required]
    public int FitnessClassId { get; set; }

    [Required(ErrorMessage = "Seleccione un socio")]
    public int MemberId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
