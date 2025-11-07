using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TripPlanner.Data.Repositories;

namespace Microsoft.AspNetCore.Routing
{
    internal static class TripComponentsEndpointRouteBuilderExtensions
    {
        // Minimal API endpoints for Trips (List, Add, Edit, Delete).
        public static IEndpointConventionBuilder MapTripEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var tripGroup = endpoints.MapGroup("/Trips").RequireAuthorization();

            // List trips for current user
            tripGroup.MapGet("/ListTrips", async (ClaimsPrincipal user, [FromServices] ITripRepository repo) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var trips = await repo.GetByUserAsync(userId);
                return TypedResults.Ok(trips);
            });

            // Add a new trip
            tripGroup.MapPost("/Add", async (
                ClaimsPrincipal user,
                [FromServices] ITripRepository repo,
                [FromBody] TripCreateModel model) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    return Results.BadRequest(new { Error = "Title is required." });
                }

                if (string.IsNullOrWhiteSpace(model.City))
                {
                    return Results.BadRequest(new { Error = "City is required." });
                }

                if (model.StartDate > model.EndDate)
                {
                    return Results.BadRequest(new { Error = "StartDate must be on or before EndDate." });
                }

                var created = await repo.CreateAsync(userId, model);
                return TypedResults.Created($"/Trips/{created.Id}", created);
            });

            // Edit an existing trip
            tripGroup.MapPut("/Edit/{id:int}", async (
                int id,
                ClaimsPrincipal user,
                [FromServices] ITripRepository repo,
                [FromBody] TripUpdateModel model) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    return Results.BadRequest(new { Error = "Title is required." });
                }

                if (string.IsNullOrWhiteSpace(model.City))
                {
                    return Results.BadRequest(new { Error = "City is required." });
                }

                if (model.StartDate > model.EndDate)
                {
                    return Results.BadRequest(new { Error = "StartDate must be on or before EndDate." });
                }

                var updated = await repo.UpdateAsync(id, userId, model);
                if (updated is null)
                {
                    return Results.NotFound();
                }

                return TypedResults.Ok(updated);
            });

            // Delete a trip
            tripGroup.MapDelete("/Delete/{id:int}", async (
                int id,
                ClaimsPrincipal user,
                [FromServices] ITripRepository repo) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var deleted = await repo.DeleteAsync(id, userId);
                return deleted ? Results.NoContent() : Results.NotFound();
            });

            return tripGroup;
        }
    }
}