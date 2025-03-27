using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Savanna.Data.Entities;

namespace Savanna.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public bool IsUserLoggedIn { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _signInManager = signInManager;
        }

        public void OnGet()
        {
            IsUserLoggedIn = _signInManager.IsSignedIn(User);
        }
    }
}