using System;

namespace TripPlanner.Data
{
    public class Trip
    {
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
}