using FitManager.Models;
using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize]
public class MembersController : Controller
{
    private readonly IMemberService _memberService;
    private readonly IPaymentService _paymentService;

    public MembersController(IMemberService memberService, IPaymentService paymentService)
    {
        _memberService = memberService;
        _paymentService = paymentService;
    }

    // GET: /Members
    public async Task<IActionResult> Index()
    {
        var members = await _memberService.GetAllAsync();
        return View(members);
    }

    // GET: /Members/Create
    public IActionResult Create() => View(new MemberViewModel { MembershipStartDate = DateTime.Today });

    // POST: /Members/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (!string.IsNullOrWhiteSpace(vm.DNI) && await _memberService.ExistsDNIAsync(vm.DNI))
        {
            ModelState.AddModelError(nameof(vm.DNI), "Ya existe un socio con ese DNI.");
            return View(vm);
        }

        await _memberService.CreateAsync(vm);
        TempData["Success"] = "Socio creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Members/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        var vm = MapToViewModel(member);
        return View(vm);
    }

    // POST: /Members/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MemberViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        if (!string.IsNullOrWhiteSpace(vm.DNI) && await _memberService.ExistsDNIAsync(vm.DNI, vm.Id))
        {
            ModelState.AddModelError(nameof(vm.DNI), "Ya existe un socio con ese DNI.");
            return View(vm);
        }

        await _memberService.UpdateAsync(vm);
        TempData["Success"] = "Socio actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Members/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        var vm = MapToViewModel(member);
        return View(vm);
    }

    // GET: /Members/Payments/5
    public async Task<IActionResult> Payments(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        var payments = await _paymentService.GetByMemberAsync(id);
        var (paid, pending) = await _paymentService.GetTotalsAsync(id);

        var model = new MemberPaymentsViewModel
        {
            Member = MapToViewModel(member),
            Payments = payments,
            TotalPaid = paid,
            TotalPending = pending,
            NewPayment = new PaymentViewModel
            {
                MemberId = id,
                MemberName = member.FullName,
                MemberNumber = member.MemberNumber,
                PaymentDate = DateTime.Today,
                DueDate = DateTime.Today.AddMonths(1)
            }
        };

        return View(model);
    }

    // POST: /Members/AddPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(PaymentViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos de pago inválidos.";
            return RedirectToAction(nameof(Payments), new { id = vm.MemberId });
        }

        var payment = await _paymentService.CreateAsync(vm, User.Identity?.Name ?? "system");
        TempData["Success"]      = "Pago registrado exitosamente.";
        TempData["NewPaymentId"] = payment.Id;
        return RedirectToAction(nameof(Payments), new { id = vm.MemberId });
    }

    // POST: /Members/DeletePayment/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeletePayment(int paymentId, int memberId)
    {
        await _paymentService.DeleteAsync(paymentId);
        TempData["Success"] = "Pago eliminado.";
        return RedirectToAction(nameof(Payments), new { id = memberId });
    }

    // POST: /Members/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _memberService.DeleteAsync(id);
        if (deleted)
            TempData["Success"] = "Socio eliminado correctamente.";
        else
            TempData["Error"] = "No se pudo eliminar el socio.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Members/ToggleStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        var vm = MapToViewModel(member);
        vm.Status = member.Status == MemberStatus.Active ? MemberStatus.Inactive : MemberStatus.Active;
        await _memberService.UpdateAsync(vm);

        var statusText = vm.Status == MemberStatus.Active ? "activado" : "desactivado";
        TempData["Success"] = $"Socio {statusText} correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Members/SearchJson?q=garcia
    [HttpGet]
    public async Task<IActionResult> SearchJson(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var term = q.Trim().ToLower();

        var results = await _memberService.GetAllAsync();
        var matches = results
            .Where(m => m.Status == MemberStatus.Active &&
                        (m.FullName.ToLower().Contains(term) ||
                         m.MemberNumber.ToLower().Contains(term) ||
                         (!string.IsNullOrEmpty(m.DNI) && m.DNI.Contains(term))))
            .Take(10)
            .Select(m => new
            {
                id     = m.Id,
                text   = $"{m.LastName}, {m.FirstName}",
                number = m.MemberNumber,
                photo  = m.PhotoPath
            });

        return Json(matches);
    }

    private static MemberViewModel MapToViewModel(Member m) => new()
    {
        Id = m.Id,
        MemberNumber = m.MemberNumber,
        FirstName = m.FirstName,
        LastName = m.LastName,
        Email = m.Email,
        Phone = m.Phone,
        Address = m.Address,
        DateOfBirth = m.DateOfBirth,
        DNI = m.DNI,
        Status = m.Status,
        MembershipStartDate = m.MembershipStartDate,
        MembershipEndDate = m.MembershipEndDate,
        Notes = m.Notes,
        PhotoPath = m.PhotoPath
    };
}
