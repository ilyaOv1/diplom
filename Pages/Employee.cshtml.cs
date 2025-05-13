using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class EmployeesModel : BasePageModel
    {

        private readonly HttpClient _httpClient;
        private readonly UserService _userService;
        public string Token { get; set; }
        public List<Users> users = new List<Users>();

        public EmployeesModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _userService = userService;
        }

        public async Task<IActionResult> OnGet()
        {
            var isAuthenticated = await IsUserAuthenticated();

            if (!isAuthenticated)
            {
                return HandleAuthorization(isAuthenticated);
            }

            var token = Request.Cookies["token"];
            Token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("users");

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<Users>>();
                    this.users = users;

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
    }
}
