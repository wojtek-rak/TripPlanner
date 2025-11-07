using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using TripPlanner.Data;
using TripPlanner.Data.Repositories;

namespace TripPlanner.Services
{
    public sealed class TripPlanGeneratorService : ITripPlanGeneratorService
    {
        private readonly ITripRepository _repo;
        private readonly JsonSerializerOptions _json;

        public TripPlanGeneratorService(ITripRepository repo)
        {
            _repo = repo;
            _json = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<List<TripDayPlanDto>> GenerateAsync(
            Trip trip,
            ApplicationUser user,
            CancellationToken ct = default)
        {
            if (trip is null) throw new ArgumentNullException(nameof(trip));
            if (trip.Id <= 0) throw new ArgumentOutOfRangeException(nameof(trip.Id));
            if (trip.StartDate == default || trip.EndDate == default || trip.EndDate < trip.StartDate)
                throw new ArgumentException("Trip dates are invalid.");
            if (string.IsNullOrWhiteSpace(user.OpenAIEndpoint))
                throw new InvalidOperationException("OpenAI endpoint is not configured for this user.");
            if (string.IsNullOrWhiteSpace(user.OpenAIApiKey))
                throw new InvalidOperationException("OpenAI API key is not configured for this user.");

            // Day count (inclusive)
            var dayCount = (int)Math.Ceiling((trip.EndDate.Date - trip.StartDate.Date).TotalDays) + 1;

            // Azure OpenAI client (endpoint + api-key)
            var azureClient = new AzureOpenAIClient(
                new Uri(user.OpenAIEndpoint),
                new AzureKeyCredential(user.OpenAIApiKey));

            // Use user's deployment name/model when provided; default to "gpt-4.1"
            var deployment = string.IsNullOrWhiteSpace(user.OpenAIModel) ? "gpt-4.1" : user.OpenAIModel!;
            ChatClient chatClient = azureClient.GetChatClient(deployment);

            // Messages
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(BuildUserPrompt(trip, dayCount))
            };

            // Request options
            var options = new ChatCompletionOptions
            {
                Temperature = 0.5f
                // Azure chat/completions may ignore response_format; the system prompt enforces strict JSON.
            };

            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options, ct);
            string content = completion?.Content?.FirstOrDefault()?.Text ?? "";

            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("OpenAI returned empty content.");

            // Parse JSON
            TripPlanResponse? plan = null;
            try
            {
                plan = JsonSerializer.Deserialize<TripPlanResponse>(content!, _json);
            }
            catch
            {
                // Fallback: extract object defensively
                var start = content!.IndexOf('{');
                var end = content!.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    var json = content.Substring(start, end - start + 1);
                    plan = JsonSerializer.Deserialize<TripPlanResponse>(json, _json);
                }
            }

            if (plan?.Days is null || plan.Days.Count == 0)
                throw new InvalidOperationException("Failed to parse trip plan JSON.");

            // Normalize to exact dayCount
            var normalized = Normalize(plan.Days, dayCount, trip.Id);

            // Persist and return
            return await _repo.GenerateAsync(normalized, ct);
        }

        private static List<TripDayPlanDto> Normalize(List<TripPlanItem> incoming, int dayCount, int tripId)
        {
            var cleaned = incoming
                .Where(d => d is not null && d.DayNumber >= 1)
                .OrderBy(d => d.DayNumber)
                .GroupBy(d => d.DayNumber)
                .Select(g => g.First())
                .ToList();

            var result = new List<TripDayPlanDto>(capacity: dayCount);
            for (int i = 1; i <= dayCount; i++)
            {
                var found = cleaned.FirstOrDefault(d => d.DayNumber == i);
                if (found is null)
                {
                    result.Add(new TripDayPlanDto(tripId, i, "Plan this day",
                        "This placeholder was added to meet the exact day count. Please regenerate or edit."));
                }
                else
                {
                    var summary = string.IsNullOrWhiteSpace(found.Summary) ? $"Day {i}" : found.Summary!;
                    var description = string.IsNullOrWhiteSpace(found.Description) ? "Details TBD." : found.Description!;
                    result.Add(new TripDayPlanDto(tripId, i, summary, description));
                }
            }
            return result;
        }

        // ===== Prompts =====
        private const string SystemPrompt = """
You are a precise travel-planning assistant.
Your task: produce a day-by-day itinerary for the given city, country, and date range.
Rules:
- Return STRICT JSON only (no prose), matching the schema:
  {
    "days": [
      { "dayNumber": 1, "summary": "string", "description": "string (1–3 sentences)" }
    ]
  }
- The number of items in "days" MUST equal the number of calendar days between StartDate and EndDate inclusive.
- Day numbering starts at 1 and increases by 1.
- "summary" is a short title (max ~60 chars). 
- "description" is 1–3 sentences with specific, practical activities in the given destination (no hyperlinks).
- Keep logistics realistic (cluster nearby sights, mix iconic/hidden gems, include breaks).
- Avoid duplicates. If a sight is closed on a weekday, propose an alternative.
- Language: same as input if it looks like Polish; otherwise use English.
- Do not include prices or bookings.
- Output strictly valid JSON. Do not include markdown fences.
""";

        private static string BuildUserPrompt(Trip trip, int dayCount) => $"""
City: {trip.City}
Country: {trip.Country}
StartDate: {trip.StartDate:yyyy-MM-dd}
EndDate: {trip.EndDate:yyyy-MM-dd}
TotalDays: {dayCount}

Produce a plan of exactly {dayCount} days for {trip.City}, {trip.Country}, covering every day from StartDate to EndDate (inclusive). 
""";

        // ===== Response mapping =====
        private sealed class TripPlanResponse
        {
            [JsonPropertyName("days")]
            public List<TripPlanItem> Days { get; set; } = new();
        }

        private sealed class TripPlanItem
        {
            [JsonPropertyName("dayNumber")]
            public int DayNumber { get; set; }
            [JsonPropertyName("summary")]
            public string? Summary { get; set; }
            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }
    }
}