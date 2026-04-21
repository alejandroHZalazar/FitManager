using FitManager.Data;
using FitManager.Models;
using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IPlanService _planService;
    private readonly ApplicationDbContext _db;

    public PaymentsController(IPaymentService paymentService, IPlanService planService, ApplicationDbContext db)
    {
        _paymentService = paymentService;
        _planService    = planService;
        _db             = db;
    }

    // GET: /Payments
    public async Task<IActionResult> Index(PaymentFilterViewModel? filter)
    {
        var payments = await _paymentService.GetAllAsync(filter);

        ViewBag.Members = new SelectList(
            await _db.Members.OrderBy(m => m.LastName).Select(m => new { m.Id, Name = m.LastName + ", " + m.FirstName }).ToListAsync(),
            "Id", "Name");
        ViewBag.Plans   = new SelectList(await _planService.GetAllAsync(), "Id", "Name");
        ViewBag.Filter  = filter ?? new PaymentFilterViewModel();

        return View(payments);
    }

    // GET: /Payments/Create
    public async Task<IActionResult> Create()
    {
        var vm = new GlobalPaymentViewModel
        {
            PaymentDate = DateTime.Today,
            DueDate     = DateTime.Today.AddMonths(1),
            Status      = PaymentStatus.Paid,
            Method      = PaymentMethod.Cash,
            Members     = await GetMembersSelectList(),
            Plans       = await _planService.GetSelectListAsync()
        };

        var allPlans = await _planService.GetAllAsync(true);
        ViewBag.PlanDataJson = System.Text.Json.JsonSerializer.Serialize(
            allPlans.Select(p => new { id = p.Id, price = p.Price, durationDays = p.DurationDays, name = p.Name }));

        return View(vm);
    }

    // POST: /Payments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GlobalPaymentViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Members = await GetMembersSelectList();
            vm.Plans   = await _planService.GetSelectListAsync();
            var allPlans = await _planService.GetAllAsync(true);
            ViewBag.PlanDataJson = System.Text.Json.JsonSerializer.Serialize(
                allPlans.Select(p => new { id = p.Id, price = p.Price, durationDays = p.DurationDays, name = p.Name }));
            return View(vm);
        }

        // Map GlobalPaymentViewModel → PaymentViewModel
        var pvm = new PaymentViewModel
        {
            MemberId      = vm.MemberId,
            PlanId        = vm.PlanId,
            Amount        = vm.Amount,
            PaymentDate   = vm.PaymentDate,
            DueDate       = vm.DueDate,
            Status        = vm.Status,
            Method        = vm.Method,
            Description   = vm.Description,
            Notes         = vm.Notes,
            ReceiptNumber = vm.ReceiptNumber
        };

        await _paymentService.CreateAsync(pvm, User.Identity?.Name ?? "system");
        TempData["Success"] = "Pago registrado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Payments/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        await _paymentService.DeleteAsync(id);
        TempData["Success"] = "Pago eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetMembersSelectList() =>
        (await _db.Members
            .Where(m => m.Status == MemberStatus.Active)
            .OrderBy(m => m.LastName)
            .ToListAsync())
        .Select(m => new SelectListItem($"{m.LastName}, {m.FirstName} — {m.MemberNumber}", m.Id.ToString()));
}
