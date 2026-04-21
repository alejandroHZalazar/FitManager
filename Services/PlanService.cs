using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class PlanService : IPlanService
{
    private readonly ApplicationDbContext _db;
    public PlanService(ApplicationDbContext db) => _db = db;

    public async Task<List<Plan>> GetAllAsync(bool activeOnly = false)
    {
        var q = _db.Plans.AsQueryable();
        if (activeOnly) q = q.Where(p => p.IsActive);
        return await q.OrderBy(p => p.PlanType).ThenBy(p => p.Price).ToListAsync();
    }

    public async Task<Plan?> GetByIdAsync(int id) => await _db.Plans.FindAsync(id);

    public async Task<Plan> CreateAsync(PlanViewModel vm)
    {
        var plan = Map(new Plan(), vm);
        plan.CreatedAt = DateTime.UtcNow;
        _db.Plans.Add(plan);
        await _db.SaveChangesAsync();
        return plan;
    }

    public async Task<Plan> UpdateAsync(PlanViewModel vm)
    {
        var plan = await _db.Plans.FindAsync(vm.Id)
            ?? throw new KeyNotFoundException("Plan no encontrado");
        Map(plan, vm);
        await _db.SaveChangesAsync();
        return plan;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var plan = await _db.Plans.FindAsync(id);
        if (plan == null) return false;
        var inUse = await _db.MemberPlans.AnyAsync(mp => mp.PlanId == id);
        if (inUse) { plan.IsActive = false; await _db.SaveChangesAsync(); return true; }
        _db.Plans.Remove(plan);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SelectListItem>> GetSelectListAsync(bool activeOnly = true)
    {
        var plans = await GetAllAsync(activeOnly);
        return plans.Select(p => new SelectListItem(
            $"{p.Name} — ${p.Price:N0} ({p.DurationDays}d)",
            p.Id.ToString()));
    }

    private static Plan Map(Plan e, PlanViewModel vm)
    {
        e.Name = vm.Name;
        e.Description = vm.Description;
        e.Price = vm.Price;
        e.PlanType = vm.PlanType;
        e.DurationDays = vm.DurationDays;
        e.IsActive = vm.IsActive;
        return e;
    }
}
