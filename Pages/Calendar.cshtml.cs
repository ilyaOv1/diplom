using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class CalendarModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        public readonly UserService _userService;

        [BindProperty]
        public List<TaskToGet> tasks { get; set; }
        public CalendarModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _userService = userService;
        }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnGetEvents()
        {
            try
            {
                var token = Request.Cookies["token"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("tasks/users");

                if (response.IsSuccessStatusCode)
                {
                    var tasksList = await response.Content.ReadFromJsonAsync<List<TaskToGet>>();

                    if (tasksList != null && tasksList.Any())
                    {
                        var filteredTasks = tasksList
                            .Where(t => t.status != "Готово")
                            .ToList();
                        var calendarEvents = filteredTasks.Select(t => new
                        {
                            title = t.name,
                            start = t.expected_date.ToString("yyyy-MM-dd"),
                            extendedProps = new
                            {
                                description = t.description,
                                status = t.status
                            }
                        });

                        return new JsonResult(calendarEvents);
                    }
                }

                // Если задач нет — вернуть пустой массив
                return new JsonResult(new List<object>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении событий: {ex.Message}");
                return new JsonResult(new List<object>());
            }
        }
    }
}
