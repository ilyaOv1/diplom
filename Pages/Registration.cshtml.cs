using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjManagmentSystem.Pages
{
    public class RegistrationModel : PageModel
    {

		[BindProperty]
		public string Password { get; set; }
        [BindProperty]
        public string RepeatPassword { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (Password != RepeatPassword)
            {
                ModelState.AddModelError("RepeatPassword", "Пароли не совпадают.");
                return RedirectToPage("/Authorization");
            }

            return Page();
        }
    }
}
