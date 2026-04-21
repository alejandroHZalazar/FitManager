using FitManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Member> Members { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }

    // ── Payment module ────────────────────────────────────────────────────────
    public DbSet<Plan> Plans { get; set; }
    public DbSet<MemberPlan> MemberPlans { get; set; }
    public DbSet<CashRegister> CashRegisters { get; set; }
    public DbSet<CashTransaction> CashTransactions { get; set; }

    // ── Classes module ────────────────────────────────────────────────────────
    public DbSet<FitnessClass>    FitnessClasses   { get; set; }
    public DbSet<ClassSchedule>   ClassSchedules   { get; set; }
    public DbSet<ClassEnrollment> ClassEnrollments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");

        // Member
        builder.Entity<Member>(e =>
        {
            e.HasIndex(m => m.MemberNumber).IsUnique();
            e.HasIndex(m => m.DNI);
            e.Property(m => m.Status).HasConversion<int>();
        });

        // Payment
        builder.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Member)
             .WithMany(m => m.Payments)
             .HasForeignKey(p => p.MemberId)
             .OnDelete(DeleteBehavior.Restrict);
            e.Property(p => p.Status).HasConversion<int>();
            e.Property(p => p.Method).HasConversion<int>();

            e.HasOne(p => p.Plan)
             .WithMany(pl => pl.Payments)
             .HasForeignKey(p => p.PlanId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(p => p.MemberPlan)
             .WithMany()
             .HasForeignKey(p => p.MemberPlanId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(p => p.CashRegister)
             .WithMany(cr => cr.Payments)
             .HasForeignKey(p => p.CashRegisterId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // MenuItem
        builder.Entity<MenuItem>(e =>
        {
            e.HasOne(m => m.Parent)
             .WithMany(m => m.Children)
             .HasForeignKey(m => m.ParentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // RoleMenuPermission
        builder.Entity<RoleMenuPermission>(e =>
        {
            e.HasIndex(r => new { r.RoleId, r.MenuItemId }).IsUnique();
            e.HasOne(r => r.Role)
             .WithMany(r => r.MenuPermissions)
             .HasForeignKey(r => r.RoleId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.MenuItem)
             .WithMany(m => m.RolePermissions)
             .HasForeignKey(r => r.MenuItemId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Plan
        builder.Entity<Plan>(e =>
        {
            e.Property(p => p.PlanType).HasConversion<int>();
        });

        // MemberPlan
        builder.Entity<MemberPlan>(e =>
        {
            e.Property(mp => mp.Status).HasConversion<int>();
            e.HasOne(mp => mp.Member)
             .WithMany()
             .HasForeignKey(mp => mp.MemberId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(mp => mp.Plan)
             .WithMany(p => p.MemberPlans)
             .HasForeignKey(mp => mp.PlanId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(mp => mp.Payment)
             .WithMany()
             .HasForeignKey(mp => mp.PaymentId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // CashRegister
        builder.Entity<CashRegister>(e =>
        {
            e.HasIndex(cr => cr.Date).IsUnique();
            e.Property(cr => cr.Status).HasConversion<int>();
        });

        // CashTransaction
        builder.Entity<CashTransaction>(e =>
        {
            e.Property(ct => ct.Type).HasConversion<int>();
            e.Property(ct => ct.Method).HasConversion<int>();
            e.HasOne(ct => ct.CashRegister)
             .WithMany(cr => cr.Transactions)
             .HasForeignKey(ct => ct.CashRegisterId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // FitnessClass
        builder.Entity<FitnessClass>(e =>
        {
            e.Property(c => c.Category).HasConversion<int>();
        });

        // ClassSchedule
        builder.Entity<ClassSchedule>(e =>
        {
            e.Property(s => s.ScheduleType).HasConversion<int>();
            e.Property(s => s.DaysOfWeek).HasConversion<int>();
            e.HasOne(s => s.FitnessClass)
             .WithMany(c => c.Schedules)
             .HasForeignKey(s => s.FitnessClassId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ClassEnrollment
        builder.Entity<ClassEnrollment>(e =>
        {
            e.Property(en => en.Status).HasConversion<int>();
            e.HasIndex(en => new { en.FitnessClassId, en.MemberId });
            e.HasOne(en => en.FitnessClass)
             .WithMany(c => c.Enrollments)
             .HasForeignKey(en => en.FitnessClassId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(en => en.Member)
             .WithMany()
             .HasForeignKey(en => en.MemberId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
