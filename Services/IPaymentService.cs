using FitManager.Models;
using FitManager.ViewModels;

namespace FitManager.Services;

public interface IPaymentService
{
    Task<List<Payment>> GetByMemberAsync(int memberId);
    Task<List<Payment>> GetAllAsync(PaymentFilterViewModel? filter = null);
    Task<Payment> CreateAsync(PaymentViewModel vm, string createdBy);
    Task<bool> DeleteAsync(int id);
    Task<(decimal paid, decimal pending)> GetTotalsAsync(int memberId);
}
