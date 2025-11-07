using System;
using System.Globalization;

namespace TripPlanner.Utilities;

public static class TripDurationExtensions
{
    private static readonly CultureInfo EnUS = new("en-US");

    public static string ToShortRangeString(this DateTime start, DateTime end)
    {
        if (start.Year == end.Year)
        {
            if (start.Month == end.Month)
            {
                // Sep 5–9, 2025
                return $"{start.ToString("MMM d", EnUS)}–{end.ToString("d, yyyy", EnUS)}";
            }
            // Apr 27–May 3, 2025
            return $"{start.ToString("MMM d", EnUS)}–{end.ToString("MMM d, yyyy", EnUS)}";
        }

        // Dec 30, 2025–Jan 2, 2026
        return $"{start.ToString("MMM d, yyyy", EnUS)}–{end.ToString("MMM d, yyyy", EnUS)}";
    }
}