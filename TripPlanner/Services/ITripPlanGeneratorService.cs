using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TripPlanner.Data;
using TripPlanner.Data.Repositories;

namespace TripPlanner.Services
{
    public interface ITripPlanGeneratorService
    {
        Task<List<TripDayPlanDto>> GenerateAsync(
            Trip trip,
            ApplicationUser user,
            CancellationToken ct = default);
    }
}