using FitManager.Models;
using FitManager.ViewModels;

namespace FitManager.Services;

public interface ICompanyService
{
    Task<CompanySettings> GetAsync();
    Task<CompanySettings> UpdateAsync(CompanySettingsViewModel vm, string webRootPath);
    Task DeleteLogoAsync(string webRootPath);

    // ── WhatsApp events ───────────────────────────────────────────────────────
    Task<List<WhatsAppEvent>> GetWhatsAppEventsAsync();
    Task<WhatsAppEvent> AddWhatsAppEventAsync(WhatsAppEventViewModel vm);  // uso interno / seed
    Task<WhatsAppEvent?> UpdateWhatsAppEventAsync(int id, WhatsAppEventViewModel vm);
    Task<bool> ToggleWhatsAppEventAsync(int id);
}
