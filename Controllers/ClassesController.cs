using FitManager.Models;
using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.Controllers;

[Authorize]
public class ClassesController : Controller
{
    private readonly IClassService   _classService;
    private readonly IMemberService  _memberService;

    public ClassesController(IClassService classService, IMemberService memberService)
    {
        _classService  = classService;
        _memberService = memberService;
    }

    // ── GET /Classes ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var list = await _classService.GetAllAsync();
        return View(list);
    }

    // ── GET /Classes/Calendar ─────────────────────────────────────────────────
    public async Task<IActionResult> Calendar(DateTime? week)
    {
        var start = week ?? DateTime.Today;
        // Normalizar al lunes de esa semana
        while (start.DayOfWeek != DayOfWeek.Monday)
            start = start.AddDays(-1);

        var vm = await _classService.GetCalendarWeekAsync(start);
        return View(vm);
    }

    // ── GET /Classes/Create ───────────────────────────────────────────────────
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create()
    {
        var vm = new FitnessClassViewModel { StartDate = DateTime.Today };
        vm.Categories = await GetCategorySelectListAsync();
        return View(vm);
    }

    // ── POST /Classes/Create ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Create(FitnessClassViewModel vm)
    {
        // Eliminar horarios vacíos antes de validar
        vm.Schedules = vm.Schedules
            .Where(s => s.ToDaysOfWeekFlags() != DaysOfWeekFlags.None
                     || s.DayOfMonth.HasValue
                     || s.SpecificDate.HasValue)
            .ToList();

        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectListAsync();
            return View(vm);
        }

        var fc = await _classService.CreateAsync(vm, User.Identity!.Name!);
        TempData["Success"] = $"Clase «{fc.Name}» creada exitosamente.";
        return RedirectToAction(nameof(Detail), new { id = fc.Id });
    }

    // ── GET /Classes/Edit/5 ───────────────────────────────────────────────────
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id)
    {
        var fc = await _classService.GetByIdAsync(id);
        if (fc == null) return NotFound();

        var vm = new FitnessClassViewModel
        {
            Id              = fc.Id,
            Name            = fc.Name,
            Description     = fc.Description,
            ClassCategoryId = fc.ClassCategoryId,
            InstructorName  = fc.InstructorName,
            Location        = fc.Location,
            Color           = fc.Color,
            StartDate       = fc.StartDate,
            EndDate         = fc.EndDate,
            MaxCapacity     = fc.MaxCapacity,
            IsActive        = fc.IsActive,
            Schedules       = fc.Schedules.Select(ClassScheduleViewModel.FromModel).ToList()
        };

        if (!vm.Schedules.Any())
            vm.Schedules.Add(new ClassScheduleViewModel());

        vm.Categories = await GetCategorySelectListAsync();
        return View(vm);
    }

    // ── POST /Classes/Edit/5 ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Edit(int id, FitnessClassViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        vm.Schedules = vm.Schedules
            .Where(s => s.ToDaysOfWeekFlags() != DaysOfWeekFlags.None
                     || s.DayOfMonth.HasValue
                     || s.SpecificDate.HasValue)
            .ToList();

        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectListAsync();
            return View(vm);
        }

        var result = await _classService.UpdateAsync(vm);
        if (result == null) return NotFound();

        TempData["Success"] = "Clase actualizada correctamente.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ── GET /Classes/Detail/5 ─────────────────────────────────────────────────
    public async Task<IActionResult> Detail(int id)
    {
        var vm = await _classService.GetDetailAsync(id);
        if (vm == null) return NotFound();

        ViewBag.Members = await GetMemberSelectListAsync();
        return View(vm);
    }

    // ── POST /Classes/Enroll ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Enroll(ClassEnrollmentViewModel vm)
    {
        var (ok, message) = await _classService.EnrollAsync(vm, User.Identity!.Name!);
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Detail), new { id = vm.FitnessClassId });
    }

    // ── POST /Classes/Unenroll ────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Unenroll(int enrollmentId, int classId)
    {
        await _classService.UnenrollAsync(enrollmentId);
        TempData["Success"] = "Inscripción cancelada.";
        return RedirectToAction(nameof(Detail), new { id = classId });
    }

    // ── POST /Classes/ToggleActive ────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _classService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Classes/Delete ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        await _classService.DeleteAsync(id);
        TempData["Success"] = "Clase eliminada.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<List<SelectListItem>> GetCategorySelectListAsync()
    {
        var cats = await _classService.GetAllCategoriesAsync();
        return cats
            .Where(c => c.IsActive)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
    }

    private async Task<List<SelectListItem>> GetMemberSelectListAsync()
    {
        var members = await _memberService.GetAllAsync();
        return members
            .Where(m => m.Status == MemberStatus.Active)
            .OrderBy(m => m.LastName)
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text  = $"{m.FullName} ({m.MemberNumber})"
            })
            .ToList();
    }
}
