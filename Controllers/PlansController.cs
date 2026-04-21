using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize(Roles = "Administrador")]
public class PlansController : Controller
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    public async Task<IActionResult> Index()
    {
        var plans = await _planService.GetAllAsync();
        return View(plans);
    }

    public IActionResult Create() => View(new PlanViewModel { IsActive = true, DurationDays = 30 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlanViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await _planService.CreateAsync(vm);
        TempData["Success"] = "Plan creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var plan = await _planService.GetByIdAsync(id);
        if (plan == null) return NotFound();

        return View(new PlanViewModel
        {
            Id           = plan.Id,
            Name         = plan.Name,
            Description  = plan.Description,
            Price        = plan.Price,
            PlanType     = plan.PlanType,
            DurationDays = plan.DurationDays,
            IsActive     = plan.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlanViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);
        await _planService.UpdateAsync(vm);
        TempData["Success"] = "Plan actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _planService.DeleteAsync(id);
        TempData["Success"] = "Plan eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
