using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjManagmentSystem.Pages
{
    public class AuthorizationModel : PageModel
    {

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }
        public string Message { get; set; }

        private readonly HttpClient _httpClient;

        public AuthorizationModel(HttpClientHandler httpClientHandler)
        {
            httpClientHandler.CookieContainer = new System.Net.CookieContainer();
            httpClientHandler.UseCookies = true;

            _httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("https://localhost:7136/api/auth/")
            };
        }
        public void OnGet()
        {
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

                // Отправляем POST-запрос
                var response = await _httpClient.PostAsync("login", content);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    Message = "Авторизация успешна! Токен: " + token;
                    return Redirect("/Profile");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Message = "Неверный email или пароль.";
                }
                else
                {
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
