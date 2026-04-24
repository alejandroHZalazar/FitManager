using System.ComponentModel.DataAnnotations;
using FitManager.Models;

namespace FitManager.ViewModels;

// ── Filtro de fecha compartido ────────────────────────────────────────────────

public enum DateRangePreset
{
    Today        = 1,
    LastWeek     = 2,
    LastMonth    = 3,
    PreviousMonth= 4,
    ThisYear     = 5,
    Custom       = 6
}

public class ReportFilterViewModel
{
    public DateRangePreset Preset { get; set; } = DateRangePreset.LastMonth;

    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    /// <summary>Resuelve el rango real según el preset o las fechas custom.</summary>
    public (DateTime from, DateTime to) Resolve()
    {
        var today = DateTime.Today;
        return Preset switch
        {
            DateRangePreset.Today         => (today, today),
            DateRangePreset.LastWeek      => (today.AddDays(-6), today),
            DateRangePreset.LastMonth     => (today.AddDays(-29), today),
            DateRangePreset.PreviousMonth => (new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                                              new DateTime(today.Year, today.Month, 1).AddDays(-1)),
            DateRangePreset.ThisYear      => (new DateTime(today.Year, 1, 1), today),
            DateRangePreset.Custom        => (From ?? today.AddDays(-29), To ?? today),
            _                             => (today.AddDays(-29), today)
        };
    }

    public string PresetLabel => Preset switch
    {
        DateRangePreset.Today          => "Hoy",
        DateRangePreset.LastWeek       => "Última semana",
        DateRangePreset.LastMonth      => "Último mes",
        DateRangePreset.PreviousMonth  => "Mes pasado",
        DateRangePreset.ThisYear       => "Este año",
        DateRangePreset.Custom         => $"{From:dd/MM/yyyy} – {To:dd/MM/yyyy}",
        _                              => ""
    };
}

// ── Reporte 1: Ingresos ───────────────────────────────────────────────────────

public class RevenueReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public DateTime From  { get; set; }
    public DateTime To    { get; set; }

    // KPIs
    public decimal TotalRevenue     { get; set; }
    public decimal CashRevenue      { get; set; }
    public decimal CardRevenue      { get; set; }
    public decimal TransferRevenue  { get; set; }
    public int     PaymentCount     { get; set; }
    public decimal AverageTicket    { get; set; }

    // Detalle por mes (para gráfico)
    public List<MonthlyRevenue> ByMonth { get; set; } = new();

    // Detalle por plan
    public List<RevenueByPlan> ByPlan { get; set; } = new();

    // Detalle por método de pago
    public List<RevenueByMethod> ByMethod { get; set; } = new();

    // Tabla de pagos
    public List<Payment> Payments { get; set; } = new();
}

public class MonthlyRevenue
{
    public int     Year   { get; set; }
    public int     Month  { get; set; }
    public decimal Total  { get; set; }
    public int     Count  { get; set; }
    public string  Label  => $"{new DateTime(Year, Month, 1):MMM yyyy}";
}

public class RevenueByPlan
{
    public string  PlanName { get; set; } = string.Empty;
    public decimal Total    { get; set; }
    public int     Count    { get; set; }
    public decimal Percent  { get; set; }
}

public class RevenueByMethod
{
    public PaymentMethod Method  { get; set; }
    public decimal       Total   { get; set; }
    public int           Count   { get; set; }
    public decimal       Percent { get; set; }
    public string        Label   => Method switch
    {
        PaymentMethod.Cash     => "Efectivo",
        PaymentMethod.Card     => "Tarjeta",
        PaymentMethod.Transfer => "Transferencia",
        _                      => "Otro"
    };
    public string Color => Method switch
    {
        PaymentMethod.Cash     => "#198754",
        PaymentMethod.Card     => "#0d6efd",
        PaymentMethod.Transfer => "#fd7e14",
        _                      => "#6c757d"
    };
}

// ── Reporte 2: Socios activos ─────────────────────────────────────────────────

