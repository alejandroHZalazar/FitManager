using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitManager.Controllers;

[Authorize]
public class MemberRoutinesController : Controller
{
    private readonly IRoutineService _routineService;
    private readonly IMemberService  _memberService;

    public MemberRoutinesController(IRoutineService routineService, IMemberService memberService)
    {
        _routineService = routineService;
        _memberService  = memberService;
    }

    // ── GET /MemberRoutines ───────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var list = await _routineService.GetAllAssignmentsAsync();
        return View(list);
    }

    // ── GET /MemberRoutines/History/5 ─────────────────────────────────────────
    public async Task<IActionResult> History(int memberId)
    {
        var member = await _memberService.GetByIdAsync(memberId);
        if (member == null) return NotFound();

        var history = await _routineService.GetMemberHistoryAsync(memberId);
        ViewBag.Member = member;
        return View(history);
    }

    // ── GET /MemberRoutines/Assign ────────────────────────────────────────────
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Assign(int? memberId)
    {
        var vm = new AssignRoutineViewModel { StartDate = DateTime.Today };
        vm.RoutineOptions = await GetRoutineSelectListAsync();

        if (memberId.HasValue)
        {
            vm.MemberId = memberId.Value;
            ViewBag.PreselectedMember = await _memberService.GetByIdAsync(memberId.Value);
        }

        return View(vm);
    }

    // ── POST /MemberRoutines/Assign ───────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Assign(AssignRoutineViewModel vm)
    {
        ModelState.Remove(nameof(vm.RoutineOptions));
        if (!ModelState.IsValid)
        {
            vm.RoutineOptions = await GetRoutineSelectListAsync();
            return View(vm);
        }

        var assignment = await _routineService.AssignAsync(vm, User.Identity!.Name!);
        TempData["Success"] = "Rutina asignada exitosamente.";
        return RedirectToAction(nameof(History), new { memberId = vm.MemberId });
    }

    // ── POST /MemberRoutines/Deactivate ───────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> Deactivate(int id, int memberId)
    {
        await _routineService.DeactivateAssignmentAsync(id);
        TempData["Success"] = "Asignación finalizada.";
        return RedirectToAction(nameof(History), new { memberId });
    }

    // ── GET /MemberRoutines/Print/5 ───────────────────────────────────────────
    public async Task<IActionResult> Print(int id)
    {
        var vm = await _routineService.GetPrintDataAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private async Task<List<SelectListItem>> GetRoutineSelectListAsync()
    {
        var routines = await _routineService.GetAllRoutinesAsync();
        return routines
            .Where(r => r.IsActive)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text  = $"{r.Name} ({r.GoalLabel} — {r.DurationWeeks} sem.)"
            })
            .ToList();
    }
}
