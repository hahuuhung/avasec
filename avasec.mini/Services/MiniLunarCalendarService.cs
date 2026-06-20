using AVASec.Mini.Models;

namespace AVASec.Mini.Services;

public static class MiniLunarCalendarService
{
    private static readonly string[] Can = ["Giáp", "Ất", "Bính", "Đinh", "Mậu", "Kỷ", "Canh", "Tân", "Nhâm", "Quý"];
    private static readonly string[] Chi = ["Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi", "Thân", "Dậu", "Tuất", "Hợi"];
    private static readonly string[] WeekdayVi = ["Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy"];
    private static readonly string[] WeekdayEn = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly int[] LunarInfo =
    [
        0x04bd8,0x04ae0,0x0a570,0x054d5,0x0d260,0x0d950,0x16554,0x056a0,0x09ad0,0x055d2,
        0x04ae0,0x0a5b6,0x0a4d0,0x0d250,0x1d255,0x0b540,0x0d6a0,0x0ada2,0x095b0,0x14977,
        0x04970,0x0a4b0,0x0b4b5,0x06a50,0x06d40,0x1ab54,0x02b60,0x09570,0x052f2,0x04970,
        0x06566,0x0d4a0,0x0ea50,0x06e95,0x05ad0,0x02b60,0x186e3,0x092e0,0x1c8d7,0x0c950,
        0x0d4a0,0x1d8a6,0x0b550,0x056a0,0x1a5b4,0x025d0,0x092d0,0x0d2b2,0x0a950,0x0b557,
        0x06ca0,0x0b550,0x15355,0x04da0,0x0a5b0,0x14573,0x052b0,0x0a9a8,0x0e950,0x06aa0,
        0x0aea6,0x0ab50,0x04b60,0x0aae4,0x0a570,0x05260,0x0f263,0x0d950,0x05b57,0x056a0,
        0x096d0,0x04dd5,0x04ad0,0x0a4d0,0x0d4d4,0x0d250,0x0d558,0x0b540,0x0b6a0,0x095b6,
        0x095b0,0x049b0,0x0a974,0x0a4b0,0x0b27a,0x06a50,0x06d40,0x0af46,0x0ab60,0x09570,
        0x04af5,0x04970,0x064b0,0x074a3,0x0ea50,0x06b58,0x055c0,0x0ab60,0x096d5,0x092e0,
        0x0c960,0x0d954,0x0d4a0,0x0da50,0x07552,0x056a0,0x0abb7,0x025d0,0x092d0,0x0cab5,
        0x0a950,0x0b4a0,0x0baa4,0x0ad50,0x055d9,0x04ba0,0x0a5b0,0x15176,0x052b0,0x0a930,
        0x07954,0x06aa0,0x0ad50,0x05b52,0x04b60,0x0a6e6,0x0a4e0,0x0d260,0x0ea65,0x0d530,
        0x05aa0,0x076a3,0x096d0,0x04afb,0x04ad0,0x0a4d0,0x1d0b6,0x0d250,0x0d520,0x0dd45,
        0x0b5a0,0x056d0,0x055b2,0x049b0,0x0a577,0x0a4b0,0x0aa50,0x1b255,0x06d20,0x0ada0,
        0x14b63,0x09370,0x049f8,0x04970,0x064b0,0x168a6,0x0ea50,0x06b20,0x1a6c4,0x0aae0,
        0x0a2e0,0x0d2e3,0x0c960,0x0d557,0x0d4a0,0x0da50,0x05d55,0x056a0,0x0a6d0,0x055d4,
        0x052d0,0x0a9b8,0x0a950,0x0b4a0,0x0b6a6,0x0ad50,0x055a0,0x0aba4,0x0a5b0,0x052b0,
        0x0b273,0x06930,0x07337,0x06aa0,0x0ad50,0x14b55,0x04b60,0x0a570,0x054e4,0x0d160,
        0x0e968,0x0d520,0x0daa0,0x16aa6,0x056d0,0x04ae0,0x0a9d4,0x0a2d0,0x0d150,0x0f252,
        0x0d520
    ];

    public static LunarCalendarInfo GetToday() => GetForDate(DateTime.Now);

    public static LunarCalendarInfo GetForDate(DateTime date)
    {
        var solar = date.Date;
        var lunar = ConvertSolarToLunar(solar.Day, solar.Month, solar.Year);
        string yearCanChi = GetCanChiYear(lunar.Year);
        string monthCanChi = GetCanChiMonth(lunar.Year, lunar.Month, lunar.Leap);
        string dayCanChi = GetCanChiDay(solar);
        var festival = GetFestival(lunar.Day, lunar.Month, solar.Month, solar.Day);

        return new LunarCalendarInfo
        {
            SolarDate = solar,
            SolarLineVi = $"{WeekdayVi[(int)solar.DayOfWeek]}, {solar:dd/MM/yyyy}",
            SolarLineEn = $"{WeekdayEn[(int)solar.DayOfWeek]}, {solar:MMMM dd, yyyy}",
            LunarDay = lunar.Day,
            LunarMonth = lunar.Month,
            LunarYear = lunar.Year,
            IsLeapMonth = lunar.Leap,
            LunarLineVi = lunar.Leap
                ? $"Âm lịch: {lunar.Day}/{lunar.Month} (nhuận) năm {yearCanChi}"
                : $"Âm lịch: ngày {lunar.Day} tháng {lunar.Month} năm {yearCanChi}",
            LunarLineEn = lunar.Leap
                ? $"Lunar: {lunar.Day}/{lunar.Month} (leap) - {yearCanChi} year"
                : $"Lunar: day {lunar.Day} month {lunar.Month} - {yearCanChi} year",
            DayCanChiVi = $"Can chi ngày / Day pillar: {dayCanChi}",
            DayCanChiEn = $"Day pillar: {dayCanChi}",
            MonthCanChiVi = $"Can chi tháng / Month pillar: {monthCanChi}",
            YearCanChiVi = yearCanChi,
            FestivalVi = festival.vi,
            FestivalEn = festival.en
        };
    }

