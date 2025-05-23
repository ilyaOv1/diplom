using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net;

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

        [BindProperty]
        public string SelectedUsersToTask { get; set; }
        [BindProperty]
        public string SelectedSubtasks { get; set; }
        [BindProperty]
        public int TaskId { get; set; }

        public TasksModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _userService = userService;
        }

        public async Task<IActionResult> OnPostProcessArrayAsync([FromBody] Users[] users)
        {
            selectedUsers = users.ToList();
            var usersList = new List<UserWithResponsibilityDTO>();
            if (!string.IsNullOrEmpty(SelectedUsersToTask))
            {
                usersList = JsonSerializer.Deserialize<List<UserWithResponsibilityDTO>>(SelectedUsersToTask);
            }
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

                var users = new List<UserWithResponsibilityDTO>();
                if (!string.IsNullOrEmpty(SelectedUsersToTask))
                {
                    users = JsonSerializer.Deserialize<List<UserWithResponsibilityDTO>>(SelectedUsersToTask);
                }
                var subtasks = new List<SubtaskDTO>();
                if (!string.IsNullOrEmpty(SelectedSubtasks))
                {
                    subtasks = JsonSerializer.Deserialize<List<SubtaskDTO>>(SelectedSubtasks);
                }
                var formContent = new MultipartFormDataContent
                {
                    { new StringContent(task.name), "name" },

                    { new StringContent(task.description), "description" }
                };
                var expectedDate = task.expected_date == DateTime.Today
                    ? DateTime.Now.AddDays(7)
                    : task.expected_date;

                formContent.Add(new StringContent(expectedDate.ToString("yyyy-MM-dd")), "expected_date");

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

                        HttpResponseMessage response2 = await AddUsersToTask(response, users);

                        if (response2.IsSuccessStatusCode)
                        {
                            response2 = await AddSubtaskToTask(subtasks, response);

                            if (response2.IsSuccessStatusCode)
                            {
                                return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                            }
                            else
                            {
                                Console.WriteLine($"Ошибка при добавлении подзадач: {response2.StatusCode}");
                                return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Ошибка при добавлении пользователей: {response2.StatusCode}");
                            return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                        }
                    }
                    else
                    {
                        HttpResponseMessage response2 = await UpdateUsersToTask(users);

                        if (response2.IsSuccessStatusCode)
                        {
                            response2 = await AddSubtaskToTask(subtasks, response);

                            if (response2.IsSuccessStatusCode)
                            {
                                return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                            }
                            else
                            {
                                Console.WriteLine($"Ошибка при добавлении подзадач: {response2.StatusCode}");
                                return RedirectToPage(new { projectName = ProjectName, projectId = ProjectId });
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Ошибка при обновлении пользователей: {response2.StatusCode}");
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
                    { new StringContent("1"), "id" },
                    { new StringContent(subtask.name), "name" },
                    { new StringContent(subtask.task.ToString()), "task"},
                    { new StringContent(subtask.description), "description" },
                    { new StringContent(subtask.description), "status" },

                    { new StringContent(subtask.responsible.ToString()), "responsible" },
                    { new StringContent(subtask.responsible.ToString()), "responsibleName"  }
                };
                var expectedDate = subtask.expected_date == DateTime.Today
                                    ? DateTime.Now.AddDays(7)
                                    : subtask.expected_date;

                formContent.Add(new StringContent(expectedDate.ToString("yyyy-MM-dd")), "expected_date");
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

        public async Task<IActionResult> OnPostLoadSubTaskAsync([FromQuery] int taskId)
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
                var response = await _httpClient.GetAsync($"tasks/subtasks/{taskId}");
                if (response.IsSuccessStatusCode)
                {
                    var subtasks = await response.Content.ReadFromJsonAsync<List<Subtask>>();
                    return new JsonResult(new { success = true, subtasks });
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

        public async Task<IActionResult> OnPostUpdateTaskStatus([FromBody] UpdateTaskStatusDTO dto)
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
                var response = await _httpClient.PutAsync($"tasks/update-status", jsonContent);

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

        public async Task<IActionResult> OnPostDeleteTask()
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
                var response = await _httpClient.DeleteAsync($"tasks/remove/{TaskId}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Tasks");
                }
                else
                {
                    TempData["ErrorMessage"] = $"Ошибка при удалении задачи: {response.StatusCode}.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при обновлении данных: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnGetChangeArchiveTask(int taskId)
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
                var response = await _httpClient.GetAsync($"tasks/update-archive/{taskId}");
                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = true, taskId = taskId });
                }
                else
                {
                    TempData["ErrorMessage"] = $"Ошибка при перенесе задачи в архив: {response.StatusCode}.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при обновлении данных: {ex.Message}");
                return Page();
            }
        }

        public async Task<HttpResponseMessage> AddUsersToTask(HttpResponseMessage response, List<UserWithResponsibilityDTO> users)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Tasks>(responseContent);
            int taskId = result.id;

            var userEmails = users.Select(u => new UserWithResponsibilityDTO
            {
                Email = u.Email,
                IsResponsible = u.IsResponsible
            }).ToList();

            var addUsersToProjectDTO = new AddUsersToTaskDTO
            {
                TaskId = taskId,
                UserIds = userEmails
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(addUsersToProjectDTO),
                Encoding.UTF8,
                "application/json"
            );

            var addUserResponse = await _httpClient.PostAsync("tasks/add-users-task", jsonContent);

            return addUserResponse;
        }

        public async Task<HttpResponseMessage> UpdateUsersToTask(List<UserWithResponsibilityDTO> users)
        {
            int projectId = EditingTaskId.Value;


            var userEmails = users.Select(u => new UserWithResponsibilityDTO
            {
                Email = u.Email,
                IsResponsible = u.IsResponsible
            }).ToList();

            var addUsersToProjectDTO = new AddUsersToTaskDTO
            {
                TaskId = projectId,
                UserIds = userEmails
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(addUsersToProjectDTO),
                Encoding.UTF8,
                "application/json"
            );

            var addUserResponse = await _httpClient.PutAsync("tasks/update-users-tasks", jsonContent);

            return addUserResponse;
        }

        public async Task<HttpResponseMessage> AddSubtaskToTask(List<SubtaskDTO> subtasks, HttpResponseMessage response)
        {
            int taskId;

            if (!EditingTaskId.HasValue)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Tasks>(responseContent);
                taskId = result.id;
            }
            else
            {
                taskId = EditingTaskId.Value;
            }

            if (subtasks == null || !subtasks.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Нет подзадач для добавления", Encoding.UTF8, "text/plain")
                };
            }

            foreach (var subtask in subtasks)
            {
                var formContent = new MultipartFormDataContent
                {
                    { new StringContent(subtask.Name), "name" },
                    { new StringContent(taskId.ToString()), "task" },
                    { new StringContent(subtask.Description ?? string.Empty), "description" },
                    { new StringContent("Новая"), "status" },
                    { new StringContent(subtask.ResponsibleEmail ?? string.Empty), "responsible" },
                    { new StringContent(subtask.ResponsibleEmail ?? string.Empty), "responsibleName" },
                    { new StringContent(subtask.ExpectedDate.ToString("yyyy-MM-dd")), "expected_date" }
                };

                response = await _httpClient.PostAsync("tasks/add-subtask", formContent);

                if (!response.IsSuccessStatusCode)
                {
                    return response; 
                }
            }

            return response;
        }
    }
}
