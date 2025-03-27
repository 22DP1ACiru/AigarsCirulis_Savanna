using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Savanna.Core.Interfaces;
using Savanna.Data.Entities;

namespace Savanna.Web.Pages.Game
{
    [Authorize]
    public class PlayModel : PageModel
    {
        private readonly IGameSessionService _sessionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PlayModel> _logger;

        public PlayModel(
            IGameSessionService sessionService,
            UserManager<ApplicationUser> userManager,
            ILogger<PlayModel> logger)
        {
            _sessionService = sessionService;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string SessionId { get; set; }

        // Flag to indicate if the current user is authorized for this session
        public bool IsValidSession { get; private set; } = false;
        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(SessionId))
            {
                _logger.LogWarning("Attempted to access game play page without SessionId.");
                ErrorMessage = "No game session specified.";
                IsValidSession = false;
                return Page(); // Show error on the page
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var session = _sessionService.GetOrCreateSession(SessionId);

            if (session.OwnerUserId == null)
            {
                // If the session was just created by GetOrCreateSession, it won't have an owner yet.
                // This means the user likely navigated directly to a non-existent/invalid session URL.
                _logger.LogWarning("Access denied. Session {SessionId} has no owner assigned.", SessionId);
                ErrorMessage = "Invalid game session specified or session has expired.";
                IsValidSession = false;
                _sessionService.RemoveSession(SessionId);
                return Page();
            }
            else if (session.OwnerUserId == user.Id)
            {
                // User is authorized for this session
                IsValidSession = true;
                _logger.LogInformation("User {UserId} authorized for session {SessionId}.", user.Id, SessionId);
                return Page();
            }
            else
            {
                // Session exists but belongs to someone else
                _logger.LogWarning("Unauthorized access attempt by User {UserId} to Session {SessionId} owned by {OwnerId}.", user.Id, SessionId, session.OwnerUserId);
                ErrorMessage = "You do not have permission to access this game session.";
                IsValidSession = false;
                return Page(); // Show error on the page
            }
        }
    }
}