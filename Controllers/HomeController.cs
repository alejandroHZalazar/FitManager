using FitManager.Data;
using FitManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var totalMembers = await _db.Members.CountAsync();
        var activeMembers = await _db.Members.CountAsync(m => m.Status == MemberStatus.Active);
        var inactiveMembers = totalMembers - activeMembers;

        var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthlyRevenue = await _db.Payments
            .Where(p => p.PaymentDate >= thisMonth && p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var newMembersThisMonth = await _db.Members
            .CountAsync(m => m.MembershipStartDate >= thisMonth);

        var recentPayments = await _db.Payments
            .Include(p => p.Member)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentMembers = await _db.Members
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .ToListAsync();

        // Monthly revenue last 6 months
        var sixMonthsAgo = DateTime.Today.AddMonths(-5);
        var monthlyData = await _db.Payments
            .Where(p => p.PaymentDate >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1)
                        && p.Status == PaymentStatus.Paid)
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.Amount) })
            .ToListAsync();

        ViewBag.TotalMembers = totalMembers;
        ViewBag.ActiveMembers = activeMembers;
        ViewBag.InactiveMembers = inactiveMembers;
        ViewBag.MonthlyRevenue = monthlyRevenue;
        ViewBag.NewMembersThisMonth = newMembersThisMonth;
        ViewBag.RecentPayments = recentPayments;
        ViewBag.RecentMembers = recentMembers;
        ViewBag.MonthlyData = monthlyData;

        return View();
    }

    public IActionResult Error() => View();
}
