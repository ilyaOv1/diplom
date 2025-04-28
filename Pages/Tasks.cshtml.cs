using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class TasksModel : BasePageModel
    {

        private readonly HttpClient _httpClient;
        public readonly UserService _userService;

        public string Token { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ProjectName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ProjectId { get; set; }
        [BindProperty]
        public List<Tasks> tasks { get; set; }
        public TasksModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
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
                var response = await _httpClient.GetAsync("tasks");

                if (response.IsSuccessStatusCode)
                {
                    var tasksList = await response.Content.ReadFromJsonAsync<List<Tasks>>();

                    this.tasks = tasksList;
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении данных задач: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при запросе к /tasks: {ex.Message}");
            }

            return Page();
        }
    }
}
