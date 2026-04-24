using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            CurrentLogoPath = settings.LogoPath
        };

        return View(vm);
    }

    // ── POST /Company/Settings ────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(CompanySettingsViewModel vm)
    {
        // IFormFile no participa de la validación de modelo
        ModelState.Remove(nameof(vm.LogoFile));
        ModelState.Remove(nameof(vm.CurrentLogoPath));

        if (!ModelState.IsValid)
        {
            var current = await _companyService.GetAsync();
            vm.CurrentLogoPath = current.LogoPath;
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
}
