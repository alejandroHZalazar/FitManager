using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize]
public class NutritionPlansController : Controller
{
    private readonly INutritionService _nutritionService;
    public NutritionPlansController(INutritionService nutritionService)
        => _nutritionService = nutritionService;

    // GET /NutritionPlans
    public async Task<IActionResult> Index()
    {
        var list = await _nutritionService.GetAllPlansAsync();
        return View(list);
    }

    // GET /NutritionPlans/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        var vm = await _nutritionService.GetPlanDetailAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // GET /NutritionPlans/Create
    [Authorize(Roles = "Administrador,Recepcionista,Instructor")]
    public IActionResult Create()
        => View(new NutritionPlanEditViewModel());

    // POST /NutritionPlans/Create
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista,Instructor")]
    public async Task<IActionResult> Create(NutritionPlanEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var plan = await _nutritionService.CreatePlanAsync(vm, User.Identity!.Name!);
        TempData["Success"] = $"Plan «{plan.Name}» creado exitosamente.";
        return RedirectToAction(nameof(Detail), new { id = plan.Id });
    }

    // GET /NutritionPlans/Edit/5
    [Authorize(Roles = "Administrador,Recepcionista,Instructor")]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _nutritionService.GetPlanEditAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // POST /NutritionPlans/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista,Instructor")]
    public async Task<IActionResult> Edit(int id, NutritionPlanEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);
        var result = await _nutritionService.UpdatePlanAsync(vm);
        if (result == null) return NotFound();
        TempData["Success"] = "Plan actualizado correctamente.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // POST /NutritionPlans/Delete
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        await _nutritionService.DeletePlanAsync(id);
        TempData["Success"] = "Plan eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // POST /NutritionPlans/ToggleActive
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _nutritionService.TogglePlanActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
