using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitManager.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService  _reports;
    private readonly ICompanyService _company;

    public ReportsController(IReportService reports, ICompanyService company)
    {
        _reports = reports;
        _company = company;
    }

    // ── Index / Hub ───────────────────────────────────────────────────────────
    public IActionResult Index() => View();

    // ── Ingresos ──────────────────────────────────────────────────────────────
    public async Task<IActionResult> Revenue(ReportFilterViewModel? filter)
    {
        filter = NormalizeFilter(filter);
        ViewBag.Company = await _company.GetAsync();
        var vm = await _reports.GetRevenueReportAsync(filter);
        return View(vm);
    }

    // ── Socios activos ────────────────────────────────────────────────────────
    public async Task<IActionResult> ActiveMembers(ReportFilterViewModel? filter)
    {
        filter = NormalizeFilter(filter);
        ViewBag.Company = await _company.GetAsync();
        var vm = await _reports.GetActiveMembersReportAsync(filter);
        return View(vm);
    }

    // ── Deudores ──────────────────────────────────────────────────────────────
    public async Task<IActionResult> Debtors(ReportFilterViewModel? filter)
    {
        filter = NormalizeFilter(filter);
        ViewBag.Company = await _company.GetAsync();
        var vm = await _reports.GetDebtorsReportAsync(filter);
        return View(vm);
    }

    // ── Caja ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Cash(ReportFilterViewModel? filter)
    {
        filter = NormalizeFilter(filter);
        ViewBag.Company = await _company.GetAsync();
        var vm = await _reports.GetCashReportAsync(filter);
        return View(vm);
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static ReportFilterViewModel NormalizeFilter(ReportFilterViewModel? f)
    {
        f ??= new ReportFilterViewModel();
        if (f.Preset == DateRangePreset.Custom && f.From == null)
            f.From = DateTime.Today.AddDays(-29);
        if (f.Preset == DateRangePreset.Custom && f.To == null)
            f.To = DateTime.Today;
        return f;
    }
}
