using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.Services;

public interface INutritionService
{
    // ── Planes ────────────────────────────────────────────────────────────────
    Task<List<NutritionPlanListViewModel>> GetAllPlansAsync();
    Task<NutritionPlanDetailViewModel?>    GetPlanDetailAsync(int id);
    Task<NutritionPlanEditViewModel?>      GetPlanEditAsync(int id);
    Task<NutritionPlan>                    CreatePlanAsync(NutritionPlanEditViewModel vm, string createdBy);
    Task<NutritionPlan?>                   UpdatePlanAsync(NutritionPlanEditViewModel vm);
    Task<bool>                             DeletePlanAsync(int id);
    Task<bool>                             TogglePlanActiveAsync(int id);
    Task<List<SelectListItem>>             GetPlanSelectListAsync();

    // ── Asignaciones ──────────────────────────────────────────────────────────
    Task<List<MemberNutritionListViewModel>> GetAllAssignmentsAsync();
    Task<List<MemberNutritionListViewModel>> GetMemberHistoryAsync(int memberId);
    Task<MemberNutritionPlan>               AssignAsync(AssignNutritionViewModel vm, string assignedBy);
    Task<bool>                              DeactivateAssignmentAsync(int id);
    Task<NutritionPrintViewModel?>          GetPrintDataAsync(int memberNutritionPlanId);
}
