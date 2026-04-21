using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.Services;

public interface IPlanService
{
    Task<List<Plan>> GetAllAsync(bool activeOnly = false);
    Task<Plan?> GetByIdAsync(int id);
    Task<Plan> CreateAsync(PlanViewModel vm);
    Task<Plan> UpdateAsync(PlanViewModel vm);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<SelectListItem>> GetSelectListAsync(bool activeOnly = true);
}
