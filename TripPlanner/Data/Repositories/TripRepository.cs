using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Data.Entities;

namespace TripPlanner.Data.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public TripRepository(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<TripDto>> GetByUserAsync(string userId)
        {
            using var db = _dbFactory.CreateDbContext();
            return await db.Trips
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Select(t => new TripDto(
                    t.Id,
                    t.Title,
                    t.City,
                    t.Country,
                    t.StartDate,
                    t.EndDate,
                    t.CoverUrl,
                    t.IsOwner,
                    t.Notes))
                .ToListAsync();
        }

        public async Task<TripDto> CreateAsync(string userId, TripCreateModel model)
        {
            var entity = new Trip
            {
                Title = model.Title?.Trim() ?? string.Empty,
                City = model.City?.Trim() ?? string.Empty,
                Country = model.Country?.Trim() ?? string.Empty,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CoverUrl = model.CoverUrl,
                Notes = model.Notes,
                IsOwner = model.IsOwner,
                UserId = userId
            };

            using var db = _dbFactory.CreateDbContext();
            db.Trips.Add(entity);
            await db.SaveChangesAsync();

            return new TripDto(
                entity.Id,
                entity.Title,
                entity.City,
                entity.Country,
                entity.StartDate,
                entity.EndDate,
                entity.CoverUrl,
                entity.IsOwner,
                entity.Notes);
        }

        public async Task<TripDto?> UpdateAsync(int id, string userId, TripUpdateModel model)
        {
            using var db = _dbFactory.CreateDbContext();
            var entity = await db.Trips.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (entity is null)
            {
                return null;
            }

            entity.Title = model.Title?.Trim() ?? string.Empty;
            entity.City = model.City?.Trim() ?? string.Empty;
            entity.Country = model.Country?.Trim() ?? string.Empty;
            entity.StartDate = model.StartDate;
            entity.EndDate = model.EndDate;
            entity.CoverUrl = model.CoverUrl;
            entity.Notes = model.Notes;
            entity.IsOwner = model.IsOwner;

            await db.SaveChangesAsync();

            return new TripDto(
                entity.Id,
                entity.Title,
                entity.City,
                entity.Country,
                entity.StartDate,
                entity.EndDate,
                entity.CoverUrl,
                entity.IsOwner,
                entity.Notes);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            using var db = _dbFactory.CreateDbContext();
            var entity = await db.Trips.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (entity is null)
            {
                return false;
            }

            db.Trips.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }

        // Implemented: get counts (All / Yours / Others) via EF Core, now uses userId
        public async Task<TripCounts> GetCountsAsync(string userId, CancellationToken ct = default)
        {
            using var db = _dbFactory.CreateDbContext();
            // total
            var all = await db.Trips.CountAsync(ct);
            // trips owned by this user
            var yours = await db.Trips.CountAsync(t => t.UserId == userId && t.IsOwner, ct);
            var others = all - yours;

            return new TripCounts
            {
                All = all,
                Yours = yours,
                Others = others
            };
        }

        // Added: Trip plan (per-day) persistence
        public async Task<List<TripDayPlanDto>> GenerateAsync(List<TripDayPlanDto> tripDays, CancellationToken ct = default)
        {
            if (tripDays is null || tripDays.Count == 0)
                return new List<TripDayPlanDto>();

            var tripId = tripDays.First().TripId;

            using var db = _dbFactory.CreateDbContext();
            using var tx = await db.Database.BeginTransactionAsync(ct);

            try
            {
                // Remove existing plan rows for this trip
                var existing = db.Set<TripPlanDay>().Where(p => p.TripId == tripId);
                db.RemoveRange(existing);
                await db.SaveChangesAsync(ct);

                // Insert new rows
                var entities = tripDays
                    .OrderBy(d => d.DayNumber)
                    .Select(d => new TripPlanDay
                    {
                        TripId = d.TripId,
                        DayNumber = d.DayNumber,
                        Summary = d.Summary,
                        Description = d.Description
                    })
                    .ToList();

                await db.Set<TripPlanDay>().AddRangeAsync(entities, ct);
                await db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                // Return persisted plan in canonical order
                return await db.Set<TripPlanDay>()
                    .AsNoTracking()
                    .Where(p => p.TripId == tripId)
                    .OrderBy(p => p.DayNumber)
                    .Select(p => new TripDayPlanDto(p.TripId, p.DayNumber, p.Summary, p.Description))
                    .ToListAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<List<TripDayPlanDto>> GetAsync(int tripId, CancellationToken ct = default)
        {
            using var db = _dbFactory.CreateDbContext();
            return await db.Set<TripPlanDay>()
                .AsNoTracking()
                .Where(p => p.TripId == tripId)
                .OrderBy(p => p.DayNumber)
                .Select(p => new TripDayPlanDto(p.TripId, p.DayNumber, p.Summary, p.Description))
                .ToListAsync(ct);
        }

        // Added: single-trip read scoped to userId (AsNoTracking)
        public async Task<TripDto?> GetByIdAsync(int id, string userId, CancellationToken ct = default)
        {
            using var db = _dbFactory.CreateDbContext();
            return await db.Trips
                .AsNoTracking()
                .Where(t => t.Id == id && t.UserId == userId)
                .Select(t => new TripDto(
                    t.Id,
                    t.Title,
                    t.City,
                    t.Country,
                    t.StartDate,
                    t.EndDate,
                    t.CoverUrl,
                    t.IsOwner,
                    t.Notes))
                .SingleOrDefaultAsync(ct);
        }
    }
}