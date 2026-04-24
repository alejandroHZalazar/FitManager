using FitManager.Models;
using FitManager.ViewModels;

namespace FitManager.Services;

public interface IRoutineService
{
    // ── Ejercicios ────────────────────────────────────────────────────────────
    Task<List<ExerciseListViewModel>> GetAllExercisesAsync(MuscleGroup? muscle = null, ExerciseType? type = null);
    Task<Exercise?> GetExerciseByIdAsync(int id);
    Task<Exercise>  CreateExerciseAsync(ExerciseViewModel vm);
    Task<Exercise?> UpdateExerciseAsync(ExerciseViewModel vm);
    Task<(bool ok, string error)> DeleteExerciseAsync(int id);
    Task<bool>      ToggleExerciseActiveAsync(int id);

    // ── Rutinas ───────────────────────────────────────────────────────────────
    Task<List<RoutineListViewModel>> GetAllRoutinesAsync();
    Task<RoutineDetailViewModel?>    GetRoutineDetailAsync(int id);
    Task<RoutineEditViewModel?>      GetRoutineEditAsync(int id);
    Task<Routine>                    CreateRoutineAsync(RoutineEditViewModel vm, string createdBy);
    Task<Routine?>                   UpdateRoutineAsync(RoutineEditViewModel vm);
    Task<bool>                       DeleteRoutineAsync(int id);
    Task<bool>                       ToggleRoutineActiveAsync(int id);
    Task<List<ExerciseSelectItem>>   GetExerciseSelectListAsync();

    // ── Asignaciones a socios ─────────────────────────────────────────────────
    Task<List<MemberRoutineListViewModel>> GetAllAssignmentsAsync();
    Task<List<MemberRoutineListViewModel>> GetMemberHistoryAsync(int memberId);
    Task<MemberRoutine>                    AssignAsync(AssignRoutineViewModel vm, string assignedBy);
    Task<bool>                             DeactivateAssignmentAsync(int id);
    Task<RoutinePrintViewModel?>           GetPrintDataAsync(int memberRoutineId);
}
