using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Savanna.Core.Interfaces;
using Savanna.Core.Models.DTOs;
using Savanna.Data.Entities;

namespace Savanna.Web.Pages.Game
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IGameSaveService _gameSaveService;
        private readonly IGameSessionService _gameSessionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IGameSaveService gameSaveService,
            IGameSessionService gameSessionService,
            UserManager<ApplicationUser> userManager)
        {
            _gameSaveService = gameSaveService;
            _gameSessionService = gameSessionService;
            _userManager = userManager;
        }

        // Property to hold the list of saves for the view
        public IEnumerable<GameSaveDto> UserSaves { get; set; } = new List<GameSaveDto>();

        // Property for displaying messages (e.g., load/delete status)
        [TempData]
        public string InfoMessage { get; set; }

        // --- Page Handlers ---
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserSaves = await _gameSaveService.GetUserSaves(user.Id);
            }
        }

        public async Task<IActionResult> OnPostStartNewGameAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(); // Should not happen if [Authorize] works
            }

            string newSessionId = Guid.NewGuid().ToString("N");
            var session = _gameSessionService.GetOrCreateSession(newSessionId);
            session.OwnerUserId = user.Id;
            session.InitializeNewGame();

            // Redirect to the Play page for the new session
            return RedirectToPage("Play", new { sessionId = newSessionId });
        }

        public async Task<IActionResult> OnPostLoadGameAsync(int saveId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            string newSessionId = await _gameSaveService.LoadGameState(saveId, user.Id);

            if (newSessionId != null)
            {
                // Redirect to the Play page for the loaded session
                return RedirectToPage("Play", new { sessionId = newSessionId });
            }
            else
            {
                InfoMessage = "Failed to load the selected game. It might be corrupted or no longer exist.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteGameAsync(int saveId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                try
                {
                    await _gameSaveService.DeleteSave(saveId, user.Id);
                    InfoMessage = "Game save deleted successfully.";
                }
                catch (Exception ex) // Catch potential errors from service
                {
                    InfoMessage = "An error occurred while deleting the game save.";
                }
            }
            return RedirectToPage();
        }
    }
}