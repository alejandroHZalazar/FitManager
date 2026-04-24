using FitManager.Models;
using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize]
public class ExercisesController : Controller
{
    private readonly IRoutineService _routineService;
    public ExercisesController(IRoutineService routineService) => _routineService = routineService;

    // ── GET /Exercises ────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(MuscleGroup? muscle, ExerciseType? type)
    {
        var list = await _routineService.GetAllExercisesAsync(muscle, type);
        ViewBag.SelectedMuscle = muscle;
        ViewBag.SelectedType   = type;
        return View(list);
    }

    // ── GET /Exercises/Create ─────────────────────────────────────────────────
    [Authorize(Roles = "Administrador,Recepcionista")]
    public IActionResult Create() => View(new ExerciseViewModel());

    // ── POST /Exercises/Create ────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(ExerciseViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var ex = await _routineService.CreateExerciseAsync(vm);
        TempData["Success"] = $"Ejercicio «{ex.Name}» agregado a la biblioteca.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Exercises/Edit/5 ─────────────────────────────────────────────────
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id)
    {
        var ex = await _routineService.GetExerciseByIdAsync(id);
        if (ex == null) return NotFound();
        return View(new ExerciseViewModel
        {
            Id           = ex.Id,
            Name         = ex.Name,
            Description  = ex.Description,
            MuscleGroup  = ex.MuscleGroup,
            ExerciseType = ex.ExerciseType,
            IsActive     = ex.IsActive
        });
    }

    // ── POST /Exercises/Edit/5 ────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id, ExerciseViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);
        var result = await _routineService.UpdateExerciseAsync(vm);
        if (result == null) return NotFound();
        TempData["Success"] = "Ejercicio actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Exercises/Delete ────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, error) = await _routineService.DeleteExerciseAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Ejercicio eliminado." : error;
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Exercises/ToggleActive ──────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _routineService.ToggleExerciseActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
