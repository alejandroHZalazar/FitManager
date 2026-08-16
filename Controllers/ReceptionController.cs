using FitManager.Data;
using FitManager.Models;
using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

[AllowAnonymous]
public class ReceptionController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IMemberService       _memberService;
    private readonly ICompanyService      _companyService;

    public ReceptionController(ApplicationDbContext db,
                               IMemberService memberService,
                               ICompanyService companyService)
    {
        _db             = db;
        _memberService  = memberService;
        _companyService = companyService;
    }

    // GET: /Reception
    public async Task<IActionResult> Index()
    {
        var settings = await _companyService.GetAsync();
        var vm = new ReceptionViewModel
        {
            ModalSeconds = settings.ReceptionModalSeconds,
            CompanyName  = settings.Name,
            LogoPath     = settings.LogoPath
        };
        return View(vm);
    }

    // GET: /Reception/Lookup?dni=xxxx  →  JSON
    public async Task<IActionResult> Lookup(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
            return Json(new { found = false, message = "DNI vacío." });

        dni = dni.Trim();

        var member = await _memberService.GetByDniAsync(dni);
        if (member == null || member.Status != MemberStatus.Active)
        {
            return Json(new
            {
                found   = false,
                message = "No se encontraron los datos del miembro. Por favor, comunicate con el administrador del gimnasio."
            });
        }

        // Pago vigente: mismo criterio que AlertsController.Expirations —
        // pago con DueDate más lejano (independiente del status).
        // Se trae también el nombre del Plan asociado en la misma consulta.
        var latestPayment = await _db.Payments
            .Where(p => p.MemberId == member.Id)
            .OrderByDescending(p => p.DueDate)
            .Select(p => new
            {
                p.DueDate,
                PlanName = p.Plan != null ? p.Plan.Name : null
            })
            .FirstOrDefaultAsync();

        DateTime? dueDate  = latestPayment?.DueDate;
        string?   planName = latestPayment?.PlanName;
        var status = BuildStatus(dueDate);

        return Json(new
        {
            found        = true,
            memberName   = member.FullName,
            memberNumber = member.MemberNumber,
            planName,
            status.Level,
            status.Message,
            endDate      = dueDate?.ToString("dd/MM/yyyy")
        });
    }

    private static (string Level, string Message) BuildStatus(DateTime? dueDate)
    {
        if (dueDate == null)
            return ("expired", "Sin suscripción registrada");

        var days = (dueDate.Value.Date - DateTime.Today).Days;
        if (days < 0)  return ("expired",  "Suscripción vencida");
        if (days <= 2) return ("critical", days == 0
                                            ? "Tu suscripción vence hoy"
                                            : $"Faltan {days} día(s) para el vencimiento (menos de 3)");
        if (days <= 5) return ("warning",  $"Faltan {days} días para el vencimiento (menos de 5)");
        return ("ok", "Suscripción al día");
    }
}
