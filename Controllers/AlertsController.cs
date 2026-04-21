using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

[Authorize]
public class AlertsController : Controller
{
    private readonly ApplicationDbContext _db;

    public AlertsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Alerts/Expirations
    public async Task<IActionResult> Expirations()
    {
        var today = DateTime.Today;
        var in5   = today.AddDays(5);

        // Traer todos los MemberPlans activos con sus relaciones
        var allActive = await _db.MemberPlans
            .Include(mp => mp.Member)
            .Include(mp => mp.Plan)
            .Where(mp => mp.Status == MemberPlanStatus.Active)
            .ToListAsync();

        // ── Lógica clave ────────────────────────────────────────────────────────
        // Por socio, quedarse SOLO con el plan que tiene la fecha de vencimiento
        // más lejana (el "plan vigente más importante").
        // Si ese plan cubre el futuro → no hay alerta.
        // Si ese plan vence en ≤5 días o ya venció → sí hay alerta.
        var latestPerMember = allActive
            .GroupBy(mp => mp.MemberId)
            .Select(g => g.OrderByDescending(mp => mp.EndDate).First())
            .ToList();

        var vm = new AlertsViewModel
        {
            ExpiringToday = latestPerMember
                .Where(mp => mp.EndDate.Date == today)
                .OrderBy(mp => mp.Member!.LastName)
                .Select(ToAlert).ToList(),

            ExpiringIn5Days = latestPerMember
                .Where(mp => mp.EndDate.Date > today && mp.EndDate.Date <= in5)
                .OrderBy(mp => mp.EndDate)
                .Select(ToAlert).ToList(),

            AlreadyExpired = latestPerMember
                .Where(mp => mp.EndDate.Date < today)
                .OrderBy(mp => mp.EndDate)
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

    private static MemberPlanAlert ToAlert(MemberPlan mp) => new()
    {
        MemberId        = mp.Member!.Id,
        MemberNumber    = mp.Member.MemberNumber,
        MemberName      = mp.Member.FullName,
        MemberPhoto     = mp.Member.PhotoPath,
        PlanName        = mp.Plan!.Name,
        EndDate         = mp.EndDate,
        DaysUntilExpiry = (mp.EndDate.Date - DateTime.Today).Days,
        Status          = mp.Status
    };
}
