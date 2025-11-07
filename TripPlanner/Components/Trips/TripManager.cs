using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using TripPlanner.Data;
using TripPlanner.Data.Repositories;
using TripPlanner.Services;

namespace TripPlanner.Components.Trips
{
    public interface ITripManager
    {
        Task<List<TripDto>> ListAsync(string userId);
        Task<TripDto> CreateAsync(string userId, TripCreateModel model);
        Task<TripDto?> UpdateAsync(int id, string userId, TripUpdateModel model);
        Task<bool> RemoveAsync(int id, string userId);

        // Added: counts for All / Yours / Others scoped to userId
        Task<TripCounts> GetCountsAsync(string userId, CancellationToken ct = default);

        // Trip retrieval scoped to userId
        Task<TripDto?> GetAsync(int id, string userId, CancellationToken ct = default);

        // Added: Trip plan (per-day) operations
        Task<List<TripDayPlanDto>> GenerateAsync(Trip trip, CancellationToken ct = default);
        Task<List<TripDayPlanDto>> GetAsync(int tripId, CancellationToken ct = default);
    }

    public class TripManager : ITripManager
    {
        private readonly ITripRepository _repo;
        private readonly ITripPlanGeneratorService _generator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TripManager(
            ITripRepository repo,
            ITripPlanGeneratorService generator,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task<List<TripDto>> ListAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required", nameof(userId));
            return _repo.GetByUserAsync(userId);
        }

        public async Task<TripDto> CreateAsync(string userId, TripCreateModel model)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required", nameof(userId));
            if (model is null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.Title)) throw new ArgumentException("Title is required", nameof(model));
            if (string.IsNullOrWhiteSpace(model.City)) throw new ArgumentException("City is required", nameof(model));
            if (model.StartDate > model.EndDate) throw new ArgumentException("StartDate must be on or before EndDate", nameof(model));

            return await _repo.CreateAsync(userId, model);
        }

        public Task<TripDto?> UpdateAsync(int id, string userId, TripUpdateModel model)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required", nameof(userId));
            if (model is null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(model.Title)) throw new ArgumentException("Title is required", nameof(model));
            if (string.IsNullOrWhiteSpace(model.City)) throw new ArgumentException("City is required", nameof(model));
            if (model.StartDate > model.EndDate) throw new ArgumentException("StartDate must be on or before EndDate", nameof(model));

            return _repo.UpdateAsync(id, userId, model);
        }

        public Task<bool> RemoveAsync(int id, string userId)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required", nameof(userId));

            return _repo.DeleteAsync(id, userId);
        }

        public Task<TripCounts> GetCountsAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required", nameof(userId));
            return _repo.GetCountsAsync(userId, ct);
        }

        // New: get single trip scoped to userId
        public Task<TripDto?> GetAsync(int id, string userId, CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("userId is required", nameof(userId));
            return _repo.GetByIdAsync(id, userId, ct);
        }

        // Trip plan (per-day) operations
        public async Task<List<TripDayPlanDto>> GenerateAsync(Trip trip, CancellationToken ct = default)
        {
            if (trip is null) throw new ArgumentNullException(nameof(trip));
            if (trip.Id <= 0) throw new ArgumentOutOfRangeException(nameof(trip.Id));

            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            if (user is null) throw new InvalidOperationException("User not found.");

            return await _generator.GenerateAsync(trip, user, ct);
        }

        public Task<List<TripDayPlanDto>> GetAsync(int tripId, CancellationToken ct = default)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            return _repo.GetAsync(tripId, ct);
        }
    }
}