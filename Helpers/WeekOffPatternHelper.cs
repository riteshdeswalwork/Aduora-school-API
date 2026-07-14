using sms.api.Models.DTOs;

namespace sms.api.Helpers
{
    public static class WeekOffPatternHelper
    {
        public static bool MatchesDate(DateTime date, WeekOffDTO weekOff)
        {
            var d = date.Date;
            var from = weekOff.EffectiveFrom.Date;
            if (d < from) return false;
            if (weekOff.EffectiveTo.HasValue && d > weekOff.EffectiveTo.Value.Date) return false;

            var pattern = string.IsNullOrWhiteSpace(weekOff.PatternType) ? "EveryWeek" : weekOff.PatternType.Trim();

            if (string.Equals(pattern, "SingleDate", StringComparison.OrdinalIgnoreCase))
            {
                var end = weekOff.EffectiveTo?.Date ?? from;
                return d >= from && d <= end;
            }

            if ((int)d.DayOfWeek != weekOff.DayOfWeek) return false;

            if (string.Equals(pattern, "EveryWeek", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(pattern, "MonthlyNth", StringComparison.OrdinalIgnoreCase))
            {
                var occurrences = ParseOccurrences(weekOff.WeekOccurrences);
                if (occurrences.Count == 0) return false;
                var nth = GetNthWeekdayInMonth(d);
                var lastNth = GetLastWeekdayIndexInMonth(d);
                return occurrences.Any(o => o == nth || (o == 5 && nth == lastNth));
            }

            return false;
        }

        public static int GetNthWeekdayInMonth(DateTime date)
        {
            var count = 0;
            var cursor = new DateTime(date.Year, date.Month, 1);
            while (cursor.Month == date.Month)
            {
                if ((int)cursor.DayOfWeek == (int)date.DayOfWeek)
                {
                    count++;
                    if (cursor.Date == date.Date) return count;
                }
                cursor = cursor.AddDays(1);
            }
            return 0;
        }

        public static int GetLastWeekdayIndexInMonth(DateTime date)
        {
            var last = 0;
            var count = 0;
            var cursor = new DateTime(date.Year, date.Month, 1);
            while (cursor.Month == date.Month)
            {
                if ((int)cursor.DayOfWeek == (int)date.DayOfWeek)
                {
                    count++;
                    last = count;
                }
                cursor = cursor.AddDays(1);
            }
            return last;
        }

        public static List<int> ParseOccurrences(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<int>();
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .Where(n => n is >= 1 and <= 5)
                .Distinct()
                .ToList();
        }

        public static string PatternLabel(WeekOffDTO weekOff)
        {
            var pattern = string.IsNullOrWhiteSpace(weekOff.PatternType) ? "EveryWeek" : weekOff.PatternType;
            if (string.Equals(pattern, "SingleDate", StringComparison.OrdinalIgnoreCase))
                return "Specific date";
            if (string.Equals(pattern, "MonthlyNth", StringComparison.OrdinalIgnoreCase))
            {
                var occ = ParseOccurrences(weekOff.WeekOccurrences);
                var labels = occ.Select(o => o == 5 ? "Last" : $"{o}{OrdinalSuffix(o)}").ToList();
                return labels.Count > 0 ? string.Join(", ", labels) : "Monthly";
            }
            return "Every week";
        }

        private static string OrdinalSuffix(int n) => n switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }
}
