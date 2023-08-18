using System.Globalization;

namespace SharedServices.Extensions;

public static class DateTimeExtensions
{
    public static int GetWeekNumber(this DateTime dateTime)
    {
        var culture = new CultureInfo("nl-NL");
        return culture.Calendar.GetWeekOfYear(
            dateTime,
            culture.DateTimeFormat.CalendarWeekRule,
            culture.DateTimeFormat.FirstDayOfWeek);
    }
}