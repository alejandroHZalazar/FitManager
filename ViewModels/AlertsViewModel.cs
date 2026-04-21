using FitManager.Models;

namespace FitManager.ViewModels;

public class AlertsViewModel
{
    // Expiring/expired plans
    public List<MemberPlanAlert> ExpiringToday   { get; set; } = new();
    public List<MemberPlanAlert> ExpiringIn5Days  { get; set; } = new();
    public List<MemberPlanAlert> AlreadyExpired   { get; set; } = new();

    // Overdue payments
    public List<DebtAlert> OverduePayments { get; set; } = new();

    public int TotalAlerts =>
        ExpiringToday.Count + ExpiringIn5Days.Count + AlreadyExpired.Count + OverduePayments.Count;
}

public class MemberPlanAlert
{
    public int MemberId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string? MemberPhoto { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public MemberPlanStatus Status { get; set; }
}

public class DebtAlert
{
    public int MemberId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string? MemberPhoto { get; set; }
    public decimal TotalDebt { get; set; }
    public int OverdueCount { get; set; }
    public DateTime OldestDueDate { get; set; }
    public int DaysOverdue { get; set; }
}
