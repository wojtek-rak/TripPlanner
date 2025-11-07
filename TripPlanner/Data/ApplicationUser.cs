using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TripPlanner.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        // Navigation for user's trips
        public IList<Trip> Trips { get; set; } = new List<Trip>();

        /// <summary>
        /// Optional API key (e.g. for external services).
        /// Stored as nullable string to avoid breaking existing accounts.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Optional service endpoint (URL) associated with the user.
        /// </summary>
        public string? Endpoint { get; set; }

        // ---- OpenAI integration (backed by existing fields where possible) ----

        /// <summary>
        /// Endpoint alias for OpenAI/Azure OpenAI. Backed by Endpoint to avoid a migration.
        /// </summary>
        public string OpenAIEndpoint
        {
            get => Endpoint ?? string.Empty;
            set => Endpoint = value;
        }

        /// <summary>
        /// API key alias for OpenAI/Azure OpenAI. Backed by ApiKey to avoid a migration.
        /// </summary>
        public string OpenAIApiKey
        {
            get => ApiKey ?? string.Empty;
            set => ApiKey = value;
        }

        /// <summary>
        /// Optional: preferred model id or Azure deployment name.
        /// </summary>
        public string? OpenAIModel { get; set; }

        /// <summary>
        /// Optional: explicit toggle for Azure OpenAI routing. If null, inferred from endpoint.
        /// </summary>
        public bool? IsAzureOpenAI { get; set; }
    }
}
