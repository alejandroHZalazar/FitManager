using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FitManager.Controllers;

[Authorize(Roles = "Administrador")]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly IWebHostEnvironment _env;

    public CompanyController(ICompanyService companyService, IWebHostEnvironment env)
    {
        _companyService = companyService;
        _env            = env;
    }

    // ── GET /Company/Settings ─────────────────────────────────────────────────
    public async Task<IActionResult> Settings()
    {
        var settings = await _companyService.GetAsync();
        var events   = await _companyService.GetWhatsAppEventsAsync();

        var vm = new CompanySettingsViewModel
        {
            Name            = settings.Name,
            Slogan          = settings.Slogan,
            Address         = settings.Address,
            City            = settings.City,
            Province        = settings.Province,
            Country         = settings.Country,
            TaxId           = settings.TaxId,
            Phone           = settings.Phone,
            Phone2          = settings.Phone2,
            Email           = settings.Email,
            Website         = settings.Website,
            Notes           = settings.Notes,
            CurrentLogoPath = settings.LogoPath,
            WhatsAppEnabled = settings.WhatsAppEnabled,
            WhatsAppApiKey  = settings.WhatsAppApiKey,
            WhatsAppApiUrl  = settings.WhatsAppApiUrl,
            ReceptionModalSeconds = settings.ReceptionModalSeconds,
            WhatsAppEvents  = events.Select(e => new WhatsAppEventViewModel
            {
                Id          = e.Id,
                Label       = e.Label,
                EventName   = e.EventName,
                SystemEvent = (int)e.SystemEvent,
                IsActive    = e.IsActive
            }).ToList()
        };

        return View(vm);
    }

    // ── POST /Company/Settings ────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(CompanySettingsViewModel vm)
    {
        ModelState.Remove(nameof(vm.LogoFile));
        ModelState.Remove(nameof(vm.CurrentLogoPath));

        if (!ModelState.IsValid)
        {
            var current = await _companyService.GetAsync();
            vm.CurrentLogoPath = current.LogoPath;
            vm.WhatsAppEvents  = (await _companyService.GetWhatsAppEventsAsync())
                .Select(e => new WhatsAppEventViewModel
                {
                    Id = e.Id, Label = e.Label, EventName = e.EventName,
                    SystemEvent = (int)e.SystemEvent, IsActive = e.IsActive
                }).ToList();
            return View(vm);
        }

        try
        {
            await _companyService.UpdateAsync(vm, _env.WebRootPath);
            TempData["Success"] = "Configuración guardada correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(vm.LogoFile), ex.Message);
            var current = await _companyService.GetAsync();
            vm.CurrentLogoPath = current.LogoPath;
            vm.WhatsAppEvents  = (await _companyService.GetWhatsAppEventsAsync())
                .Select(e => new WhatsAppEventViewModel
                {
                    Id = e.Id, Label = e.Label, EventName = e.EventName,
                    SystemEvent = (int)e.SystemEvent, IsActive = e.IsActive
                }).ToList();
            return View(vm);
        }

        return RedirectToAction(nameof(Settings));
    }

    // ── POST /Company/DeleteLogo ──────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogo()
    {
        await _companyService.DeleteLogoAsync(_env.WebRootPath);
        TempData["Success"] = "Logotipo eliminado.";
        return RedirectToAction(nameof(Settings));
    }

    // ── AJAX: POST /Company/UpdateWhatsAppEvent ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateWhatsAppEvent([FromBody] WhatsAppEventViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.EventName))
            return Json(new { success = false, error = "El nombre del evento es obligatorio." });

        var evt = await _companyService.UpdateWhatsAppEventAsync(vm.Id, vm);
        if (evt == null) return Json(new { success = false, error = "Evento no encontrado." });
        return Json(new { success = true });
    }

    // ── AJAX: POST /Company/ToggleWhatsAppEvent ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleWhatsAppEvent([FromBody] int id)
    {
        var ok = await _companyService.ToggleWhatsAppEventAsync(id);
        return Json(new { success = ok });
    }
}
