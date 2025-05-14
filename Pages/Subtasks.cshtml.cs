using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjManagmentSystem.Pages
{
    public class SubtasksModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        public readonly UserService _userService;

        [BindProperty]
        public List<Subtask> subtasks { get; set; }
        [BindProperty]
        public bool IsPermissionToCreateAndEdit { get; set; }
        [BindProperty]
        public int? EditingSubtaskId { get; set; }
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

                    response = await _httpClient.GetAsync($"tasks/subtasks/{TaskId}/permission");

                    if (response.IsSuccessStatusCode)
                    {
                        var hasPermission = await response.Content.ReadFromJsonAsync<bool>();

                        this.IsPermissionToCreateAndEdit = hasPermission;
                    }
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

        public async Task<IActionResult> OnPostGetUsersAsync([FromBody] TaskIdDto dto)
        {
            try
            {

                var token = Request.Cookies["token"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                int taskId = dto.TaskId;
                var response = await _httpClient.GetAsync($"user/task/{taskId}");

                if (response.IsSuccessStatusCode)
                {
                    var task = await response.Content.ReadFromJsonAsync<List<Users>>();
                    return new JsonResult(new { success = true, data = task });
                }

                return new JsonResult(new { success = false, message = "Ошибка загрузки задачи." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCreateSubTask([FromForm] Subtask subtask)
        {
            var isAuthenticated = await IsUserAuthenticated();

            if (!isAuthenticated)
            {
                return HandleAuthorization(isAuthenticated);
            }

            var token = Request.Cookies["token"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var formContent = new MultipartFormDataContent
                {
                    { new StringContent("1"), "id" },
                    { new StringContent(subtask.name), "name" },
                    { new StringContent(subtask.task.ToString()), "task"},
                    { new StringContent(subtask.description), "description" },
                    { new StringContent(subtask.description), "status" },

                    { new StringContent(subtask.responsible.ToString()), "responsible" }
                };

                var expectedDate = subtask.expected_date == DateTime.Today
                                ? DateTime.Now.AddDays(7)
                                : subtask.expected_date;

                formContent.Add(new StringContent(expectedDate.ToString("yyyy-MM-dd")), "expected_date");
                HttpResponseMessage response;
                if (!EditingSubtaskId.HasValue)
                {
                    response = await _httpClient.PostAsync("tasks/add-subtask", formContent);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToPage(new { taskName = TaskName, taskId = TaskId });
                    }
                }
                else
                {
                    response = await _httpClient.PostAsync($"tasks/update-subtask/{EditingSubtaskId.Value}", formContent);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToPage(new { taskName = TaskName, taskId = TaskId });
                    }
                }
                return Page();
            }
            catch (Exception ex)
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdateSubtaskStatus([FromBody] UpdateTaskStatusDTO dto)
        {
            try
            {
                var token = Request.Cookies["token"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await _httpClient.PutAsync($"tasks/subtask/update-status", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    return new JsonResult(new { success = false, message = $"Ошибка сервера: {errorText}" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
