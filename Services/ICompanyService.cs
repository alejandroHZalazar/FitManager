using FitManager.Models;
using FitManager.ViewModels;

namespace FitManager.Services;

public interface ICompanyService
{
    Task<CompanySettings> GetAsync();
    Task<CompanySettings> UpdateAsync(CompanySettingsViewModel vm, string webRootPath);
    Task DeleteLogoAsync(string webRootPath);
}
