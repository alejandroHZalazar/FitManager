using FitManager.ViewModels;

namespace FitManager.Services;

public interface IReportService
{
    Task<RevenueReportViewModel>       GetRevenueReportAsync(ReportFilterViewModel filter);
    Task<ActiveMembersReportViewModel> GetActiveMembersReportAsync(ReportFilterViewModel filter);
    Task<DebtorsReportViewModel>       GetDebtorsReportAsync(ReportFilterViewModel filter);
    Task<CashReportViewModel>          GetCashReportAsync(ReportFilterViewModel filter);
}
