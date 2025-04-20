using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class ProfileModel : BasePageModel
    {
        public string Token { get; set; }
        private readonly HttpClient _httpClient;
        public Users user = new Users();

        public ProfileModel(IHttpClientFactory httpClientFactory) : base(httpClientFactory) 
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
        }

        public async Task<IActionResult> OnGet()
        {
            var isAuthenticated = await IsUserAuthenticated();

            if (!isAuthenticated)
            {
                return HandleAuthorization(isAuthenticated); 
            }

            Token = Request.Cookies["token"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            try
            {
                var response = await _httpClient.GetAsync("users/current");

                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadFromJsonAsync<Users>();
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении данных пользователей: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при запросе к /users: {ex.Message}");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostButtonClick()
        {
            Response.Cookies.Delete("token");


            return RedirectToPage("/Authorization");
        }
    }
}
