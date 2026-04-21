using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    public PaymentService(ApplicationDbContext db) => _db = db;

    public async Task<List<Payment>> GetByMemberAsync(int memberId) =>
        await _db.Payments
            .Include(p => p.Plan)
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<List<Payment>> GetAllAsync(PaymentFilterViewModel? filter = null)
    {
        var q = _db.Payments
            .Include(p => p.Member)
            .Include(p => p.Plan)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.From.HasValue)   q = q.Where(p => p.PaymentDate >= filter.From.Value);
            if (filter.To.HasValue)     q = q.Where(p => p.PaymentDate <= filter.To.Value);
            if (filter.MemberId.HasValue) q = q.Where(p => p.MemberId == filter.MemberId.Value);
            if (filter.PlanId.HasValue) q = q.Where(p => p.PlanId == filter.PlanId.Value);
            if (filter.Status.HasValue) q = q.Where(p => p.Status == filter.Status.Value);
            if (filter.Method.HasValue) q = q.Where(p => p.Method == filter.Method.Value);
        }

        return await q.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Payment> CreateAsync(PaymentViewModel vm, string createdBy)
    {
        // Resolve open cash register
        var cashRegister = await _db.CashRegisters
            .FirstOrDefaultAsync(cr => cr.Date == DateTime.Today && cr.Status == CashRegisterStatus.Open);

        var payment = new Payment
        {
            MemberId      = vm.MemberId,
            Amount        = vm.Amount,
            PaymentDate   = vm.PaymentDate,
            DueDate       = vm.DueDate,
            Status        = vm.Status,
            Method        = vm.Method,
            Description   = vm.Description,
            Notes         = vm.Notes,
            ReceiptNumber = vm.ReceiptNumber,
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = createdBy,
            CashRegisterId = cashRegister?.Id
        };

        // Handle plan
        if (vm.PlanId.HasValue && vm.PlanId > 0)
        {
            var plan = await _db.Plans.FindAsync(vm.PlanId.Value);
            if (plan != null)
            {
                payment.PlanId = plan.Id;
                payment.Description ??= plan.Name;
                if (payment.Amount == 0) payment.Amount = plan.Price;

                // Create MemberPlan
                var memberPlan = await CreateOrUpdateMemberPlanAsync(vm.MemberId, plan, payment.PaymentDate);
                payment.MemberPlanId = memberPlan.Id;
                payment.DueDate = memberPlan.EndDate;
            }
        }

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        // Link MemberPlan → Payment
        if (payment.MemberPlanId.HasValue)
        {
            var mp = await _db.MemberPlans.FindAsync(payment.MemberPlanId.Value);
            if (mp != null) { mp.PaymentId = payment.Id; await _db.SaveChangesAsync(); }
        }

        return payment;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _db.Payments.FindAsync(id);
        if (payment == null) return false;
        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(decimal paid, decimal pending)> GetTotalsAsync(int memberId)
    {
        var payments = await _db.Payments.Where(p => p.MemberId == memberId).ToListAsync();
        var paid    = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        var pending = payments.Where(p => p.Status is PaymentStatus.Pending or PaymentStatus.Overdue).Sum(p => p.Amount);
        return (paid, pending);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<MemberPlan> CreateOrUpdateMemberPlanAsync(int memberId, Plan plan, DateTime startDate)
    {
        var today = DateTime.Today;

        // ── Expirar todos los MemberPlans del socio cuya fecha de fin ya pasó ──
        // Evita que planes vencidos sigan apareciendo como "activos" en alertas.
        var staleOnes = await _db.MemberPlans
            .Where(mp => mp.MemberId == memberId
                      && mp.Status == MemberPlanStatus.Active
                      && mp.EndDate.Date < today)
            .ToListAsync();

        foreach (var stale in staleOnes)
            stale.Status = MemberPlanStatus.Expired;

        if (staleOnes.Any())
            await _db.SaveChangesAsync();

        // ── Buscar el plan VIGENTE más lejano del mismo tipo para extender ──────
        var existing = await _db.MemberPlans
            .Where(mp => mp.MemberId == memberId
                      && mp.PlanId == plan.Id
                      && mp.Status == MemberPlanStatus.Active
                      && mp.EndDate.Date >= today)
            .OrderByDescending(mp => mp.EndDate)
            .FirstOrDefaultAsync();

        DateTime start = existing != null
            ? existing.EndDate.AddDays(1)   // acumula días si ya tiene ese plan vigente
            : startDate;

        var memberPlan = new MemberPlan
        {
            MemberId  = memberId,
            PlanId    = plan.Id,
            StartDate = start,
            EndDate   = start.AddDays(plan.DurationDays - 1),
            Status    = MemberPlanStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _db.MemberPlans.Add(memberPlan);
        await _db.SaveChangesAsync();
        return memberPlan;
    }
}
