using FitManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Apply pending migrations
        await db.Database.MigrateAsync();

        // ── Fix AUTO_INCREMENT on Routine module tables ──────────────────────
        // The initial AddRoutinesModule migration was created without the
        // Pomelo identity annotation, so the Id columns were created without
        // AUTO_INCREMENT. This idempotent fix ensures they have it.
        foreach (var table in new[] { "Exercises", "Routines", "RoutineDays", "RoutineExercises", "MemberRoutines" })
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(
                    $"ALTER TABLE `{table}` MODIFY `Id` INT NOT NULL AUTO_INCREMENT;");
            }
            catch { /* table may not exist yet, or already has AUTO_INCREMENT */ }
        }

        // ── Roles ─────────────────────────────────────────────────────────────
        var roles = new[]
        {
            new ApplicationRole { Name = "Administrador", Description = "Acceso total al sistema", IsSystem = true },
            new ApplicationRole { Name = "Recepcionista", Description = "Gestión de socios y pagos", IsSystem = true },
            new ApplicationRole { Name = "Instructor", Description = "Consulta de socios", IsSystem = true }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
                await roleManager.CreateAsync(role);
        }

        // ── Admin user ────────────────────────────────────────────────────────
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@fitmanager.com",
                FirstName = "Administrador",
                LastName = "Sistema",
                IsActive = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "1234");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Administrador");
        }

        // ── Menu items ────────────────────────────────────────────────────────
        if (!await db.MenuItems.AnyAsync())
        {
            var menuItems = new List<MenuItem>
            {
                // ── Dashboard
                new() { Id = 1,  Name = "Dashboard",        Icon = "fas fa-tachometer-alt",       Controller = "Home",         Action = "Index",           OrderIndex = 1 },
                // ── Socios
                new() { Id = 2,  Name = "Socios",           Icon = "fas fa-users",          Url = "#",                        OrderIndex = 2 },
                new() { Id = 3,  Name = "Listado",          Icon = "fas fa-list",                 Controller = "Members",      Action = "Index",           ParentId = 2, OrderIndex = 1 },
                new() { Id = 4,  Name = "Nuevo Socio",      Icon = "fas fa-user-plus",            Controller = "Members",      Action = "Create",          ParentId = 2, OrderIndex = 2 },
                // ── Cuotas y Pagos
                new() { Id = 5,  Name = "Cuotas y Pagos",  Icon = "fas fa-credit-card",    Url = "#",                        OrderIndex = 3 },
                new() { Id = 6,  Name = "Registrar Pago",  Icon = "fas fa-plus-circle",          Controller = "Payments",     Action = "Create",          ParentId = 5, OrderIndex = 1 },
                new() { Id = 7,  Name = "Historial",        Icon = "fas fa-history",              Controller = "Payments",     Action = "Index",           ParentId = 5, OrderIndex = 2 },
                new() { Id = 8,  Name = "Planes",           Icon = "fas fa-tags",                 Controller = "Plans",        Action = "Index",           ParentId = 5, OrderIndex = 3 },
                new() { Id = 9,  Name = "Vencimientos",     Icon = "fas fa-calendar-times",       Controller = "Alerts",       Action = "Expirations",     ParentId = 5, OrderIndex = 4 },
                new() { Id = 10, Name = "Alertas de Deuda", Icon = "fas fa-exclamation-triangle", Controller = "Alerts",       Action = "Debts",           ParentId = 5, OrderIndex = 5 },
                new() { Id = 11, Name = "Caja Diaria",      Icon = "fas fa-cash-register",        Controller = "CashRegister", Action = "Index",           ParentId = 5, OrderIndex = 6 },
                // ── Clases
                new() { Id = 16, Name = "Clases",           Icon = "fas fa-chalkboard-teacher", Url = "#",                    OrderIndex = 4 },
                new() { Id = 17, Name = "Agenda Semanal",   Icon = "fas fa-calendar-week",        Controller = "Classes",      Action = "Calendar",        ParentId = 16, OrderIndex = 1 },
                new() { Id = 18, Name = "Gestión de Clases",Icon = "fas fa-list-alt",             Controller = "Classes",      Action = "Index",           ParentId = 16, OrderIndex = 2 },
                new() { Id = 24, Name = "Categorías",       Icon = "fas fa-tags",                 Controller = "ClassCategories", Action = "Index",         ParentId = 16, OrderIndex = 3 },
                // ── Rutinas
                new() { Id = 26, Name = "Rutinas",            Icon = "fas fa-clipboard-list",    Url = "#",                     OrderIndex = 5 },
                new() { Id = 27, Name = "Ejercicios",         Icon = "fas fa-dumbbell",                Controller = "Exercises",    Action = "Index",      ParentId = 26, OrderIndex = 1 },
                new() { Id = 28, Name = "Gestión de Rutinas", Icon = "fas fa-list-alt",                Controller = "Routines",     Action = "Index",      ParentId = 26, OrderIndex = 2 },
                new() { Id = 29, Name = "Asignaciones",       Icon = "fas fa-user-check",              Controller = "MemberRoutines", Action = "Index",    ParentId = 26, OrderIndex = 3 },
                // ── Nutrición
                new() { Id = 30, Name = "Nutrición",          Icon = "fas fa-leaf",               Url = "#",                     OrderIndex = 6 },
                new() { Id = 31, Name = "Planes",             Icon = "fas fa-clipboard-list",           Controller = "NutritionPlans", Action = "Index",    ParentId = 30, OrderIndex = 1 },
                new() { Id = 32, Name = "Asignaciones",       Icon = "fas fa-user-check",               Controller = "MemberNutrition",Action = "Index",    ParentId = 30, OrderIndex = 2 },
                // ── Reportes
                new() { Id = 19, Name = "Reportes",          Icon = "fas fa-chart-bar",          Url = "#",                    OrderIndex = 7 },
                new() { Id = 20, Name = "Ingresos",          Icon = "fas fa-dollar-sign",          Controller = "Reports",      Action = "Revenue",         ParentId = 19, OrderIndex = 1 },
                new() { Id = 21, Name = "Socios Activos",    Icon = "fas fa-users",                Controller = "Reports",      Action = "ActiveMembers",   ParentId = 19, OrderIndex = 2 },
                new() { Id = 22, Name = "Deudores",          Icon = "fas fa-exclamation-circle",   Controller = "Reports",      Action = "Debtors",         ParentId = 19, OrderIndex = 3 },
                new() { Id = 23, Name = "Informe de Caja",   Icon = "fas fa-cash-register",        Controller = "Reports",      Action = "Cash",            ParentId = 19, OrderIndex = 4 },
                // ── Administración
                new() { Id = 12, Name = "Administración",  Icon = "fas fa-cog",            Url = "#",                        OrderIndex = 10 },
                new() { Id = 13, Name = "Roles",            Icon = "fas fa-shield-alt",           Controller = "Roles",        Action = "Index",           ParentId = 12, OrderIndex = 1 },
                new() { Id = 14, Name = "Usuarios",         Icon = "fas fa-user-cog",             Controller = "Users",        Action = "Index",           ParentId = 12, OrderIndex = 2 },
                new() { Id = 15, Name = "Permisos Menú",   Icon = "fas fa-bars",                 Controller = "Roles",        Action = "MenuPermissions", ParentId = 12, OrderIndex = 3 },
                new() { Id = 25, Name = "Empresa",          Icon = "fas fa-building",              Controller = "Company",      Action = "Settings",        ParentId = 12, OrderIndex = 4 }
            };

            db.MenuItems.AddRange(menuItems);
            await db.SaveChangesAsync();

            // Administrador: all items
            var adminRole = await db.Roles.FirstAsync(r => r.Name == "Administrador");
            db.RoleMenuPermissions.AddRange(menuItems.Select(m => new RoleMenuPermission { RoleId = adminRole.Id, MenuItemId = m.Id, IsEnabled = true }));

            // Recepcionista: socios + cuotas/pagos + clases + rutinas + nutrición + reportes (sin Administración)
            var recepRole = await db.Roles.FirstAsync(r => r.Name == "Recepcionista");
            foreach (var id in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 16, 17, 18, 24, 26, 27, 28, 29, 30, 31, 32, 19, 20, 21, 22, 23 })
                db.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = recepRole.Id, MenuItemId = id, IsEnabled = true });

            // Instructor: socios + clases + rutinas + nutrición
            var instrRole = await db.Roles.FirstAsync(r => r.Name == "Instructor");
            foreach (var id in new[] { 1, 2, 3, 16, 17, 26, 27, 28, 29, 30, 31, 32 })
                db.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = instrRole.Id, MenuItemId = id, IsEnabled = true });

            await db.SaveChangesAsync();
        }
        else
        {
            await SeedNewMenuItemsAsync(db);
        }

        // ── Expirar MemberPlans vencidos que quedaron como Active ─────────────
        var expiredPlans = await db.MemberPlans
            .Where(mp => mp.Status == MemberPlanStatus.Active && mp.EndDate.Date < DateTime.Today)
            .ToListAsync();
        if (expiredPlans.Any())
        {
            foreach (var mp in expiredPlans)
                mp.Status = MemberPlanStatus.Expired;
            await db.SaveChangesAsync();
        }

        // ── Default plans ─────────────────────────────────────────────────────
        if (!await db.Plans.AnyAsync())
        {
            var plans = new List<Plan>
            {
                new() { Name = "Pase Diario",     PlanType = PlanType.Daily,   DurationDays = 1,  Price = 500,   Description = "Acceso por un día", IsActive = true },
                new() { Name = "Plan Semanal",    PlanType = PlanType.Weekly,  DurationDays = 7,  Price = 2000,  Description = "Acceso durante 7 días", IsActive = true },
                new() { Name = "Plan Mensual",    PlanType = PlanType.Monthly, DurationDays = 30, Price = 6000,  Description = "Acceso durante 30 días", IsActive = true },
                new() { Name = "Plan Mensual VIP",PlanType = PlanType.Monthly, DurationDays = 30, Price = 9000,  Description = "Acceso ilimitado + clases", IsActive = true }
            };
            db.Plans.AddRange(plans);
            await db.SaveChangesAsync();
        }

        // ── Ejercicios predefinidos ───────────────────────────────────────────
        if (!await db.Exercises.AnyAsync())
        {
            var exercises = new List<Exercise>
            {
                // Pecho
                new() { Name = "Press de Banca",          MuscleGroup = MuscleGroup.Chest,      ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Press de Banca Inclinado", MuscleGroup = MuscleGroup.Chest,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Aperturas con Mancuernas", MuscleGroup = MuscleGroup.Chest,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Fondos en Paralelas",      MuscleGroup = MuscleGroup.Chest,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Crossover en Polea",       MuscleGroup = MuscleGroup.Chest,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Espalda
                new() { Name = "Dominadas",                MuscleGroup = MuscleGroup.Back,       ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Remo con Barra",           MuscleGroup = MuscleGroup.Back,       ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Jalón al Pecho",           MuscleGroup = MuscleGroup.Back,       ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Remo en Polea Baja",       MuscleGroup = MuscleGroup.Back,       ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Peso Muerto",              MuscleGroup = MuscleGroup.Back,       ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Hombros
                new() { Name = "Press Militar con Barra",  MuscleGroup = MuscleGroup.Shoulders,  ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Elevaciones Laterales",    MuscleGroup = MuscleGroup.Shoulders,  ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Elevaciones Frontales",    MuscleGroup = MuscleGroup.Shoulders,  ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Pájaro / Elevaciones Posteriores", MuscleGroup = MuscleGroup.Shoulders, ExerciseType = ExerciseType.Strength, IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Bíceps
                new() { Name = "Curl con Barra",           MuscleGroup = MuscleGroup.Biceps,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Curl Martillo",            MuscleGroup = MuscleGroup.Biceps,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Curl Concentrado",         MuscleGroup = MuscleGroup.Biceps,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Curl en Polea Baja",       MuscleGroup = MuscleGroup.Biceps,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Tríceps
                new() { Name = "Press Francés",            MuscleGroup = MuscleGroup.Triceps,    ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Extensión en Polea Alta",  MuscleGroup = MuscleGroup.Triceps,    ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Fondos para Tríceps",      MuscleGroup = MuscleGroup.Triceps,    ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Cuádriceps
                new() { Name = "Sentadilla",               MuscleGroup = MuscleGroup.Quadriceps, ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Prensa de Piernas",        MuscleGroup = MuscleGroup.Quadriceps, ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Extensión de Cuádriceps",  MuscleGroup = MuscleGroup.Quadriceps, ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Zancada / Lunges",         MuscleGroup = MuscleGroup.Quadriceps, ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Femorales
                new() { Name = "Curl de Femorales",        MuscleGroup = MuscleGroup.Hamstrings, ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Peso Muerto Rumano",       MuscleGroup = MuscleGroup.Hamstrings, ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Pantorrillas
                new() { Name = "Elevación de Talones de Pie", MuscleGroup = MuscleGroup.Calves,  ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Elevación de Talones Sentado", MuscleGroup = MuscleGroup.Calves, ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Glúteos
                new() { Name = "Hip Thrust",               MuscleGroup = MuscleGroup.Glutes,     ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Patada de Glúteo en Polea", MuscleGroup = MuscleGroup.Glutes,    ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Abdominales
                new() { Name = "Crunch Abdominal",         MuscleGroup = MuscleGroup.Abs,        ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Plancha",                  MuscleGroup = MuscleGroup.Abs,        ExerciseType = ExerciseType.Functional,  IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Elevación de Piernas",     MuscleGroup = MuscleGroup.Abs,        ExerciseType = ExerciseType.Strength,    IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Russian Twist",            MuscleGroup = MuscleGroup.Abs,        ExerciseType = ExerciseType.Functional,  IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Cardio
                new() { Name = "Cinta / Trotadora",        MuscleGroup = MuscleGroup.Cardio,     ExerciseType = ExerciseType.Cardio,      IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Bicicleta Fija",           MuscleGroup = MuscleGroup.Cardio,     ExerciseType = ExerciseType.Cardio,      IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Elíptica",                 MuscleGroup = MuscleGroup.Cardio,     ExerciseType = ExerciseType.Cardio,      IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Remo Ergómetro",           MuscleGroup = MuscleGroup.Cardio,     ExerciseType = ExerciseType.Cardio,      IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Salto a la Soga",          MuscleGroup = MuscleGroup.Cardio,     ExerciseType = ExerciseType.Cardio,      IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                // Cuerpo Completo / Funcional
                new() { Name = "Burpees",                  MuscleGroup = MuscleGroup.FullBody,   ExerciseType = ExerciseType.Functional,  IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Sentadilla con Salto",     MuscleGroup = MuscleGroup.FullBody,   ExerciseType = ExerciseType.Functional,  IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Name = "Flexiones de Brazos",      MuscleGroup = MuscleGroup.FullBody,   ExerciseType = ExerciseType.Functional,  IsCustom = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            };
            db.Exercises.AddRange(exercises);
            await db.SaveChangesAsync();
        }
    }

    // ── Incremental menu seed ─────────────────────────────────────────────────
    private static async Task SeedNewMenuItemsAsync(ApplicationDbContext db)
    {
        // Canonical menu definition (source of truth)
        var canonical = new List<(int Id, string Name, string Icon, string? Url, string? Controller, string? Action, int? ParentId, int Order)>
        {
            (1,  "Dashboard",          "fas fa-tachometer-alt",       null, "Home",         "Index",           null, 1),
            (2,  "Socios",             "fas fa-users",                "#",  null,           null,              null, 2),
            (3,  "Listado",            "fas fa-list",                 null, "Members",      "Index",           2,    1),
            (4,  "Nuevo Socio",        "fas fa-user-plus",            null, "Members",      "Create",          2,    2),
            (5,  "Cuotas y Pagos",     "fas fa-credit-card",          "#",  null,           null,              null, 3),
            (6,  "Registrar Pago",     "fas fa-plus-circle",          null, "Payments",     "Create",          5,    1),
            (7,  "Historial",          "fas fa-history",              null, "Payments",     "Index",           5,    2),
            (8,  "Planes",             "fas fa-tags",                 null, "Plans",        "Index",           5,    3),
            (9,  "Vencimientos",       "fas fa-calendar-times",       null, "Alerts",       "Expirations",     5,    4),
            (10, "Alertas de Deuda",   "fas fa-exclamation-triangle", null, "Alerts",       "Debts",           5,    5),
            (11, "Caja Diaria",        "fas fa-cash-register",        null, "CashRegister", "Index",           5,    6),
            (16, "Clases",             "fas fa-chalkboard-teacher",   "#",  null,           null,              null, 4),
            (17, "Agenda Semanal",     "fas fa-calendar-week",        null, "Classes",      "Calendar",        16,   1),
            (18, "Gestión de Clases",  "fas fa-list-alt",             null, "Classes",         "Index",   16,   2),
            (24, "Categorías",        "fas fa-tags",                 null, "ClassCategories", "Index",   16,   3),
            (26, "Rutinas",            "fas fa-clipboard-list",       "#",  null,           null,              null, 5),
            (27, "Ejercicios",         "fas fa-dumbbell",             null, "Exercises",    "Index",           26,   1),
            (28, "Gestión de Rutinas", "fas fa-list-alt",             null, "Routines",     "Index",           26,   2),
            (29, "Asignaciones",       "fas fa-user-check",           null, "MemberRoutines",  "Index",         26,   3),
            (30, "Nutrición",         "fas fa-leaf",                 "#",  null,            null,              null, 6),
            (31, "Planes",            "fas fa-clipboard-list",       null, "NutritionPlans","Index",           30,   1),
            (32, "Asignaciones",      "fas fa-user-check",           null, "MemberNutrition","Index",          30,   2),
            (19, "Reportes",           "fas fa-chart-bar",            "#",  null,           null,              null, 7),
            (20, "Ingresos",           "fas fa-dollar-sign",          null, "Reports",      "Revenue",         19,   1),
            (21, "Socios Activos",     "fas fa-users",                null, "Reports",      "ActiveMembers",   19,   2),
            (22, "Deudores",           "fas fa-exclamation-circle",   null, "Reports",      "Debtors",         19,   3),
            (23, "Informe de Caja",    "fas fa-cash-register",        null, "Reports",      "Cash",            19,   4),
            (12, "Administración",     "fas fa-cog",                  "#",  null,           null,              null, 10),
            (13, "Roles",              "fas fa-shield-alt",           null, "Roles",        "Index",           12,   1),
            (14, "Usuarios",           "fas fa-user-cog",             null, "Users",        "Index",           12,   2),
            (15, "Permisos Menú",      "fas fa-bars",                 null, "Roles",        "MenuPermissions", 12,   3),
            (25, "Empresa",           "fas fa-building",             null, "Company",      "Settings",        12,   4)
        };

        var adminRole  = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Administrador");
        var recepRole  = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Recepcionista");
        var instrRole  = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Instructor");
        var existingPerms = await db.RoleMenuPermissions
            .Select(p => new { p.RoleId, p.MenuItemId }).ToListAsync();

        // ── Upsert each canonical item ────────────────────────────────────────
        foreach (var (id, name, icon, url, controller, action, parentId, order) in canonical)
        {
            var existing = await db.MenuItems.FindAsync(id);
            if (existing == null)
            {
                db.MenuItems.Add(new MenuItem
                {
                    Id = id, Name = name, Icon = icon, Url = url,
                    Controller = controller, Action = action,
                    ParentId = parentId, OrderIndex = order, IsActive = true
                });
            }
            else
            {
                // Repair name/routing if it was from an older seed
                existing.Name       = name;
                existing.Icon       = icon;
                existing.Url        = url;
                existing.Controller = controller;
                existing.Action     = action;
                existing.ParentId   = parentId;
                existing.OrderIndex = order;
                existing.IsActive   = true;
            }
        }
        await db.SaveChangesAsync();

        // ── Remove items with IDs not in canonical that don't belong ─────────
        var canonicalIds = canonical.Select(c => c.Id).ToHashSet();
        var orphans = await db.MenuItems.Where(m => !canonicalIds.Contains(m.Id)).ToListAsync();
        if (orphans.Any())
        {
            var orphanIds = orphans.Select(o => o.Id).ToList();
            var orphanPerms = db.RoleMenuPermissions.Where(p => orphanIds.Contains(p.MenuItemId));
            db.RoleMenuPermissions.RemoveRange(orphanPerms);
            db.MenuItems.RemoveRange(orphans);
            await db.SaveChangesAsync();
        }

        // ── Ensure permissions are correct ────────────────────────────────────
        void AddPermIfMissing(string roleId, int menuId)
        {
            if (!existingPerms.Any(p => p.RoleId == roleId && p.MenuItemId == menuId))
                db.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = roleId, MenuItemId = menuId, IsEnabled = true });
        }

        if (adminRole != null)
            foreach (var (id, _, _, _, _, _, _, _) in canonical)
                AddPermIfMissing(adminRole.Id, id);

        if (recepRole != null)
            foreach (var id in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 16, 17, 18, 24, 19, 20, 21, 22, 23, 26, 27, 28, 29, 30, 31, 32 })
                AddPermIfMissing(recepRole.Id, id);

        if (instrRole != null)
            foreach (var id in new[] { 1, 2, 3, 16, 17, 26, 27, 28, 29, 30, 31, 32 })
                AddPermIfMissing(instrRole.Id, id);

        await db.SaveChangesAsync();
    }
}
