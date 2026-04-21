using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FitManager.Services;

public class MemberService : IMemberService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public MemberService(ApplicationDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;
    }

    public async Task<List<Member>> GetAllAsync() =>
        await _db.Members.OrderByDescending(m => m.CreatedAt).ToListAsync();

    public async Task<Member?> GetByIdAsync(int id) =>
        await _db.Members.Include(m => m.Payments).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<string> GenerateMemberNumberAsync()
    {
        var year = DateTime.Today.Year;
        var count = await _db.Members.CountAsync(m => m.MembershipStartDate.Year == year);
        return $"FM-{year}-{(count + 1):D4}";
    }

    public async Task<bool> ExistsDNIAsync(string dni, int? excludeId = null)
    {
        var query = _db.Members.Where(m => m.DNI == dni);
        if (excludeId.HasValue) query = query.Where(m => m.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<Member> CreateAsync(MemberViewModel vm)
    {
        var member = MapToEntity(new Member(), vm);
        member.MemberNumber = await GenerateMemberNumberAsync();
        member.CreatedAt = DateTime.UtcNow;
        member.UpdatedAt = DateTime.UtcNow;

        if (vm.PhotoFile != null)
            member.PhotoPath = await SavePhotoAsync(vm.PhotoFile);

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task<Member> UpdateAsync(MemberViewModel vm)
    {
        var member = await _db.Members.FindAsync(vm.Id)
            ?? throw new KeyNotFoundException("Socio no encontrado");

        MapToEntity(member, vm);
        member.UpdatedAt = DateTime.UtcNow;

        if (vm.PhotoFile != null)
        {
            DeletePhoto(member.PhotoPath);
            member.PhotoPath = await SavePhotoAsync(vm.PhotoFile);
        }

        await _db.SaveChangesAsync();
        return member;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return false;
        DeletePhoto(member.PhotoPath);
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Member MapToEntity(Member entity, MemberViewModel vm)
    {
        entity.FirstName = vm.FirstName;
        entity.LastName = vm.LastName;
        entity.Email = vm.Email;
        entity.Phone = vm.Phone;
        entity.Address = vm.Address;
        entity.DateOfBirth = vm.DateOfBirth;
        entity.DNI = vm.DNI;
        entity.Status = vm.Status;
        entity.MembershipStartDate = vm.MembershipStartDate;
        entity.MembershipEndDate = vm.MembershipEndDate;
        entity.Notes = vm.Notes;
        return entity;
    }

    private async Task<string> SavePhotoAsync(IFormFile file)
    {
        var uploadsPath = _config["AppSettings:PhotosPath"] ?? "uploads/members";
        var fullPath = Path.Combine(_env.WebRootPath, uploadsPath);
        Directory.CreateDirectory(fullPath);

        var fileName = $"{Guid.NewGuid():N}.webp";
        var filePath = Path.Combine(fullPath, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(400, 400),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center
        }));
        await image.SaveAsWebpAsync(filePath);

        return $"/{uploadsPath}/{fileName}";
    }

    private void DeletePhoto(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}