public class ActiveMembersReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public DateTime From  { get; set; }
    public DateTime To    { get; set; }

    // KPIs
    public int TotalActive        { get; set; }
    public int NewInPeriod        { get; set; }
    public int WithActivePlan     { get; set; }
    public int WithoutActivePlan  { get; set; }
    public int ExpiringIn5Days    { get; set; }

    // Por plan
    public List<MembersByPlan> ByPlan { get; set; } = new();

    // Tabla
    public List<ActiveMemberRow> Members { get; set; } = new();
}

public class MembersByPlan
{
    public string PlanName { get; set; } = string.Empty;
    public int    Count    { get; set; }
    public decimal Percent { get; set; }
}

public class ActiveMemberRow
{
    public int      Id            { get; set; }
    public string   MemberNumber  { get; set; } = string.Empty;
    public string   FullName      { get; set; } = string.Empty;
    public string?  PhotoPath     { get; set; }
    public string?  PlanName      { get; set; }
    public DateTime? PlanEndDate  { get; set; }
    public int?     DaysLeft      { get; set; }
    public DateTime JoinDate      { get; set; }
    public bool     IsNew         { get; set; }   // Se unió en el período
}

// ── Reporte 3: Deudores ───────────────────────────────────────────────────────

public class DebtorsReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public DateTime From  { get; set; }
    public DateTime To    { get; set; }

    // KPIs
    public int     TotalDebtors   { get; set; }
    public decimal TotalDebt      { get; set; }
    public decimal AvgDebt        { get; set; }
    public int     CriticalCount  { get; set; }   // > 30 días de mora

    // Tabla
    public List<DebtorRow> Debtors { get; set; } = new();
}

public class DebtorRow
{
    public int      MemberId      { get; set; }
    public string   MemberNumber  { get; set; } = string.Empty;
    public string   FullName      { get; set; } = string.Empty;
    public string?  PhotoPath     { get; set; }
    public string?  Phone         { get; set; }
    public string?  Email         { get; set; }
    public decimal  TotalDebt     { get; set; }
    public int      PaymentCount  { get; set; }
    public DateTime OldestDueDate { get; set; }
    public int      DaysOverdue   { get; set; }
    public string   Severity      => DaysOverdue switch
    {
        > 30 => "danger",
        > 15 => "warning",
        _    => "secondary"
    };
    public string SeverityLabel => DaysOverdue switch
    {
        > 30 => "Crítico",
        > 15 => "Moderado",
        _    => "Leve"
    };
}

// ── Reporte 4: Caja ───────────────────────────────────────────────────────────

public class CashReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public DateTime From  { get; set; }
    public DateTime To    { get; set; }

    // KPIs
    public decimal TotalIncome      { get; set; }
    public decimal TotalExpenses    { get; set; }
    public decimal TotalPayments    { get; set; }
    public decimal NetBalance       { get; set; }
    public int     RegistersCount   { get; set; }
    public int     ClosedRegisters  { get; set; }

    // Por día (gráfico)
    public List<DailyCash> ByDay { get; set; } = new();

    // Detalle de cajas
    public List<CashRegisterRow> Registers { get; set; } = new();
}

public class DailyCash
{
    public DateTime Date     { get; set; }
    public decimal  Income   { get; set; }
    public decimal  Expenses { get; set; }
    public decimal  Payments { get; set; }
    public string   Label    => Date.ToString("dd/MM");
}

public class CashRegisterRow
{
    public int      Id             { get; set; }
    public DateTime Date           { get; set; }
    public string   OpenedBy       { get; set; } = string.Empty;
    public string?  ClosedBy       { get; set; }
    public decimal  OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal  PaymentsTotal  { get; set; }
    public decimal  Income         { get; set; }
    public decimal  Expenses       { get; set; }
    public decimal  NetBalance     { get; set; }
    public CashRegisterStatus Status { get; set; }
    public bool     WasReopened    { get; set; }
}
