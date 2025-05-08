using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class SubtasksModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        public readonly UserService _userService;

        [BindProperty]
        public List<Subtask> subtasks { get; set; }
        [BindProperty(SupportsGet = true)]
        public string TaskName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TaskId { get; set; }

        public SubtasksModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _userService = userService;
        }

        public async Task<IActionResult> OnGet(int taskId)
        {
            if (TaskId == null || TaskId == 0)
            {
                return Redirect("/Profile");
            }

            var isAuthenticated = await IsUserAuthenticated();

            if (!isAuthenticated)
            {
                return HandleAuthorization(isAuthenticated);
            }

            var token = Request.Cookies["token"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync($"tasks/subtasks/{TaskId}");

                if (response.IsSuccessStatusCode)
                {
                    var subtasksList = await response.Content.ReadFromJsonAsync<List<Subtask>>();

                    this.subtasks = subtasksList;
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
