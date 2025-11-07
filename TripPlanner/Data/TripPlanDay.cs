using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TripPlanner.Data.Entities;

/// <summary>
/// Per-day plan entry for a Trip. Composite key: (TripId, DayNumber).
/// </summary>
[PrimaryKey(nameof(TripId), nameof(DayNumber))]
public class TripPlanDay
{
    /// <summary>FK to Trips.Id</summary>
    public int TripId { get; set; }

    /// <summary>1-based day number within the trip duration</summary>
    [Range(1, int.MaxValue)]
    public int DayNumber { get; set; }

    /// <summary>Short per-day summary (varchar(max))</summary>
    public string? Summary { get; set; }

    /// <summary>Detailed per-day description (varchar(max))</summary>
    public string? Description { get; set; }
}