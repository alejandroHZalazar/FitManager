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
                // ── Administración
                new() { Id = 12, Name = "Administración",  Icon = "fas fa-cog",            Url = "#",                        OrderIndex = 10 },
                new() { Id = 13, Name = "Roles",            Icon = "fas fa-shield-alt",           Controller = "Roles",        Action = "Index",           ParentId = 12, OrderIndex = 1 },
                new() { Id = 14, Name = "Usuarios",         Icon = "fas fa-user-cog",             Controller = "Users",        Action = "Index",           ParentId = 12, OrderIndex = 2 },
                new() { Id = 15, Name = "Permisos Menú",   Icon = "fas fa-bars",                 Controller = "Roles",        Action = "MenuPermissions", ParentId = 12, OrderIndex = 3 }
            };

            db.MenuItems.AddRange(menuItems);
            await db.SaveChangesAsync();

            // Administrador: all items
            var adminRole = await db.Roles.FirstAsync(r => r.Name == "Administrador");
            db.RoleMenuPermissions.AddRange(menuItems.Select(m => new RoleMenuPermission { RoleId = adminRole.Id, MenuItemId = m.Id, IsEnabled = true }));

            // Recepcionista: socios + cuotas/pagos + clases (sin Administración)
            var recepRole = await db.Roles.FirstAsync(r => r.Name == "Recepcionista");
            foreach (var id in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 16, 17, 18 })
                db.RoleMenuPermissions.Add(new RoleMenuPermission { RoleId = recepRole.Id, MenuItemId = id, IsEnabled = true });

            // Instructor: socios + agenda de clases
            var instrRole = await db.Roles.FirstAsync(r => r.Name == "Instructor");
            foreach (var id in new[] { 1, 2, 3, 16, 17 })
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
            (18, "Gestión de Clases",  "fas fa-list-alt",             null, "Classes",      "Index",           16,   2),
            (12, "Administración",     "fas fa-cog",                  "#",  null,           null,              null, 10),
            (13, "Roles",              "fas fa-shield-alt",           null, "Roles",        "Index",           12,   1),
            (14, "Usuarios",           "fas fa-user-cog",             null, "Users",        "Index",           12,   2),
            (15, "Permisos Menú",      "fas fa-bars",                 null, "Roles",        "MenuPermissions", 12,   3)
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
            foreach (var id in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 16, 17, 18 })
                AddPermIfMissing(recepRole.Id, id);

        if (instrRole != null)
            foreach (var id in new[] { 1, 2, 3, 16, 17 })
                AddPermIfMissing(instrRole.Id, id);

        await db.SaveChangesAsync();
    }
}
