using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class RoutineService : IRoutineService
{
    private readonly ApplicationDbContext _db;
    private readonly ICompanyService      _company;

    public RoutineService(ApplicationDbContext db, ICompanyService company)
    {
        _db      = db;
        _company = company;
    }

    // ── Helpers de etiquetas ──────────────────────────────────────────────────

    public static string GoalLabel(RoutineGoal g) => g switch
    {
        RoutineGoal.MassGain       => "Ganar Masa Muscular",
        RoutineGoal.WeightLoss     => "Bajar de Peso",
        RoutineGoal.Definition     => "Definición",
        RoutineGoal.Strength       => "Fuerza",
        RoutineGoal.Endurance      => "Resistencia",
        RoutineGoal.Rehabilitation => "Rehabilitación",
        RoutineGoal.Maintenance    => "Mantenimiento",
        RoutineGoal.Flexibility    => "Flexibilidad",
        _                          => "Otro"
    };

    public static string MuscleLabel(MuscleGroup m) => m switch
    {
        MuscleGroup.Chest      => "Pecho",
        MuscleGroup.Back       => "Espalda",
        MuscleGroup.Shoulders  => "Hombros",
        MuscleGroup.Biceps     => "Bíceps",
        MuscleGroup.Triceps    => "Tríceps",
        MuscleGroup.Quadriceps => "Cuádriceps",
        MuscleGroup.Hamstrings => "Femorales",
        MuscleGroup.Calves     => "Pantorrillas",
        MuscleGroup.Glutes     => "Glúteos",
        MuscleGroup.Abs        => "Abdominales",
        MuscleGroup.Cardio     => "Cardio",
        MuscleGroup.FullBody   => "Cuerpo Completo",
        _                      => "Otro"
    };

    public static string TypeLabel(ExerciseType t) => t switch
    {
        ExerciseType.Strength    => "Fuerza",
        ExerciseType.Cardio      => "Cardio",
        ExerciseType.Flexibility => "Flexibilidad",
        ExerciseType.Balance     => "Equilibrio",
        ExerciseType.Functional  => "Funcional",
        _                        => "Otro"
    };

    // ── Ejercicios ────────────────────────────────────────────────────────────

    public async Task<List<ExerciseListViewModel>> GetAllExercisesAsync(
        MuscleGroup? muscle = null, ExerciseType? type = null)
    {
        var q = _db.Exercises
            .Include(e => e.RoutineExercises)
            .AsQueryable();

        if (muscle.HasValue) q = q.Where(e => e.MuscleGroup  == muscle.Value);
        if (type.HasValue)   q = q.Where(e => e.ExerciseType == type.Value);

        var list = await q.OrderBy(e => e.MuscleGroup).ThenBy(e => e.Name).ToListAsync();

        return list.Select(e => new ExerciseListViewModel
        {
            Id               = e.Id,
            Name             = e.Name,
            MuscleGroup      = MuscleLabel(e.MuscleGroup),
            ExerciseType     = TypeLabel(e.ExerciseType),
            MuscleGroupEnum  = e.MuscleGroup,
            ExerciseTypeEnum = e.ExerciseType,
            IsCustom         = e.IsCustom,
            IsActive         = e.IsActive,
            UsageCount       = e.RoutineExercises.Count
        }).ToList();
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int id) =>
        await _db.Exercises.FindAsync(id);

    public async Task<Exercise> CreateExerciseAsync(ExerciseViewModel vm)
    {
        var ex = new Exercise
        {
            Name         = vm.Name,
            Description  = vm.Description,
            MuscleGroup  = vm.MuscleGroup,
            ExerciseType = vm.ExerciseType,
            IsCustom     = true,
            IsActive     = vm.IsActive,
            CreatedAt    = DateTime.UtcNow
        };
        _db.Exercises.Add(ex);
        await _db.SaveChangesAsync();
        return ex;
    }

    public async Task<Exercise?> UpdateExerciseAsync(ExerciseViewModel vm)
    {
        var ex = await _db.Exercises.FindAsync(vm.Id);
        if (ex == null) return null;

        ex.Name         = vm.Name;
        ex.Description  = vm.Description;
        ex.MuscleGroup  = vm.MuscleGroup;
        ex.ExerciseType = vm.ExerciseType;
        ex.IsActive     = vm.IsActive;

        await _db.SaveChangesAsync();
        return ex;
    }

    public async Task<(bool ok, string error)> DeleteExerciseAsync(int id)
    {
        var ex = await _db.Exercises.Include(e => e.RoutineExercises).FirstOrDefaultAsync(e => e.Id == id);
        if (ex == null) return (false, "Ejercicio no encontrado.");
        if (ex.RoutineExercises.Any())
            return (false, $"No se puede eliminar: está en uso en {ex.RoutineExercises.Count} rutina(s).");
        _db.Exercises.Remove(ex);
        await _db.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<bool> ToggleExerciseActiveAsync(int id)
    {
        var ex = await _db.Exercises.FindAsync(id);
        if (ex == null) return false;
        ex.IsActive = !ex.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ExerciseSelectItem>> GetExerciseSelectListAsync()
    {
        return await _db.Exercises
            .Where(e => e.IsActive)
            .OrderBy(e => e.MuscleGroup)
            .ThenBy(e => e.Name)
            .Select(e => new ExerciseSelectItem
            {
                Id          = e.Id,
                Name        = e.Name,
                MuscleGroup = e.MuscleGroup.ToString()
            })
            .ToListAsync();
    }

    // ── Rutinas ───────────────────────────────────────────────────────────────

    public async Task<List<RoutineListViewModel>> GetAllRoutinesAsync()
    {
        var list = await _db.Routines
            .Include(r => r.Days)
            .Include(r => r.MemberRoutines)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return list.Select(r => new RoutineListViewModel
        {
            Id               = r.Id,
            Name             = r.Name,
            GoalLabel        = GoalLabel(r.Goal),
            DurationWeeks    = r.DurationWeeks,
            FrequencyPerWeek = r.FrequencyPerWeek,
            IsGeneral        = r.IsGeneral,
            DayCount         = r.Days.Count,
            AssignedCount    = r.MemberRoutines.Count(mr => mr.IsActive),
            IsActive         = r.IsActive
        }).ToList();
    }

    public async Task<RoutineDetailViewModel?> GetRoutineDetailAsync(int id)
    {
        var r = await _db.Routines
            .Include(r => r.Days.OrderBy(d => d.OrderIndex))
                .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
                    .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (r == null) return null;

        return new RoutineDetailViewModel { Routine = r, GoalLabel = GoalLabel(r.Goal) };
    }

    public async Task<RoutineEditViewModel?> GetRoutineEditAsync(int id)
    {
        var r = await _db.Routines
            .Include(r => r.Days.OrderBy(d => d.OrderIndex))
                .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (r == null) return null;

        return new RoutineEditViewModel
        {
            Id               = r.Id,
            Name             = r.Name,
            Description      = r.Description,
            Goal             = r.Goal,
            DurationWeeks    = r.DurationWeeks,
            FrequencyPerWeek = r.FrequencyPerWeek,
            IsGeneral        = r.IsGeneral,
            IsActive         = r.IsActive,
            Days = r.Days.Select(d => new RoutineDayFormVM
            {
                Id        = d.Id,
                DayNumber = d.DayNumber,
                Name      = d.Name,
                IsRestDay = d.IsRestDay,
                Exercises = d.Exercises.Select(e => new RoutineExerciseFormVM
                {
                    Id          = e.Id,
                    ExerciseId  = e.ExerciseId,
                    Sets        = e.Sets,
                    Reps        = e.Reps,
                    Weight      = e.Weight,
                    RestSeconds = e.RestSeconds,
                    Notes       = e.Notes,
                    OrderIndex  = e.OrderIndex
                }).ToList()
            }).ToList()
        };
    }

    public async Task<Routine> CreateRoutineAsync(RoutineEditViewModel vm, string createdBy)
    {
        var routine = new Routine
        {
            Name             = vm.Name,
            Description      = vm.Description,
            Goal             = vm.Goal,
            DurationWeeks    = vm.DurationWeeks,
            FrequencyPerWeek = vm.FrequencyPerWeek,
            IsGeneral        = vm.IsGeneral,
            IsActive         = vm.IsActive,
            CreatedAt        = DateTime.UtcNow,
            CreatedBy        = createdBy
        };

        MapDaysFromVm(routine, vm.Days);

        _db.Routines.Add(routine);
        await _db.SaveChangesAsync();
        return routine;
    }

    public async Task<Routine?> UpdateRoutineAsync(RoutineEditViewModel vm)
    {
        var routine = await _db.Routines
            .Include(r => r.Days).ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(r => r.Id == vm.Id);
        if (routine == null) return null;

        routine.Name             = vm.Name;
        routine.Description      = vm.Description;
        routine.Goal             = vm.Goal;
        routine.DurationWeeks    = vm.DurationWeeks;
        routine.FrequencyPerWeek = vm.FrequencyPerWeek;
        routine.IsGeneral        = vm.IsGeneral;
        routine.IsActive         = vm.IsActive;

        // Eliminar días existentes y recrear desde el VM
        _db.RoutineExercises.RemoveRange(routine.Days.SelectMany(d => d.Exercises));
        _db.RoutineDays.RemoveRange(routine.Days);
        routine.Days.Clear();

        MapDaysFromVm(routine, vm.Days);

        await _db.SaveChangesAsync();
        return routine;
    }

    private static void MapDaysFromVm(Routine routine, List<RoutineDayFormVM> days)
    {
        for (int i = 0; i < days.Count; i++)
        {
            var dvm = days[i];
            var day = new RoutineDay
            {
                DayNumber  = dvm.DayNumber > 0 ? dvm.DayNumber : i + 1,
                Name       = dvm.Name,
                IsRestDay  = dvm.IsRestDay,
                OrderIndex = i
            };

            if (!dvm.IsRestDay)
            {
                for (int j = 0; j < dvm.Exercises.Count; j++)
                {
                    var evm = dvm.Exercises[j];
                    if (evm.ExerciseId <= 0) continue;
                    day.Exercises.Add(new RoutineExercise
                    {
                        ExerciseId  = evm.ExerciseId,
                        Sets        = evm.Sets > 0 ? evm.Sets : 3,
                        Reps        = string.IsNullOrWhiteSpace(evm.Reps) ? "10" : evm.Reps,
                        Weight      = evm.Weight,
                        RestSeconds = evm.RestSeconds,
                        Notes       = evm.Notes,
                        OrderIndex  = j
                    });
                }
            }

            routine.Days.Add(day);
        }
    }

    public async Task<bool> DeleteRoutineAsync(int id)
    {
        var r = await _db.Routines.FindAsync(id);
        if (r == null) return false;
        _db.Routines.Remove(r);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleRoutineActiveAsync(int id)
    {
        var r = await _db.Routines.FindAsync(id);
        if (r == null) return false;
        r.IsActive = !r.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Asignaciones ──────────────────────────────────────────────────────────

    public async Task<List<MemberRoutineListViewModel>> GetAllAssignmentsAsync()
    {
        var list = await _db.MemberRoutines
            .Include(mr => mr.Member)
            .Include(mr => mr.Routine)
            .OrderByDescending(mr => mr.AssignedAt)
            .ToListAsync();

        return list.Select(MapAssignment).ToList();
    }

    public async Task<List<MemberRoutineListViewModel>> GetMemberHistoryAsync(int memberId)
    {
        var list = await _db.MemberRoutines
            .Include(mr => mr.Member)
            .Include(mr => mr.Routine)
            .Where(mr => mr.MemberId == memberId)
            .OrderByDescending(mr => mr.AssignedAt)
            .ToListAsync();

        return list.Select(MapAssignment).ToList();
    }

    private static MemberRoutineListViewModel MapAssignment(MemberRoutine mr) => new()
    {
        Id           = mr.Id,
        MemberId     = mr.MemberId,
        MemberName   = mr.Member.FullName,
        MemberNumber = mr.Member.MemberNumber,
        RoutineId    = mr.RoutineId,
        RoutineName  = mr.Routine.Name,
        GoalLabel    = GoalLabel(mr.Routine.Goal),
        AssignedAt   = mr.AssignedAt,
        StartDate    = mr.StartDate,
        EndDate      = mr.EndDate,
        IsActive     = mr.IsActive,
        Notes        = mr.Notes
    };

    public async Task<MemberRoutine> AssignAsync(AssignRoutineViewModel vm, string assignedBy)
    {
        // Desactivar asignaciones previas activas del socio
        var prev = await _db.MemberRoutines
            .Where(mr => mr.MemberId == vm.MemberId && mr.IsActive)
            .ToListAsync();
        foreach (var p in prev)
        {
            p.IsActive = false;
            p.EndDate  = DateTime.Today;
        }

        var assignment = new MemberRoutine
        {
            MemberId   = vm.MemberId,
            RoutineId  = vm.RoutineId,
            AssignedAt = DateTime.UtcNow,
            StartDate  = vm.StartDate,
            EndDate    = vm.EndDate,
            IsActive   = true,
            Notes      = vm.Notes,
            AssignedBy = assignedBy
        };

        _db.MemberRoutines.Add(assignment);
        await _db.SaveChangesAsync();
        return assignment;
    }

    public async Task<bool> DeactivateAssignmentAsync(int id)
    {
        var mr = await _db.MemberRoutines.FindAsync(id);
        if (mr == null) return false;
        mr.IsActive = false;
        mr.EndDate  ??= DateTime.Today;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<RoutinePrintViewModel?> GetPrintDataAsync(int memberRoutineId)
    {
        var mr = await _db.MemberRoutines
            .Include(m => m.Member)
            .Include(m => m.Routine)
                .ThenInclude(r => r.Days.OrderBy(d => d.OrderIndex))
                    .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
                        .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(m => m.Id == memberRoutineId);

        if (mr == null) return null;

        return new RoutinePrintViewModel
        {
            Company    = await _company.GetAsync(),
            Member     = mr.Member,
            Assignment = mr,
            Routine    = mr.Routine,
            GoalLabel  = GoalLabel(mr.Routine.Goal)
        };
    }
}
