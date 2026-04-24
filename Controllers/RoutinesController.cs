using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize]
public class RoutinesController : Controller
{
    private readonly IRoutineService _routineService;
    public RoutinesController(IRoutineService routineService) => _routineService = routineService;

    // ── GET /Routines ─────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var list = await _routineService.GetAllRoutinesAsync();
        return View(list);
    }

    // ── GET /Routines/Detail/5 ────────────────────────────────────────────────
    public async Task<IActionResult> Detail(int id)
    {
        var vm = await _routineService.GetRoutineDetailAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // ── GET /Routines/Create ──────────────────────────────────────────────────
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create()
    {
        var vm = new RoutineEditViewModel();
        vm.ExerciseOptions = await _routineService.GetExerciseSelectListAsync();
        return View(vm);
    }

    // ── POST /Routines/Create ─────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(RoutineEditViewModel vm)
    {
        ModelState.Remove(nameof(vm.ExerciseOptions));
        if (!ModelState.IsValid)
        {
            vm.ExerciseOptions = await _routineService.GetExerciseSelectListAsync();
            return View(vm);
        }
        var r = await _routineService.CreateRoutineAsync(vm, User.Identity!.Name!);
        TempData["Success"] = $"Rutina «{r.Name}» creada exitosamente.";
        return RedirectToAction(nameof(Detail), new { id = r.Id });
    }

    // ── GET /Routines/Edit/5 ──────────────────────────────────────────────────
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _routineService.GetRoutineEditAsync(id);
        if (vm == null) return NotFound();
        vm.ExerciseOptions = await _routineService.GetExerciseSelectListAsync();
        return View(vm);
    }

    // ── POST /Routines/Edit/5 ─────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id, RoutineEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        ModelState.Remove(nameof(vm.ExerciseOptions));
        if (!ModelState.IsValid)
        {
            vm.ExerciseOptions = await _routineService.GetExerciseSelectListAsync();
            return View(vm);
        }
        var result = await _routineService.UpdateRoutineAsync(vm);
        if (result == null) return NotFound();
        TempData["Success"] = "Rutina actualizada correctamente.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ── POST /Routines/Delete ─────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        await _routineService.DeleteRoutineAsync(id);
        TempData["Success"] = "Rutina eliminada.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Routines/ToggleActive ───────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _routineService.ToggleRoutineActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
