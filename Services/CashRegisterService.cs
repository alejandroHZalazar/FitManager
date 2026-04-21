using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class CashRegisterService : ICashRegisterService
{
    private readonly ApplicationDbContext _db;
    public CashRegisterService(ApplicationDbContext db) => _db = db;

    public async Task<CashRegister?> GetTodayAsync() =>
        await _db.CashRegisters.FirstOrDefaultAsync(cr => cr.Date == DateTime.Today);

    public async Task<CashRegister?> GetByIdAsync(int id) =>
        await _db.CashRegisters.Include(cr => cr.Transactions).FirstOrDefaultAsync(cr => cr.Id == id);

    public async Task<List<CashRegister>> GetHistoryAsync(int take = 30) =>
        await _db.CashRegisters
            .Include(cr => cr.Transactions)
            .Include(cr => cr.Payments)
            .OrderByDescending(cr => cr.Date)
            .Take(take)
            .ToListAsync();

    public async Task<CashRegister> OpenAsync(OpenCashRegisterViewModel vm, string user)
    {
        var existing = await GetTodayAsync();
        if (existing != null)
            throw new InvalidOperationException("Ya existe una caja abierta para hoy.");

        var register = new CashRegister
        {
            Date = DateTime.Today,
            OpeningBalance = vm.OpeningBalance,
            Notes = vm.Notes,
            Status = CashRegisterStatus.Open,
            OpenedBy = user,
            OpenedAt = DateTime.UtcNow
        };
        _db.CashRegisters.Add(register);
        await _db.SaveChangesAsync();
        return register;
    }

    public async Task<CashRegister> CloseAsync(CloseCashRegisterViewModel vm, string user)
    {
        var register = await _db.CashRegisters.FindAsync(vm.RegisterId)
            ?? throw new KeyNotFoundException("Caja no encontrada");

        if (register.Status == CashRegisterStatus.Closed)
            throw new InvalidOperationException("La caja ya está cerrada.");

        register.ClosingBalance = vm.ClosingBalance;
        register.ClosedBy = user;
        register.ClosedAt = DateTime.UtcNow;
        register.Status = CashRegisterStatus.Closed;
        if (!string.IsNullOrEmpty(vm.Notes)) register.Notes = vm.Notes;

        await _db.SaveChangesAsync();
        return register;
    }

    public async Task<CashTransaction> AddTransactionAsync(CashTransactionFormViewModel vm, string user)
    {
        var tx = new CashTransaction
        {
            CashRegisterId = vm.CashRegisterId,
            Type = vm.Type,
            Amount = vm.Amount,
            Description = vm.Description,
            Method = vm.Method,
            Notes = vm.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user
        };
        _db.CashTransactions.Add(tx);
        await _db.SaveChangesAsync();
        return tx;
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        var tx = await _db.CashTransactions.FindAsync(id);
        if (tx == null) return false;
        _db.CashTransactions.Remove(tx);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<CashRegisterDetailViewModel> GetDetailAsync(int id)
    {
        var register = await _db.CashRegisters
            .Include(cr => cr.Transactions)
            .FirstOrDefaultAsync(cr => cr.Id == id)
            ?? throw new KeyNotFoundException("Caja no encontrada");

        return await BuildDetailViewModel(register);
    }

    public async Task<CashRegister> ReopenAsync(ReopenCashRegisterViewModel vm, string user)
    {
        var register = await _db.CashRegisters.FindAsync(vm.RegisterId)
            ?? throw new KeyNotFoundException("Caja no encontrada");

        if (register.Status == CashRegisterStatus.Open)
            throw new InvalidOperationException("La caja ya está abierta.");

        register.Status         = CashRegisterStatus.Open;
        register.ClosingBalance = null;
        register.ClosedBy       = null;
        register.ClosedAt       = null;
        register.WasReopened    = true;
        register.ReopenedBy     = user;
        register.ReopenedAt     = DateTime.UtcNow;
        register.ReopenNotes    = vm.Reason;

        await _db.SaveChangesAsync();
        return register;
    }

    public async Task<CashRegisterDetailViewModel?> GetTodayDetailAsync()
    {
        var register = await _db.CashRegisters
            .Include(cr => cr.Transactions)
            .FirstOrDefaultAsync(cr => cr.Date == DateTime.Today);

        return register == null ? null : await BuildDetailViewModel(register);
    }

    private async Task<CashRegisterDetailViewModel> BuildDetailViewModel(CashRegister register)
    {
        var payments = await _db.Payments
            .Include(p => p.Member)
            .Include(p => p.Plan)
            .Where(p => p.CashRegisterId == register.Id && p.Status == PaymentStatus.Paid)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var cash     = payments.Where(p => p.Method == PaymentMethod.Cash).Sum(p => p.Amount);
        var card     = payments.Where(p => p.Method == PaymentMethod.Card).Sum(p => p.Amount);
        var transfer = payments.Where(p => p.Method == PaymentMethod.Transfer).Sum(p => p.Amount);
        var other    = payments.Where(p => p.Method == PaymentMethod.Other).Sum(p => p.Amount);
        var total    = cash + card + transfer + other;

        var manualIncome  = register.Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var manualExpense = register.Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var grossIncome   = total + manualIncome;
        var netBalance    = register.OpeningBalance + grossIncome - manualExpense;

        return new CashRegisterDetailViewModel
        {
            Register         = register,
            Payments         = payments,
            PaymentsCash     = cash,
            PaymentsCard     = card,
            PaymentsTransfer = transfer,
            PaymentsOther    = other,
            PaymentsTotal    = total,
            Transactions     = register.Transactions.OrderByDescending(t => t.CreatedAt).ToList(),
            ManualIncome     = manualIncome,
            ManualExpense    = manualExpense,
            GrossIncome      = grossIncome,
            NetBalance       = netBalance,
            Discrepancy      = register.ClosingBalance.HasValue
                               ? register.ClosingBalance.Value - netBalance
                               : 0,
            NewTransaction   = new CashTransactionFormViewModel { CashRegisterId = register.Id }
        };
    }
}
