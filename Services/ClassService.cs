using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class ClassService : IClassService
{
    private readonly ApplicationDbContext _db;
    public ClassService(ApplicationDbContext db) => _db = db;

    // ── CRUD Clases ───────────────────────────────────────────────────────────

    public async Task<List<FitnessClassListViewModel>> GetAllAsync()
    {
        var classes = await _db.FitnessClasses
            .Include(c => c.Schedules)
            .Include(c => c.Enrollments)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return classes.Select(c => new FitnessClassListViewModel
        {
            Id               = c.Id,
            Name             = c.Name,
            Category         = CategoryLabel(c.Category),
            InstructorName   = c.InstructorName,
            Location         = c.Location,
            Color            = c.Color,
            StartDate        = c.StartDate,
            EndDate          = c.EndDate,
            MaxCapacity      = c.MaxCapacity,
            ActiveEnrollments= c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
            ScheduleCount    = c.Schedules.Count(s => s.IsActive),
            IsActive         = c.IsActive
        }).ToList();
    }

    public async Task<FitnessClass?> GetByIdAsync(int id) =>
        await _db.FitnessClasses
            .Include(c => c.Schedules.OrderBy(s => s.StartTime))
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<FitnessClassDetailViewModel?> GetDetailAsync(int id)
    {
        var fc = await _db.FitnessClasses
            .Include(c => c.Schedules.OrderBy(s => s.StartTime))
            .Include(c => c.Enrollments.OrderBy(e => e.EnrolledAt))
                .ThenInclude(e => e.Member)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (fc == null) return null;

        return new FitnessClassDetailViewModel
        {
            Class         = fc,
            Enrollments   = fc.Enrollments.ToList(),
            ActiveCount   = fc.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
            WaitlistCount = fc.Enrollments.Count(e => e.Status == EnrollmentStatus.Waitlisted)
        };
    }

    public async Task<FitnessClass> CreateAsync(FitnessClassViewModel vm, string createdBy)
    {
        var fc = new FitnessClass
        {
            Name          = vm.Name,
            Description   = vm.Description,
            Category      = vm.Category,
            InstructorName= vm.InstructorName,
            Location      = vm.Location,
            Color         = vm.Color,
            StartDate     = vm.StartDate,
            EndDate       = vm.EndDate,
            MaxCapacity   = vm.MaxCapacity,
            IsActive      = vm.IsActive,
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = createdBy
        };

        foreach (var svm in vm.Schedules.Where(s => IsValidSchedule(s)))
            fc.Schedules.Add(ToScheduleModel(svm));

        _db.FitnessClasses.Add(fc);
        await _db.SaveChangesAsync();
        return fc;
    }

    public async Task<FitnessClass?> UpdateAsync(FitnessClassViewModel vm)
    {
        var fc = await _db.FitnessClasses
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.Id == vm.Id);
        if (fc == null) return null;

        fc.Name           = vm.Name;
        fc.Description    = vm.Description;
        fc.Category       = vm.Category;
        fc.InstructorName = vm.InstructorName;
        fc.Location       = vm.Location;
        fc.Color          = vm.Color;
        fc.StartDate      = vm.StartDate;
        fc.EndDate        = vm.EndDate;
        fc.MaxCapacity    = vm.MaxCapacity;
        fc.IsActive       = vm.IsActive;

        // Reemplazar horarios: eliminar los existentes y agregar los nuevos
        _db.ClassSchedules.RemoveRange(fc.Schedules);
        fc.Schedules.Clear();

        foreach (var svm in vm.Schedules.Where(s => IsValidSchedule(s)))
            fc.Schedules.Add(ToScheduleModel(svm));

        await _db.SaveChangesAsync();
        return fc;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var fc = await _db.FitnessClasses.FindAsync(id);
        if (fc == null) return false;
        _db.FitnessClasses.Remove(fc);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var fc = await _db.FitnessClasses.FindAsync(id);
        if (fc == null) return false;
        fc.IsActive = !fc.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Horarios ──────────────────────────────────────────────────────────────

    public async Task AddScheduleAsync(int classId, ClassScheduleViewModel vm)
    {
        var s = ToScheduleModel(vm);
        s.FitnessClassId = classId;
        _db.ClassSchedules.Add(s);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteScheduleAsync(int scheduleId)
    {
        var s = await _db.ClassSchedules.FindAsync(scheduleId);
        if (s == null) return false;
        _db.ClassSchedules.Remove(s);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Inscripciones ─────────────────────────────────────────────────────────

    public async Task<(bool ok, string message)> EnrollAsync(ClassEnrollmentViewModel vm, string enrolledBy)
    {
        var fc = await _db.FitnessClasses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == vm.FitnessClassId);

        if (fc == null) return (false, "Clase no encontrada.");

        // Verificar si ya está inscripto
        var existing = fc.Enrollments.FirstOrDefault(e =>
            e.MemberId == vm.MemberId && e.Status != EnrollmentStatus.Cancelled);

        if (existing != null)
        {
            return existing.Status == EnrollmentStatus.Waitlisted
                ? (false, "El socio ya está en lista de espera.")
                : (false, "El socio ya está inscripto en esta clase.");
        }

        // Verificar cupo
        var activeCount = fc.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
        var status = fc.MaxCapacity.HasValue && activeCount >= fc.MaxCapacity.Value
            ? EnrollmentStatus.Waitlisted
            : EnrollmentStatus.Active;

        _db.ClassEnrollments.Add(new ClassEnrollment
        {
            FitnessClassId = vm.FitnessClassId,
            MemberId       = vm.MemberId,
            EnrolledAt     = DateTime.UtcNow,
            Status         = status,
            Notes          = vm.Notes,
            EnrolledBy     = enrolledBy
        });

        await _db.SaveChangesAsync();

        return status == EnrollmentStatus.Waitlisted
            ? (true, "Sin cupo disponible. El socio fue agregado a la lista de espera.")
            : (true, "Socio inscripto exitosamente.");
    }

    public async Task<bool> UnenrollAsync(int enrollmentId)
    {
        var e = await _db.ClassEnrollments.FindAsync(enrollmentId);
        if (e == null) return false;
        e.Status = EnrollmentStatus.Cancelled;

        // Promover al siguiente en lista de espera si había cupo
        var fc = await _db.FitnessClasses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == e.FitnessClassId);

        if (fc?.MaxCapacity.HasValue == true)
        {
            var activeCount = fc.Enrollments.Count(x => x.Status == EnrollmentStatus.Active && x.Id != enrollmentId);
            if (activeCount < fc.MaxCapacity.Value)
            {
                var next = fc.Enrollments
                    .Where(x => x.Status == EnrollmentStatus.Waitlisted)
                    .OrderBy(x => x.EnrolledAt)
                    .FirstOrDefault();
                if (next != null) next.Status = EnrollmentStatus.Active;
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetEnrollmentStatusAsync(int enrollmentId, EnrollmentStatus status)
    {
        var e = await _db.ClassEnrollments.FindAsync(enrollmentId);
        if (e == null) return false;
        e.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Calendario ───────────────────────────────────────────────────────────

    public async Task<ClassCalendarViewModel> GetCalendarWeekAsync(DateTime weekStart)
    {
        // Asegurar que weekStart sea lunes
        while (weekStart.DayOfWeek != DayOfWeek.Monday)
            weekStart = weekStart.AddDays(-1);

        var weekEnd = weekStart.AddDays(6);

        var classes = await _db.FitnessClasses
            .Include(c => c.Schedules.Where(s => s.IsActive))
            .Include(c => c.Enrollments.Where(e => e.Status == EnrollmentStatus.Active))
            .Where(c => c.IsActive
                     && c.StartDate.Date <= weekEnd.Date
                     && (c.EndDate == null || c.EndDate.Value.Date >= weekStart.Date))
            .ToListAsync();

        var days = new List<CalendarDayViewModel>();

        for (var day = weekStart; day <= weekEnd; day = day.AddDays(1))
        {
            var slots = new List<CalendarSlotViewModel>();

            foreach (var fc in classes)
            {
                foreach (var schedule in fc.Schedules.Where(s => s.IsActive))
                {
                    if (fc.StartDate.Date > day.Date) continue;
                    if (fc.EndDate.HasValue && fc.EndDate.Value.Date < day.Date) continue;
                    if (schedule.EffectiveFrom.Date > day.Date) continue;
                    if (schedule.EffectiveTo.HasValue && schedule.EffectiveTo.Value.Date < day.Date) continue;

                    if (!AppliesToDay(schedule, day)) continue;

                    var enrolled = fc.Enrollments.Count;
                    slots.Add(new CalendarSlotViewModel
                    {
                        ClassId        = fc.Id,
                        ClassName      = fc.Name,
                        Category       = fc.Category,
                        InstructorName = fc.InstructorName,
                        Location       = fc.Location,
                        StartTime      = schedule.StartTime,
                        EndTime        = schedule.EndTime,
                        Color          = fc.Color,
                        EnrolledCount  = enrolled,
                        MaxCapacity    = fc.MaxCapacity,
                        IsFull         = fc.MaxCapacity.HasValue && enrolled >= fc.MaxCapacity.Value,
                        ScheduleNotes  = schedule.Notes
                    });
                }
            }

            days.Add(new CalendarDayViewModel
            {
                Date  = day,
                Slots = slots.OrderBy(s => s.StartTime).ToList()
            });
        }

        return new ClassCalendarViewModel
        {
            WeekStart = weekStart,
            WeekEnd   = weekEnd,
            Days      = days
        };
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    private static bool AppliesToDay(ClassSchedule s, DateTime day)
    {
        return s.ScheduleType switch
        {
            ScheduleType.Weekly   => AppliesToDayWeekly(s, day),
            ScheduleType.Biweekly => AppliesToDayBiweekly(s, day),
            ScheduleType.Monthly  => s.DayOfMonth.HasValue && day.Day == s.DayOfMonth.Value,
            ScheduleType.OneTime  => s.SpecificDate.HasValue && s.SpecificDate.Value.Date == day.Date,
            _ => false
        };
    }

    private static bool AppliesToDayWeekly(ClassSchedule s, DateTime day)
    {
        var flag = DayToFlag(day.DayOfWeek);
        return (s.DaysOfWeek & flag) != 0;
    }

    private static bool AppliesToDayBiweekly(ClassSchedule s, DateTime day)
    {
        if (!AppliesToDayWeekly(s, day)) return false;
        var weeksDiff = (int)((day.Date - s.EffectiveFrom.Date).TotalDays / 7);
        return weeksDiff >= 0 && weeksDiff % 2 == 0;
    }

    private static DaysOfWeekFlags DayToFlag(DayOfWeek d) => d switch
    {
        DayOfWeek.Sunday    => DaysOfWeekFlags.Sunday,
        DayOfWeek.Monday    => DaysOfWeekFlags.Monday,
        DayOfWeek.Tuesday   => DaysOfWeekFlags.Tuesday,
        DayOfWeek.Wednesday => DaysOfWeekFlags.Wednesday,
        DayOfWeek.Thursday  => DaysOfWeekFlags.Thursday,
        DayOfWeek.Friday    => DaysOfWeekFlags.Friday,
        DayOfWeek.Saturday  => DaysOfWeekFlags.Saturday,
        _ => DaysOfWeekFlags.None
    };

    private static ClassSchedule ToScheduleModel(ClassScheduleViewModel vm) => new()
    {
        ScheduleType  = vm.ScheduleType,
        DaysOfWeek    = vm.ToDaysOfWeekFlags(),
        DayOfMonth    = vm.DayOfMonth,
        SpecificDate  = vm.SpecificDate,
        StartTime     = vm.StartTime,
        EndTime       = vm.EndTime,
        EffectiveFrom = vm.EffectiveFrom,
        EffectiveTo   = vm.EffectiveTo,
        Notes         = vm.Notes,
        IsActive      = true
    };

    private static bool IsValidSchedule(ClassScheduleViewModel vm)
    {
        return vm.ScheduleType switch
        {
            ScheduleType.Weekly   => vm.ToDaysOfWeekFlags() != DaysOfWeekFlags.None,
            ScheduleType.Biweekly => vm.ToDaysOfWeekFlags() != DaysOfWeekFlags.None,
            ScheduleType.Monthly  => vm.DayOfMonth.HasValue,
            ScheduleType.OneTime  => vm.SpecificDate.HasValue,
            _ => false
        };
    }

    private static string CategoryLabel(ClassCategory c) => c switch
    {
        ClassCategory.Spinning    => "Spinning",
        ClassCategory.Yoga        => "Yoga",
        ClassCategory.Pilates     => "Pilates",
        ClassCategory.Zumba       => "Zumba",
        ClassCategory.CrossFit    => "CrossFit",
        ClassCategory.Functional  => "Funcional",
        ClassCategory.Kickboxing  => "Kickboxing",
        ClassCategory.Natacion    => "Natación",
        ClassCategory.Musculacion => "Musculación",
        _                         => "Otro"
    };
}
