using FitManager.Models;

namespace FitManager.ViewModels;

public class ClassCalendarViewModel
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd   { get; set; }
    public List<CalendarDayViewModel> Days { get; set; } = new();

    public DateTime PrevWeek => WeekStart.AddDays(-7);
    public DateTime NextWeek => WeekStart.AddDays(7);

    public bool IsCurrentWeek =>
        WeekStart.Date <= DateTime.Today && DateTime.Today <= WeekEnd.Date;
}

public class CalendarDayViewModel
{
    public DateTime Date  { get; set; }
    public List<CalendarSlotViewModel> Slots { get; set; } = new();

    public bool IsToday   => Date.Date == DateTime.Today;
    public bool IsWeekend => Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    public string DayName => Date.ToString("dddd");
}

public class CalendarSlotViewModel
{
    public int    ClassId        { get; set; }
    public string ClassName      { get; set; } = string.Empty;
    public ClassCategory Category { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string? Location      { get; set; }
    public TimeSpan StartTime    { get; set; }
    public TimeSpan EndTime      { get; set; }
    public string Color          { get; set; } = "#ff6b35";
    public int    EnrolledCount  { get; set; }
    public int?   MaxCapacity    { get; set; }
    public bool   IsFull         { get; set; }
    public string? ScheduleNotes { get; set; }

    public string CapacityText => MaxCapacity.HasValue
        ? $"{EnrolledCount}/{MaxCapacity}"
        : $"{EnrolledCount} inscriptos";

    public string Duration
    {
        get
        {
            var min = (int)(EndTime - StartTime).TotalMinutes;
            return min >= 60 ? $"{min / 60}h{(min % 60 > 0 ? $" {min % 60}min" : "")}" : $"{min}min";
        }
    }
}
