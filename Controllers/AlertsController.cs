using FitManager.Data;
using FitManager.Models;
using FitManager.Services;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

[Authorize]
public class AlertsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWhatsAppService    _whatsApp;

    public AlertsController(ApplicationDbContext db, IWhatsAppService whatsApp)
    {
        _db       = db;
        _whatsApp = whatsApp;
    }

    // GET: /Alerts/Expirations
    public async Task<IActionResult> Expirations()
    {
        var today = DateTime.Today;
        var in5   = today.AddDays(5);

        // ── Fuente de verdad: Payments ───────────────────────────────────────
        // El "vencimiento" de un socio = DueDate del pago más lejano.
        // Se consideran pagos en cualquier estado (Paid/Pending/Overdue): el
        // DueDate marca el fin de la cobertura aunque el pago no esté cobrado.
        // El cobro se trata en /Alerts/Debts.
        var allPayments = await _db.Payments
            .Include(p => p.Member)
            .Include(p => p.Plan)
            .Where(p => p.Member!.Status == MemberStatus.Active)
            .ToListAsync();

        // Por socio, quedarse con el pago de DueDate más lejana.
        // Mientras el socio esté activo y no haya pagos posteriores, este pago
        // sigue marcando el vencimiento (aunque hayan pasado varios días).
        var latestPerMember = allPayments
            .GroupBy(p => p.MemberId)
            .Select(g => g.OrderByDescending(p => p.DueDate).First())
            .ToList();

        var vm = new AlertsViewModel
        {
            ExpiringToday = latestPerMember
                .Where(p => p.DueDate.Date == today)
                .OrderBy(p => p.Member!.LastName)
                .Select(ToAlert).ToList(),

            ExpiringIn5Days = latestPerMember
                .Where(p => p.DueDate.Date > today && p.DueDate.Date <= in5)
                .OrderBy(p => p.DueDate)
                .Select(ToAlert).ToList(),

            AlreadyExpired = latestPerMember
                .Where(p => p.DueDate.Date < today)
                .OrderBy(p => p.DueDate)
                .Select(ToAlert).ToList(),

            OverduePayments = new List<DebtAlert>()
        };

        return View(vm);
    }

    // GET: /Alerts/Debts
    public async Task<IActionResult> Debts()
    {
        var overduePayments = await _db.Payments
            .Include(p => p.Member)
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue)
            .OrderBy(p => p.DueDate)
            .ToListAsync();

        var debtAlerts = overduePayments
            .GroupBy(p => p.MemberId)
            .Select(g =>
            {
                var member   = g.First().Member!;
                var oldest   = g.Min(p => p.DueDate);
                var daysOver = (DateTime.Today - oldest).Days;
                return new DebtAlert
                {
                    MemberId      = member.Id,
                    MemberNumber  = member.MemberNumber,
                    MemberName    = member.FullName,
                    MemberPhoto   = member.PhotoPath,
                    TotalDebt     = g.Sum(p => p.Amount),
                    OverdueCount  = g.Count(),
                    OldestDueDate = oldest,
                    DaysOverdue   = daysOver > 0 ? daysOver : 0
                };
            })
            .OrderByDescending(d => d.DaysOverdue)
            .ToList();

        var vm = new AlertsViewModel
        {
            ExpiringToday   = new List<MemberPlanAlert>(),
            ExpiringIn5Days = new List<MemberPlanAlert>(),
            AlreadyExpired  = new List<MemberPlanAlert>(),
            OverduePayments = debtAlerts
        };

        return View(vm);
    }

    // ── AJAX: POST /Alerts/SendDebtNotice ────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendDebtNotice([FromBody] DebtNoticeRequest req)
    {
        var (ok, err) = await _whatsApp.SendDebtNoticeAsync(req.MemberId, req.EndDate);
        return Json(new { success = ok, error = err });
    }

    private static MemberPlanAlert ToAlert(Payment p) => new()
    {
        MemberId        = p.Member!.Id,
        MemberNumber    = p.Member.MemberNumber,
        MemberName      = p.Member.FullName,
        MemberPhoto     = p.Member.PhotoPath,
        PlanName        = p.Plan?.Name ?? p.Description ?? "Pago libre",
        EndDate         = p.DueDate,
        DaysUntilExpiry = (p.DueDate.Date - DateTime.Today).Days,
        Status          = p.DueDate.Date < DateTime.Today
                          ? MemberPlanStatus.Expired
                          : MemberPlanStatus.Active
    };
}

public class DebtNoticeRequest
{
    public int      MemberId { get; set; }
    public DateTime EndDate  { get; set; }
}
