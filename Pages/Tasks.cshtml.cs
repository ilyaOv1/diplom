using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http.Headers;
using System.Text;

namespace ProjManagmentSystem.Pages
{
    public class TasksModel : BasePageModel
    {

        private readonly HttpClient _httpClient;
        public readonly UserService _userService;

        public string Token { get; set; }
        [BindProperty]
        public List<Users> selectedUsers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string ProjectName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ProjectId { get; set; }
        [BindProperty]
        public List<Tasks> tasks { get; set; }
        public static List<Users> selectedUsersToTask = new List<Users>();
        public TasksModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _userService = userService;
        }

        public async Task<IActionResult> OnPostProcessArrayAsync([FromBody] Users[] users)
        {
            selectedUsers = users.ToList();
            selectedUsersToTask = users.ToList();
            return new JsonResult(new { success = true, message = "Массив получен" });
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

        public async Task<IActionResult> OnPostCreateTask([FromForm] Tasks task)
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
                
                var formContent = new MultipartFormDataContent
                {
                    { new StringContent(task.name), "name" },
                    { new StringContent(ProjectId.ToString()), "project"},

                    { new StringContent(task.description), "description" }
                };
                var response = await _httpClient.PostAsync("tasks/add", formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Tasks>(responseContent);
                    int taskId = result.id;
                    List<string> emails = new List<string>();
                    foreach (var i in selectedUsersToTask)
                    {
                        emails.Add(i.email);
                    }
                    var addUsersToProjectDTO = new AddUsersToTaskDTO
                    {
                        TaskId = taskId,
                        UserIds = emails
                    };

                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(addUsersToProjectDTO),
                        Encoding.UTF8,
                        "application/json"
                    );
                    var addUserResponse = await _httpClient.PostAsync("tasks/add-users-task", jsonContent);

                    if (addUserResponse.IsSuccessStatusCode)
                    {
                        return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка при добавлении пользователей: {addUserResponse.StatusCode}");
                        return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                    }
                    
                }
                else
                {
                    Console.WriteLine($"Ошибка при обновлении данных: {response.StatusCode}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при обновлении данных: {ex.Message}");
                return Page();
            }


        }
    }
}
