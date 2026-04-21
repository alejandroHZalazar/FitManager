using FitManager.Models;
using FitManager.ViewModels;

namespace FitManager.Services;

public interface IMemberService
{
    Task<List<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(int id);
    Task<Member> CreateAsync(MemberViewModel vm);
    Task<Member> UpdateAsync(MemberViewModel vm);
    Task<bool> DeleteAsync(int id);
    Task<string> GenerateMemberNumberAsync();
    Task<bool> ExistsDNIAsync(string dni, int? excludeId = null);
}
