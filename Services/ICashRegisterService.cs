using FitManager.Models;
using FitManager.ViewModels;

namespace FitManager.Services;

public interface ICashRegisterService
{
    Task<CashRegister?> GetTodayAsync();
    Task<CashRegister?> GetByIdAsync(int id);
    Task<List<CashRegister>> GetHistoryAsync(int take = 30);
    Task<CashRegister> OpenAsync(OpenCashRegisterViewModel vm, string user);
    Task<CashRegister> CloseAsync(CloseCashRegisterViewModel vm, string user);
    Task<CashTransaction> AddTransactionAsync(CashTransactionFormViewModel vm, string user);
    Task<bool> DeleteTransactionAsync(int id);
    Task<CashRegisterDetailViewModel> GetDetailAsync(int id);
    Task<CashRegisterDetailViewModel?> GetTodayDetailAsync();
    Task<CashRegister> ReopenAsync(ReopenCashRegisterViewModel vm, string user);
}
