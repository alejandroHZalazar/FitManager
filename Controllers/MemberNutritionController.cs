using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize]
public class MemberNutritionController : Controller
{
    private readonly INutritionService _nutritionService;
    private readonly IMemberService    _memberService;

    public MemberNutritionController(INutritionService nutritionService, IMemberService memberService)
    {
        _nutritionService = nutritionService;
        _memberService    = memberService;
    }

    // GET /MemberNutrition
    public async Task<IActionResult> Index()
    {
        var list = await _nutritionService.GetAllAssignmentsAsync();
        return View(list);
    }

    // GET /MemberNutrition/History?memberId=5
    public async Task<IActionResult> History(int memberId)
    {
        var member = await _memberService.GetByIdAsync(memberId);
        if (member == null) return NotFound();
        var history = await _nutritionService.GetMemberHistoryAsync(memberId);
        ViewBag.Member = member;
        return View(history);
    }

    // GET /MemberNutrition/Assign?memberId=5
    [Authorize(Roles = "Administrador,Recepcionista,Instructor")]
    public async Task<IActionResult> Assign(int? memberId)
    {
        var vm = new AssignNutritionViewModel { StartDate = DateTime.Today };
        vm.PlanOptions = await _nutritionService.GetPlanSelectListAsync();

        if (memberId.HasValue)
        {
            vm.MemberId = memberId.Value;
            ViewBag.PreselectedMember = await _memberService.GetByIdAsync(memberId.Value);
        }

        return View(vm);
    }

    // POST /MemberNutrition/Assign
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista,Instructor")]
    public async Task<IActionResult> Assign(AssignNutritionViewModel vm)
    {
        ModelState.Remove(nameof(vm.PlanOptions));
        if (!ModelState.IsValid)
        {
            vm.PlanOptions = await _nutritionService.GetPlanSelectListAsync();
            return View(vm);
        }
        await _nutritionService.AssignAsync(vm, User.Identity!.Name!);
        TempData["Success"] = "Plan de nutrición asignado exitosamente.";
        return RedirectToAction(nameof(History), new { memberId = vm.MemberId });
    }

    // POST /MemberNutrition/Deactivate
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador,Recepcionista,Instructor")]
    public async Task<IActionResult> Deactivate(int id, int memberId)
    {
        await _nutritionService.DeactivateAssignmentAsync(id);
        TempData["Success"] = "Asignación finalizada.";
        return RedirectToAction(nameof(History), new { memberId });
    }

    // GET /MemberNutrition/Print/5
    public async Task<IActionResult> Print(int id)
    {
        var vm = await _nutritionService.GetPrintDataAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }
}
