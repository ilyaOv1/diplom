using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjManagmentSystem.Pages
{
    public class ProfileModel : PageModel
    {
        public string Token { get; set; }
        public void OnGet()
        {
            if (Request.Cookies.TryGetValue("token", out var token))
            {
                Token = token; // Куки найдены
            }
            else
            {
                Token = "Куки не найдены.";
            }
        }
    }
}
