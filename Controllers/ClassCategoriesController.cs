using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize(Roles = "Administrador")]
public class ClassCategoriesController : Controller
{
    private readonly IClassService _classService;

    public ClassCategoriesController(IClassService classService)
        => _classService = classService;

    // ── GET /ClassCategories ──────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var list = await _classService.GetAllCategoriesAsync();
        return View(list);
    }

    // ── GET /ClassCategories/Create ───────────────────────────────────────────
    public IActionResult Create()
        => View(new ClassCategoryViewModel());

    // ── POST /ClassCategories/Create ──────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassCategoryViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var cat = await _classService.CreateCategoryAsync(vm);
        TempData["Success"] = $"Categoría «{cat.Name}» creada exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /ClassCategories/Edit/5 ───────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var cat = await _classService.GetCategoryByIdAsync(id);
        if (cat == null) return NotFound();

        var vm = new ClassCategoryViewModel
        {
            Id          = cat.Id,
            Name        = cat.Name,
            Description = cat.Description,
            Color       = cat.Color,
            Icon        = cat.Icon,
            OrderIndex  = cat.OrderIndex,
            IsActive    = cat.IsActive
        };
        return View(vm);
    }

    // ── POST /ClassCategories/Edit/5 ──────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClassCategoryViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var result = await _classService.UpdateCategoryAsync(vm);
        if (result == null) return NotFound();

        TempData["Success"] = "Categoría actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /ClassCategories/Delete ──────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (ok, error) = await _classService.DeleteCategoryAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Categoría eliminada." : error;
        return RedirectToAction(nameof(Index));
    }

    // ── POST /ClassCategories/ToggleActive ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _classService.ToggleCategoryActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
