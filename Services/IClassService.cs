using FitManager.Models;
using FitManager.ViewModels;

namespace FitManager.Services;

public interface IClassService
{
    // ── CRUD Clases ───────────────────────────────────────────────────────────
    Task<List<FitnessClassListViewModel>> GetAllAsync();
    Task<FitnessClass?> GetByIdAsync(int id);
    Task<FitnessClassDetailViewModel?> GetDetailAsync(int id);
    Task<FitnessClass> CreateAsync(FitnessClassViewModel vm, string createdBy);
    Task<FitnessClass?> UpdateAsync(FitnessClassViewModel vm);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleActiveAsync(int id);

    // ── Horarios ──────────────────────────────────────────────────────────────
    Task AddScheduleAsync(int classId, ClassScheduleViewModel vm);
    Task<bool> DeleteScheduleAsync(int scheduleId);

    // ── Inscripciones ─────────────────────────────────────────────────────────
    Task<(bool ok, string message)> EnrollAsync(ClassEnrollmentViewModel vm, string enrolledBy);
    Task<bool> UnenrollAsync(int enrollmentId);
    Task<bool> SetEnrollmentStatusAsync(int enrollmentId, EnrollmentStatus status);

    // ── Calendario ───────────────────────────────────────────────────────────
    Task<ClassCalendarViewModel> GetCalendarWeekAsync(DateTime weekStart);
}
