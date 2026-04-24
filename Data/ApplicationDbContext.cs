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
    public DbSet<ClassCategory>   ClassCategories  { get; set; }
    public DbSet<FitnessClass>    FitnessClasses   { get; set; }
    public DbSet<ClassSchedule>   ClassSchedules   { get; set; }
    public DbSet<ClassEnrollment> ClassEnrollments { get; set; }

    // ── Company ───────────────────────────────────────────────────────────────
    public DbSet<CompanySettings> CompanySettings  { get; set; }

    // ── Routines module ───────────────────────────────────────────────────────
    public DbSet<Exercise>        Exercises        { get; set; }
    public DbSet<Routine>         Routines         { get; set; }
    public DbSet<RoutineDay>      RoutineDays      { get; set; }
    public DbSet<RoutineExercise> RoutineExercises { get; set; }
    public DbSet<MemberRoutine>   MemberRoutines   { get; set; }

    // ── Nutrition module ──────────────────────────────────────────────────────
    public DbSet<NutritionPlan>       NutritionPlans       { get; set; }
    public DbSet<NutritionMeal>       NutritionMeals       { get; set; }
    public DbSet<MemberNutritionPlan> MemberNutritionPlans { get; set; }

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

        // ClassCategory
        builder.Entity<ClassCategory>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        // FitnessClass
        builder.Entity<FitnessClass>(e =>
        {
            e.HasOne(c => c.Category)
             .WithMany(cat => cat.Classes)
             .HasForeignKey(c => c.ClassCategoryId)
             .OnDelete(DeleteBehavior.SetNull);
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

        // Exercise
        builder.Entity<Exercise>(e =>
        {
            e.Property(ex => ex.MuscleGroup).HasConversion<int>();
            e.Property(ex => ex.ExerciseType).HasConversion<int>();
        });

        // Routine
        builder.Entity<Routine>(e =>
        {
            e.Property(r => r.Goal).HasConversion<int>();
        });

        // RoutineDay
        builder.Entity<RoutineDay>(e =>
        {
            e.HasOne(d => d.Routine)
             .WithMany(r => r.Days)
             .HasForeignKey(d => d.RoutineId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // RoutineExercise
        builder.Entity<RoutineExercise>(e =>
        {
            e.HasOne(re => re.RoutineDay)
             .WithMany(d => d.Exercises)
             .HasForeignKey(re => re.RoutineDayId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(re => re.Exercise)
             .WithMany(ex => ex.RoutineExercises)
             .HasForeignKey(re => re.ExerciseId)
             .OnDelete(DeleteBehavior.Restrict);
            e.Property(re => re.Weight).HasColumnType("decimal(8,2)");
        });

        // MemberRoutine
        builder.Entity<MemberRoutine>(e =>
        {
            e.HasOne(mr => mr.Member)
             .WithMany()
             .HasForeignKey(mr => mr.MemberId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(mr => mr.Routine)
             .WithMany(r => r.MemberRoutines)
             .HasForeignKey(mr => mr.RoutineId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(mr => new { mr.MemberId, mr.IsActive });
        });

        // NutritionPlan
        builder.Entity<NutritionPlan>(e =>
        {
            e.Property(p => p.Goal).HasConversion<int>();
        });

        // NutritionMeal
        builder.Entity<NutritionMeal>(e =>
        {
            e.Property(m => m.MealType).HasConversion<int>();
            e.HasOne(m => m.NutritionPlan)
             .WithMany(p => p.Meals)
             .HasForeignKey(m => m.NutritionPlanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // MemberNutritionPlan
        builder.Entity<MemberNutritionPlan>(e =>
        {
            e.HasOne(a => a.Member)
             .WithMany()
             .HasForeignKey(a => a.MemberId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.NutritionPlan)
             .WithMany(p => p.Assignments)
             .HasForeignKey(a => a.NutritionPlanId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => new { a.MemberId, a.IsActive });
        });
    }
}
