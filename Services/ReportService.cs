using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _db;
    public ReportService(ApplicationDbContext db) => _db = db;

    // ── Reporte 1: Ingresos ───────────────────────────────────────────────────

    public async Task<RevenueReportViewModel> GetRevenueReportAsync(ReportFilterViewModel filter)
    {
        var (from, to) = filter.Resolve();
        var toEnd = to.Date.AddDays(1).AddTicks(-1); // incluye todo el día "to"

        var payments = await _db.Payments
            .Include(p => p.Member)
            .Include(p => p.Plan)
            .Where(p => p.Status == PaymentStatus.Paid
                     && p.PaymentDate.Date >= from.Date
                     && p.PaymentDate.Date <= to.Date)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var total = payments.Sum(p => p.Amount);

        // Por mes
        var byMonth = payments
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new MonthlyRevenue
            {
                Year  = g.Key.Year,
                Month = g.Key.Month,
                Total = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        // Por plan
        var byPlan = payments
            .GroupBy(p => p.Plan?.Name ?? "Sin plan")
            .Select(g => new RevenueByPlan
            {
                PlanName = g.Key,
                Total    = g.Sum(p => p.Amount),
                Count    = g.Count(),
                Percent  = total > 0 ? Math.Round(g.Sum(p => p.Amount) / total * 100, 1) : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // Por método
        var byMethod = payments
            .GroupBy(p => p.Method)
            .Select(g => new RevenueByMethod
            {
                Method  = g.Key,
                Total   = g.Sum(p => p.Amount),
                Count   = g.Count(),
                Percent = total > 0 ? Math.Round(g.Sum(p => p.Amount) / total * 100, 1) : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        return new RevenueReportViewModel
        {
            Filter           = filter,
            From             = from,
            To               = to,
            TotalRevenue     = total,
            CashRevenue      = payments.Where(p => p.Method == PaymentMethod.Cash).Sum(p => p.Amount),
            CardRevenue      = payments.Where(p => p.Method == PaymentMethod.Card).Sum(p => p.Amount),
            TransferRevenue  = payments.Where(p => p.Method == PaymentMethod.Transfer).Sum(p => p.Amount),
            PaymentCount     = payments.Count,
            AverageTicket    = payments.Count > 0 ? Math.Round(total / payments.Count, 2) : 0,
            ByMonth          = byMonth,
            ByPlan           = byPlan,
            ByMethod         = byMethod,
            Payments         = payments
        };
    }

    // ── Reporte 2: Socios activos ─────────────────────────────────────────────

    public async Task<ActiveMembersReportViewModel> GetActiveMembersReportAsync(ReportFilterViewModel filter)
    {
        var (from, to) = filter.Resolve();
        var today = DateTime.Today;

        var allActive = await _db.Members
            .Where(m => m.Status == MemberStatus.Active)
            .ToListAsync();

        var activePlans = await _db.MemberPlans
            .Include(mp => mp.Plan)
            .Where(mp => mp.Status == MemberPlanStatus.Active
                      && mp.EndDate.Date >= today)
            .ToListAsync();

        var planByMember = activePlans
            .GroupBy(mp => mp.MemberId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.EndDate).First());

        // Nuevos en el período
        var newMembers = allActive
            .Where(m => m.MembershipStartDate.Date >= from.Date
                     && m.MembershipStartDate.Date <= to.Date)
            .ToList();

        // Por plan
        var byPlan = activePlans
            .GroupBy(mp => mp.Plan.Name)
            .Select(g => new MembersByPlan
            {
                PlanName = g.Key,
                Count    = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var totalWithPlan = byPlan.Sum(x => x.Count);
        foreach (var p in byPlan)
            p.Percent = totalWithPlan > 0 ? Math.Round((decimal)p.Count / totalWithPlan * 100, 1) : 0;

        // Filas
        var rows = allActive.Select(m =>
        {
            planByMember.TryGetValue(m.Id, out var mp);
            var daysLeft = mp != null ? (int)(mp.EndDate.Date - today).TotalDays : (int?)null;
            return new ActiveMemberRow
            {
                Id           = m.Id,
                MemberNumber = m.MemberNumber,
                FullName     = m.FullName,
                PhotoPath    = m.PhotoPath,
                PlanName     = mp?.Plan?.Name,
                PlanEndDate  = mp?.EndDate,
                DaysLeft     = daysLeft,
                JoinDate     = m.MembershipStartDate,
                IsNew        = m.MembershipStartDate.Date >= from.Date && m.MembershipStartDate.Date <= to.Date
            };
        })
        .OrderBy(r => r.FullName)
        .ToList();

        return new ActiveMembersReportViewModel
        {
            Filter             = filter,
            From               = from,
            To                 = to,
            TotalActive        = allActive.Count,
            NewInPeriod        = newMembers.Count,
            WithActivePlan     = planByMember.Count,
            WithoutActivePlan  = allActive.Count - planByMember.Count,
            ExpiringIn5Days    = activePlans.Count(mp => mp.EndDate.Date >= today && mp.EndDate.Date <= today.AddDays(5)),
            ByPlan             = byPlan,
            Members            = rows
        };
    }

    // ── Reporte 3: Deudores ───────────────────────────────────────────────────

    public async Task<DebtorsReportViewModel> GetDebtorsReportAsync(ReportFilterViewModel filter)
    {
        var (from, to) = filter.Resolve();
        var today = DateTime.Today;

        var overduePayments = await _db.Payments
            .Include(p => p.Member)
            .Where(p => (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue)
                     && p.DueDate.Date >= from.Date
                     && p.DueDate.Date <= to.Date)
            .ToListAsync();

        var debtors = overduePayments
            .GroupBy(p => p.MemberId)
            .Select(g =>
            {
                var member   = g.First().Member!;
                var oldest   = g.Min(p => p.DueDate);
                var daysOver = Math.Max(0, (today - oldest.Date).Days);
                return new DebtorRow
                {
                    MemberId      = member.Id,
                    MemberNumber  = member.MemberNumber,
                    FullName      = member.FullName,
                    PhotoPath     = member.PhotoPath,
                    Phone         = member.Phone,
                    Email         = member.Email,
                    TotalDebt     = g.Sum(p => p.Amount),
                    PaymentCount  = g.Count(),
                    OldestDueDate = oldest,
                    DaysOverdue   = daysOver
                };
            })
            .OrderByDescending(d => d.DaysOverdue)
            .ToList();

        var totalDebt = debtors.Sum(d => d.TotalDebt);

        return new DebtorsReportViewModel
        {
            Filter        = filter,
            From          = from,
            To            = to,
            TotalDebtors  = debtors.Count,
            TotalDebt     = totalDebt,
            AvgDebt       = debtors.Count > 0 ? Math.Round(totalDebt / debtors.Count, 2) : 0,
            CriticalCount = debtors.Count(d => d.DaysOverdue > 30),
            Debtors       = debtors
        };
    }

    // ── Reporte 4: Caja ───────────────────────────────────────────────────────

    public async Task<CashReportViewModel> GetCashReportAsync(ReportFilterViewModel filter)
    {
        var (from, to) = filter.Resolve();

        var registers = await _db.CashRegisters
            .Include(cr => cr.Transactions)
            .Include(cr => cr.Payments)
            .Where(cr => cr.Date.Date >= from.Date && cr.Date.Date <= to.Date)
            .OrderBy(cr => cr.Date)
            .ToListAsync();

        var rows = registers.Select(cr => new CashRegisterRow
        {
            Id             = cr.Id,
            Date           = cr.Date,
            OpenedBy       = cr.OpenedBy,
            ClosedBy       = cr.ClosedBy,
            OpeningBalance = cr.OpeningBalance,
            ClosingBalance = cr.ClosingBalance,
            PaymentsTotal  = cr.PaymentsTotal,
            Income         = cr.TotalIncome,
            Expenses       = cr.TotalExpense,
            NetBalance     = cr.NetBalance,
            Status         = cr.Status,
            WasReopened    = cr.WasReopened
        }).ToList();

        // Por día (para gráfico)
        var byDay = rows.Select(r => new DailyCash
        {
            Date     = r.Date,
            Income   = r.Income,
            Expenses = r.Expenses,
            Payments = r.PaymentsTotal
        }).ToList();

        return new CashReportViewModel
        {
            Filter          = filter,
            From            = from,
            To              = to,
            TotalIncome     = rows.Sum(r => r.Income + r.PaymentsTotal),
            TotalExpenses   = rows.Sum(r => r.Expenses),
            TotalPayments   = rows.Sum(r => r.PaymentsTotal),
            NetBalance      = rows.Sum(r => r.NetBalance),
            RegistersCount  = rows.Count,
            ClosedRegisters = rows.Count(r => r.Status == CashRegisterStatus.Closed),
            ByDay           = byDay,
            Registers       = rows
        };
    }
}
