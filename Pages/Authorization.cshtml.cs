using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Services;

namespace ProjManagmentSystem.Pages
{
    public class AuthorizationModel : BasePageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Message { get; set; }
        private readonly CookieService cookieService = new CookieService();
        public AuthorizationModel(IHttpClientFactory httpClientFactory) : base(httpClientFactory) 
        { 
            
        }

        public async Task<IActionResult> OnGet()
        {
            ViewData["ShowSidebar"] = false;
            if (await IsUserAuthenticated())
            {
                return Redirect("/Profile");
            }
            else return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("email", Email),
                new KeyValuePair<string, string>("password", Password)
            });

                var response = await _httpClient.PostAsync("login", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewData["ShowSidebar"] = true;
                    cookieService.SaveCookie(response, Response);
                    return RedirectToPage("/Profile");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    ViewData["ShowSidebar"] = false;
                    Message = "Неверный email или пароль.";
                }
                else
                {
                    ViewData["ShowSidebar"] = false;
                    Message = "Произошла ошибка при авторизации.";
                }
            }
            catch (Exception ex)
            {
                Message = "Ошибка: " + ex.Message;
            }

            return Page();
        }
    }
}
