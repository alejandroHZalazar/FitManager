using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize]
public class CashRegisterController : Controller
{
    private readonly ICashRegisterService _cashService;

    public CashRegisterController(ICashRegisterService cashService)
    {
        _cashService = cashService;
    }

    // GET: /CashRegister  → today's register (or open form)
    public async Task<IActionResult> Index()
    {
        var detail = await _cashService.GetTodayDetailAsync();
        return View(detail);
    }

    // GET: /CashRegister/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        var detail = await _cashService.GetDetailAsync(id);
        return View(detail);
    }

    // GET: /CashRegister/History
    public async Task<IActionResult> History()
    {
        var registers = await _cashService.GetHistoryAsync(60);
        return View(registers);
    }

    // POST: /CashRegister/Open
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(OpenCashRegisterViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos inválidos para abrir la caja.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _cashService.OpenAsync(vm, User.Identity?.Name ?? "system");
            TempData["Success"] = "Caja abierta exitosamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /CashRegister/Close
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Close(CloseCashRegisterViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos inválidos para cerrar la caja.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _cashService.CloseAsync(vm, User.Identity?.Name ?? "system");
            TempData["Success"] = "Caja cerrada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /CashRegister/Reopen
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Reopen(ReopenCashRegisterViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Ingresá el motivo de la reapertura.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var register = await _cashService.ReopenAsync(vm, User.Identity?.Name ?? "system");
            TempData["Success"] = "Caja reabierta correctamente.";
            // Si la caja reabierta es de hoy, volvemos al Index; si es de otro día, al Detail
            if (register.Date.Date == DateTime.Today)
                return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Detail), new { id = register.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(History));
        }
    }

    // POST: /CashRegister/AddTransaction
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTransaction(CashTransactionFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos de transacción inválidos.";
            return RedirectToAction(nameof(Detail), new { id = vm.CashRegisterId });
        }

        await _cashService.AddTransactionAsync(vm, User.Identity?.Name ?? "system");
        TempData["Success"] = "Transacción registrada.";
        return RedirectToAction(nameof(Detail), new { id = vm.CashRegisterId });
    }

    // POST: /CashRegister/DeleteTransaction/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteTransaction(int id, int registerId)
    {
        await _cashService.DeleteTransactionAsync(id);
        TempData["Success"] = "Transacción eliminada.";
        return RedirectToAction(nameof(Detail), new { id = registerId });
    }
}