    private static string GetCanChiYear(int lunarYear) => $"{Can[(lunarYear + 6) % 10]} {Chi[(lunarYear + 8) % 12]}";

    private static string GetCanChiMonth(int lunarYear, int lunarMonth, bool leap)
    {
        _ = leap;
        return $"{Can[(lunarYear * 12 + lunarMonth + 3) % 10]} {Chi[(lunarMonth + 1) % 12]}";
    }

    private static string GetCanChiDay(DateTime date)
    {
        var baseDate = new DateTime(1900, 1, 31);
        int days = (int)(date.Date - baseDate).TotalDays;
        return $"{Can[(days + 9) % 10]} {Chi[(days + 1) % 12]}";
    }

    private static (string vi, string en) GetFestival(int lDay, int lMonth, int sMonth, int sDay)
    {
        if (lDay == 1 && lMonth == 1) return ("Mùng 1 Tết / Lunar New Year", "Lunar New Year");
        if (lDay == 15 && lMonth == 1) return ("Rằm tháng Giêng / First full moon", "First lunar full moon");
        if (lDay == 15 && lMonth == 7) return ("Lễ Vu Lan / Ghost festival", "Vu Lan festival");
        if (lDay == 15 && lMonth == 8) return ("Tết Trung Thu / Mid-Autumn", "Mid-Autumn festival");
        if (sMonth == 1 && sDay == 1) return ("Tết Dương lịch / New Year", "Gregorian New Year");
        if (sMonth == 4 && sDay == 30) return ("Giải phóng miền Nam / Reunification", "Reunification Day");
        if (sMonth == 5 && sDay == 1) return ("Quốc tế Lao động / Labor Day", "Labor Day");
        if (sMonth == 9 && sDay == 2) return ("Quốc khánh / National Day", "National Day");
        return ("Ngày thường / Regular day", "Regular day");
    }

    private sealed record LunarParts(int Day, int Month, int Year, bool Leap);

    private static LunarParts ConvertSolarToLunar(int dd, int mm, int yyyy)
    {
        int dayNumber = JulianDay(dd, mm, yyyy);
        int k = (int)((dayNumber - 2415021.076998695) / 29.530588853);
        int monthStart = GetNewMoonDay(k + 1);
        if (monthStart > dayNumber)
            monthStart = GetNewMoonDay(k);

        int a11 = GetLunarMonth11(yyyy);
        int lunarYear;
        if (a11 >= monthStart)
        {
            lunarYear = yyyy;
            a11 = GetLunarMonth11(yyyy - 1);
        }
        else
        {
            lunarYear = yyyy + 1;
        }

        int lunarDay = dayNumber - monthStart + 1;
        int diff = (int)((monthStart - a11) / 29.0);
        int lunarMonth = diff + 11;
        bool leap = false;

        if (diff >= 12)
        {
            int leapMonth = GetLeapMonth(lunarYear);
            if (leapMonth > 0 && lunarMonth >= leapMonth)
            {
                lunarMonth--;
                if (lunarMonth == leapMonth)
                    leap = true;
            }
        }

        if (lunarMonth > 12)
            lunarMonth -= 12;
        if (lunarMonth >= 11 && diff < 4)
            lunarYear--;

        return new LunarParts(lunarDay, lunarMonth, lunarYear, leap);
    }

    private static int GetLeapMonth(int year) => year is < 1900 or > 2100 ? 0 : LunarInfo[year - 1900] & 0xF;

    private static int GetLunarMonth11(int year)
    {
        int off = JulianDay(31, 12, year) - 2415021;
        int k = (int)(off / 29.530588853);
        return GetNewMoonDay(k);
    }

    private static int GetNewMoonDay(int k)
    {
        double t = k / 1236.85;
        double t2 = t * t;
        double t3 = t2 * t;
        double jd = 2415020.75933 + 29.53058868 * k + 0.0001178 * t2 - 0.000000155 * t3;
        jd += 0.00033 * Math.Sin((166.56 + 132.87 * t - 0.009173 * t2) * Math.PI / 180);
        double m = 359.2242 + 29.10535608 * k - 0.0000333 * t2 - 0.00000347 * t3;
        double mpr = 306.0253 + 385.816918 * k + 0.0107306 * t2 + 0.00001236 * t3;
        double f = 21.6844 + 390.67050284 * k - 0.0016528 * t2 - 0.00000239 * t3;
        double c1 = (0.1734 - 0.000393 * t) * Math.Sin(m * Math.PI / 180) + 0.0021 * Math.Sin(2 * m * Math.PI / 180);
        double c2 = 0.01 * Math.Sin(mpr * Math.PI / 180) + 0.015 * Math.Sin(2 * mpr * Math.PI / 180);
        double c3 = 0.0028 * Math.Sin(2 * f * Math.PI / 180);
        return (int)(jd + c1 - c2 + c3 + 0.5 + 7 / 24.0);
    }

    private static int JulianDay(int dd, int mm, int yyyy)
    {
        int a = (14 - mm) / 12;
        int y = yyyy + 4800 - a;
        int m = mm + 12 * a - 3;
        return dd + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
    }
}