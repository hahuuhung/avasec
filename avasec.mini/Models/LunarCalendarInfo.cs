namespace AVASec.Mini.Models;

public sealed class LunarCalendarInfo
{
    public DateTime SolarDate { get; init; }
    public string SolarLineVi { get; init; } = string.Empty;
    public string SolarLineEn { get; init; } = string.Empty;
    public int LunarDay { get; init; }
    public int LunarMonth { get; init; }
    public int LunarYear { get; init; }
    public bool IsLeapMonth { get; init; }
    public string LunarLineVi { get; init; } = string.Empty;
    public string LunarLineEn { get; init; } = string.Empty;
    public string DayCanChiVi { get; init; } = string.Empty;
    public string DayCanChiEn { get; init; } = string.Empty;
    public string MonthCanChiVi { get; init; } = string.Empty;
    public string YearCanChiVi { get; init; } = string.Empty;
    public string FestivalVi { get; init; } = string.Empty;
    public string FestivalEn { get; init; } = string.Empty;
}