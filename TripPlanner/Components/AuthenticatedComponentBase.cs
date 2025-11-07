using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace TripPlanner.Components.Common
{
    /// <summary>
    /// Reusable base component that centralizes retrieving the current user's id
    /// and a small authenticated state helper for pages/components.
    /// </summary>
    public abstract class AuthenticatedComponentBase : ComponentBase
    {
        [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

        protected string? UserId { get; private set; }
        protected bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
        protected string? AuthError { get; private set; }

        /// <summary>
        /// Ensures the user is authenticated and populates <see cref="UserId"/>.
        /// Returns true when authenticated, false otherwise (and sets <see cref="AuthError"/>).
        /// Call from lifecycle methods or event handlers before using UserId.
        /// </summary>
        protected async Task<bool> EnsureAuthenticatedAsync()
        {
            AuthError = null;
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var uid = authState.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(uid))
            {
                UserId = null;
                AuthError = "Not authenticated.";
                return false;
            }

            UserId = uid;
            return true;
        }
    }
}