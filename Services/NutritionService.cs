using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class NutritionService : INutritionService
{
    private readonly ApplicationDbContext _db;
    private readonly ICompanyService      _company;

    public NutritionService(ApplicationDbContext db, ICompanyService company)
    {
        _db      = db;
        _company = company;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public static string GoalLabel(NutritionGoal g) => g switch
    {
        NutritionGoal.WeightLoss  => "Bajar de Peso",
        NutritionGoal.MassGain    => "Ganar Masa",
        NutritionGoal.Maintenance => "Mantenimiento",
        NutritionGoal.Definition  => "Definición",
        NutritionGoal.Endurance   => "Resistencia",
        _                         => "Otro"
    };

    public static string MealLabel(MealType t) => t switch
    {
        MealType.Breakfast      => "Desayuno",
        MealType.MorningSnack   => "Colación Mañana",
        MealType.Lunch          => "Almuerzo",
        MealType.AfternoonSnack => "Merienda",
        MealType.Dinner         => "Cena",
        MealType.EveningSnack   => "Colación Noche",
        _                       => "Comida"
    };

    private static string DurationLabel(int days) => days switch
    {
        7  => "Semanal (7 días)",
        14 => "Quincenal (14 días)",
        30 => "Mensual (30 días)",
        _  => $"{days} días"
    };

    // ── Planes ────────────────────────────────────────────────────────────────

    public async Task<List<NutritionPlanListViewModel>> GetAllPlansAsync()
    {
        var list = await _db.NutritionPlans
            .Include(p => p.Meals)
            .Include(p => p.Assignments)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return list.Select(p => new NutritionPlanListViewModel
        {
            Id            = p.Id,
            Name          = p.Name,
            GoalLabel     = GoalLabel(p.Goal),
            DurationDays  = p.DurationDays,
            DurationLabel = DurationLabel(p.DurationDays),
            MealCount     = p.Meals.Count,
            AssignedCount = p.Assignments.Count(a => a.IsActive),
            IsTemplate    = p.IsTemplate,
            IsActive      = p.IsActive
        }).ToList();
    }

    public async Task<NutritionPlanDetailViewModel?> GetPlanDetailAsync(int id)
    {
        var p = await _db.NutritionPlans
            .Include(x => x.Meals.OrderBy(m => m.OrderIndex))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new NutritionPlanDetailViewModel
        {
            NutritionPlan = p,
            GoalLabel     = GoalLabel(p.Goal),
            DurationLabel = DurationLabel(p.DurationDays)
        };
    }

    public async Task<NutritionPlanEditViewModel?> GetPlanEditAsync(int id)
    {
        var p = await _db.NutritionPlans
            .Include(x => x.Meals.OrderBy(m => m.OrderIndex))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        // Build 6-slot list, populate from DB where exists
        var vm = new NutritionPlanEditViewModel
        {
            Id           = p.Id,
            Name         = p.Name,
            Description  = p.Description,
            Goal         = p.Goal,
            DurationDays = p.DurationDays,
            GeneralNotes = p.GeneralNotes,
            IsTemplate   = p.IsTemplate,
            IsActive     = p.IsActive,
            Meals        = NutritionPlanEditViewModel.CreateDefaultMeals()
        };

        foreach (var slot in vm.Meals)
        {
            var dbMeal = p.Meals.FirstOrDefault(m => m.MealType == slot.MealType);
            if (dbMeal != null)
            {
                slot.Id         = dbMeal.Id;
                slot.IsEnabled  = true;
                slot.CustomName = dbMeal.CustomName;
                slot.Content    = dbMeal.Content;
                slot.Quantities = dbMeal.Quantities;
                slot.Notes      = dbMeal.Notes;
            }
        }

        return vm;
    }

    public async Task<NutritionPlan> CreatePlanAsync(NutritionPlanEditViewModel vm, string createdBy)
    {
        var plan = new NutritionPlan
        {
            Name         = vm.Name,
            Description  = vm.Description,
            Goal         = vm.Goal,
            DurationDays = vm.DurationDays,
            GeneralNotes = vm.GeneralNotes,
            IsTemplate   = vm.IsTemplate,
            IsActive     = vm.IsActive,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = createdBy
        };

        MapMealsFromVm(plan, vm.Meals);

        _db.NutritionPlans.Add(plan);
        await _db.SaveChangesAsync();
        return plan;
    }

    public async Task<NutritionPlan?> UpdatePlanAsync(NutritionPlanEditViewModel vm)
    {
        var plan = await _db.NutritionPlans
            .Include(p => p.Meals)
            .FirstOrDefaultAsync(p => p.Id == vm.Id);

        if (plan == null) return null;

        plan.Name         = vm.Name;
        plan.Description  = vm.Description;
        plan.Goal         = vm.Goal;
        plan.DurationDays = vm.DurationDays;
        plan.GeneralNotes = vm.GeneralNotes;
        plan.IsTemplate   = vm.IsTemplate;
        plan.IsActive     = vm.IsActive;

        // Delete-and-recreate meals
        _db.NutritionMeals.RemoveRange(plan.Meals);
        plan.Meals.Clear();
        MapMealsFromVm(plan, vm.Meals);

        await _db.SaveChangesAsync();
        return plan;
    }

    private static void MapMealsFromVm(NutritionPlan plan, List<NutritionMealFormVM> meals)
    {
        int order = 0;
        foreach (var m in meals.Where(m => m.IsEnabled))
        {
            if (string.IsNullOrWhiteSpace(m.Content) &&
                string.IsNullOrWhiteSpace(m.Quantities) &&
                string.IsNullOrWhiteSpace(m.CustomName))
                continue; // skip empty enabled slots

            plan.Meals.Add(new NutritionMeal
            {
                MealType   = m.MealType,
                CustomName = m.CustomName?.Trim(),
                Content    = m.Content?.Trim(),
                Quantities = m.Quantities?.Trim(),
                Notes      = m.Notes?.Trim(),
                OrderIndex = order++
            });
        }
    }

    public async Task<bool> DeletePlanAsync(int id)
    {
        var p = await _db.NutritionPlans.FindAsync(id);
        if (p == null) return false;
        _db.NutritionPlans.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TogglePlanActiveAsync(int id)
    {
        var p = await _db.NutritionPlans.FindAsync(id);
        if (p == null) return false;
        p.IsActive = !p.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<SelectListItem>> GetPlanSelectListAsync()
    {
        var plans = await _db.NutritionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return plans.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text  = $"{p.Name} ({GoalLabel(p.Goal)} — {DurationLabel(p.DurationDays)})"
        }).ToList();
    }

    // ── Asignaciones ──────────────────────────────────────────────────────────

    public async Task<List<MemberNutritionListViewModel>> GetAllAssignmentsAsync()
    {
        var list = await _db.MemberNutritionPlans
            .Include(a => a.Member)
            .Include(a => a.NutritionPlan)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        return list.Select(MapAssignment).ToList();
    }

    public async Task<List<MemberNutritionListViewModel>> GetMemberHistoryAsync(int memberId)
    {
        var list = await _db.MemberNutritionPlans
            .Include(a => a.Member)
            .Include(a => a.NutritionPlan)
            .Where(a => a.MemberId == memberId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        return list.Select(MapAssignment).ToList();
    }

    private static MemberNutritionListViewModel MapAssignment(MemberNutritionPlan a) => new()
    {
        Id              = a.Id,
        MemberId        = a.MemberId,
        MemberName      = a.Member.FullName,
        MemberNumber    = a.Member.MemberNumber,
        NutritionPlanId = a.NutritionPlanId,
        PlanName        = a.NutritionPlan.Name,
        GoalLabel       = GoalLabel(a.NutritionPlan.Goal),
        AssignedAt      = a.AssignedAt,
        StartDate       = a.StartDate,
        EndDate         = a.EndDate,
        IsActive        = a.IsActive,
        Notes           = a.Notes
    };

    public async Task<MemberNutritionPlan> AssignAsync(AssignNutritionViewModel vm, string assignedBy)
    {
        // Deactivate previous active plans for this member
        var prev = await _db.MemberNutritionPlans
            .Where(a => a.MemberId == vm.MemberId && a.IsActive)
            .ToListAsync();

        foreach (var p in prev)
        {
            p.IsActive = false;
            p.EndDate  = DateTime.Today;
        }

        var assignment = new MemberNutritionPlan
        {
            MemberId        = vm.MemberId,
            NutritionPlanId = vm.NutritionPlanId,
            AssignedAt      = DateTime.UtcNow,
            StartDate       = vm.StartDate,
            EndDate         = vm.EndDate,
            IsActive        = true,
            Notes           = vm.Notes,
            AssignedBy      = assignedBy
        };

        _db.MemberNutritionPlans.Add(assignment);
        await _db.SaveChangesAsync();
        return assignment;
    }

    public async Task<bool> DeactivateAssignmentAsync(int id)
    {
        var a = await _db.MemberNutritionPlans.FindAsync(id);
        if (a == null) return false;
        a.IsActive = false;
        a.EndDate  ??= DateTime.Today;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<NutritionPrintViewModel?> GetPrintDataAsync(int memberNutritionPlanId)
    {
        var a = await _db.MemberNutritionPlans
            .Include(x => x.Member)
            .Include(x => x.NutritionPlan)
                .ThenInclude(p => p.Meals.OrderBy(m => m.OrderIndex))
            .FirstOrDefaultAsync(x => x.Id == memberNutritionPlanId);

        if (a == null) return null;

        return new NutritionPrintViewModel
        {
            Company       = await _company.GetAsync(),
            Member        = a.Member,
            Assignment    = a,
            Plan          = a.NutritionPlan,
            GoalLabel     = GoalLabel(a.NutritionPlan.Goal),
            DurationLabel = DurationLabel(a.NutritionPlan.DurationDays)
        };
    }
}
