using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _db;

    public CompanyService(ApplicationDbContext db) => _db = db;

    /// <summary>Devuelve el registro singleton (Id=1). Si no existe lo crea.</summary>
    public async Task<CompanySettings> GetAsync()
    {
        var settings = await _db.CompanySettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new CompanySettings { Id = 1, Name = "FitManager", UpdatedAt = DateTime.UtcNow };
            _db.CompanySettings.Add(settings);
            await _db.SaveChangesAsync();
        }
        return settings;
    }

    public async Task<CompanySettings> UpdateAsync(CompanySettingsViewModel vm, string webRootPath)
    {
        var settings = await GetAsync();

        settings.Name      = vm.Name;
        settings.Slogan    = vm.Slogan;
        settings.Address   = vm.Address;
        settings.City      = vm.City;
        settings.Province  = vm.Province;
        settings.Country   = vm.Country;
        settings.TaxId     = vm.TaxId;
        settings.Phone     = vm.Phone;
        settings.Phone2    = vm.Phone2;
        settings.Email     = vm.Email;
        settings.Website   = vm.Website;
        settings.Notes     = vm.Notes;
        settings.UpdatedAt = DateTime.UtcNow;

        if (vm.LogoFile != null && vm.LogoFile.Length > 0)
        {
            // Validar extensión
            var ext = Path.GetExtension(vm.LogoFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };
            if (!allowed.Contains(ext))
                throw new InvalidOperationException($"Formato de imagen no permitido: {ext}");

            // Validar tamaño (max 3 MB)
            if (vm.LogoFile.Length > 3 * 1024 * 1024)
                throw new InvalidOperationException("El logo no puede superar los 3 MB.");

            // Eliminar logo anterior
            if (!string.IsNullOrEmpty(settings.LogoPath))
            {
                var oldPath = Path.Combine(webRootPath, settings.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }

            // Guardar nuevo logo
            var uploadDir = Path.Combine(webRootPath, "uploads", "company");
            Directory.CreateDirectory(uploadDir);
            var fileName = $"logo{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await vm.LogoFile.CopyToAsync(stream);

            settings.LogoPath = $"uploads/company/{fileName}";
        }

        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task DeleteLogoAsync(string webRootPath)
    {
        var settings = await GetAsync();
        if (string.IsNullOrEmpty(settings.LogoPath)) return;

        var fullPath = Path.Combine(webRootPath, settings.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath)) File.Delete(fullPath);

        settings.LogoPath  = null;
        settings.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
