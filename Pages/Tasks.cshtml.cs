using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjManagmentSystem.Pages
{
    public class TasksModel : BasePageModel
    {

        private readonly HttpClient _httpClient;
        public readonly UserService _userService;

        public string Token { get; set; }
        [BindProperty]
        public bool IsPermissionToCreateAndEdit { get; set; }
        [BindProperty]
        public int? EditingTaskId { get; set; }
        [BindProperty]
        public List<Users> selectedUsers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string ProjectName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ProjectId { get; set; }
        [BindProperty]
        public List<TaskToGet> tasks { get; set; }
        public List<Users> TaskUsers { get; set; } = new();

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

        public async Task<IActionResult> OnGet(int taskId)
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
                var response = await _httpClient.GetAsync("tasks/users");

                if (response.IsSuccessStatusCode)
                {
                    var tasksList = await response.Content.ReadFromJsonAsync<List<TaskToGet>>();

                    this.tasks = tasksList;

                    response = await _httpClient.GetAsync($"tasks/{ProjectId}/permission");

                    if (response.IsSuccessStatusCode)
                    {
                        var hasPermission = await response.Content.ReadFromJsonAsync<bool>();

                        this.IsPermissionToCreateAndEdit = hasPermission;
                    }
                    else
                    {
                        this.IsPermissionToCreateAndEdit = false;
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении данных задач: {response.StatusCode}");
                }
                if (taskId > 0)
                {
                    var usersResponse = await _httpClient.GetAsync($"user/task/{taskId}");
                    if (usersResponse.IsSuccessStatusCode)
                    {
                        TaskUsers = await usersResponse.Content.ReadFromJsonAsync<List<Users>>();
                    }
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

                if ((ProjectId == null || ProjectId == 0) && !EditingTaskId.HasValue) return Redirect("/Tasks");
                
                var formContent = new MultipartFormDataContent
                {
                    { new StringContent(task.name), "name" },

                    { new StringContent(task.description), "description" },
                    { new StringContent(task.status), "status"}
                };


                HttpResponseMessage response;
                if (EditingTaskId.HasValue)
                {
                    response = await _httpClient.PutAsync($"tasks/update?taskId={EditingTaskId}", formContent);
                }
                else
                {
                    response = await _httpClient.PostAsync($"tasks/add/{ProjectId}", formContent);
                }

                if (response.IsSuccessStatusCode)
                {
                    if (!EditingTaskId.HasValue)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<Tasks>(responseContent);
                        int taskId = result.id;
                        var userEmails = selectedUsersToTask.Select(u => new UserWithResponsibilityDTO
                        {
                            Email = u.email,
                            IsResponsible = u.IsResponsible.Value
                        }).ToList();

                        var addUsersToProjectDTO = new AddUsersToTaskDTO
                        {
                            taskId = taskId,
                            userIds = userEmails
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
                        int projectId = EditingTaskId.Value;


                        var userEmails = selectedUsersToTask.Select(u => new UserWithResponsibilityDTO
                        {
                            Email = u.email,
                            IsResponsible = u.IsResponsible ?? false
                        }).ToList();

                        var addUsersToProjectDTO = new AddUsersToTaskDTO
                        {
                            taskId = projectId,
                            userIds = userEmails
                        };

                        var jsonContent = new StringContent(
                            JsonSerializer.Serialize(addUsersToProjectDTO),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var addUserResponse = await _httpClient.PutAsync("tasks/update-users-tasks", jsonContent);

                        if (addUserResponse.IsSuccessStatusCode)
                        {
                            return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                        }
                        else
                        {
                            Console.WriteLine($"Ошибка при обновлении пользователей: {addUserResponse.StatusCode}");
                            ModelState.AddModelError(string.Empty, "Ошибка при обновлении пользователей");
                            return Page();
                        }
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
        public async Task<IActionResult> OnPostCreateSubTask([FromForm] Subtask subtask)
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
                    { new StringContent(subtask.name), "name" },

                    { new StringContent(subtask.description), "description" },
                    { new StringContent(subtask.task.ToString()), "task"},
                    { new StringContent(subtask.responsible.ToString()), "responsible" }
                };
                var response = await _httpClient.PostAsync("tasks/add-subtask", formContent);
                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = true });
                }
                return Page();
            }
            catch (Exception ex)
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPostLoadTaskUsersAsync([FromQuery] int taskId)
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
                var response = await _httpClient.GetAsync($"user/task/{taskId}");
                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<Users>>();
                    return new JsonResult(new { success = true, users });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Ошибка загрузки пользователей" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Ошибка: " + ex.Message });
            }
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
    }
}
