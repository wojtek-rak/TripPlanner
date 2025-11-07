using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TripPlanner.Services;

namespace TripPlanner.Data.Repositories
{
    public interface ITripRepository
    {
        Task<List<TripDto>> GetByUserAsync(string userId);
        Task<TripDto> CreateAsync(string userId, TripCreateModel model);
        Task<TripDto?> UpdateAsync(int id, string userId, TripUpdateModel model);
        Task<bool> DeleteAsync(int id, string userId);

        // Added: return counts for All / Yours / Others (now accepts userId)
        Task<TripCounts> GetCountsAsync(string userId, CancellationToken ct = default);

        // Added: Trip plan (per-day) operations
        Task<List<TripDayPlanDto>> GenerateAsync(List<TripDayPlanDto> tripDays, CancellationToken ct = default);
        Task<List<TripDayPlanDto>> GetAsync(int tripId, CancellationToken ct = default);

        // Added: get single trip by id scoped to userId (AsNoTracking read)
        Task<TripDto?> GetByIdAsync(int id, string userId, CancellationToken ct = default);
    }

    public class TripDto
    {
        public TripDto(int id, string title, string city, string country, DateTime startDate, DateTime endDate, string? coverUrl, bool isOwner, string? notes)
        {
            Id = id;
            Title = title;
            City = city;
            Country = country;
            StartDate = startDate;
            EndDate = endDate;
            CoverUrl = coverUrl;
            Notes = notes;
            IsOwner = isOwner;
        }

        public int Id { get; set; }                  // auto-generated identity

        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? CoverUrl { get; set; }

        public string? Notes { get; set; }

        // Determines ownership for tabs (Your Trips vs Others' Trips)
        public bool IsOwner { get; set; }

        // Link to Identity user
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        /// <summary>
        /// Returns a human friendly duration string for the trip, e.g. "1 day" or "5 days".
        /// Matches UI usage in TripCard (calls <c>Trip.GetDurationText()</c>).
        /// </summary>
        public string GetDurationText()
        {
            // Calculate inclusive days (start and end are both part of the trip)
            var days = (EndDate.Date - StartDate.Date).Days + 1;

            if (days <= 0)
            {
                // Fallback if dates are invalid or equal in unexpected way
                return "0 days";
            }

            return days == 1 ? "1 day" : $"{days} days";
        }
    }

    public sealed record TripCreateModel(
        string Title,
        string City,
        string Country,
        DateTime StartDate,
        DateTime EndDate,
        string? CoverUrl,
        bool IsOwner,
        string? Notes);

    public sealed record TripUpdateModel(
        string Title,
        string City,
        string Country,
        DateTime StartDate,
        DateTime EndDate,
        string? CoverUrl,
        bool IsOwner,
        string? Notes);

    public sealed class TripCounts
    {
        public int All { get; set; }
        public int Yours { get; set; }
        public int Others { get; set; }
    }

    // Added: per-day plan DTO (aligned with TripPlanDay entity)
    public sealed record TripDayPlanDto(int TripId, int DayNumber, string? Summary, string? Description);
}