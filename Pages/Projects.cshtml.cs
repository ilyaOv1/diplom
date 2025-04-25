using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace ProjManagmentSystem.Pages
{
    public class ProjectsModel : BasePageModel
    {

        private readonly HttpClient _httpClient;
        private readonly UserService _userService;
        public string Token { get; set; }
        public Users user = new Users();
        [BindProperty]
        public List<Users> selectedUsers { get; set; } = new();
        public static List<Users> selectedUsersToProject = new List<Users>();


        public ProjectsModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _userService = userService;
        }

        public async Task<IActionResult> OnPostProcessArrayAsync([FromBody] Users[] users)
        {
            selectedUsers = users.ToList();
            selectedUsersToProject = users.ToList();
            return new JsonResult(new { success = true, message = "Массив получен" });
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateProject([FromForm] Project project)
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
                project.IsPrivate = Request.Form["IsPrivate"] == "1";
                var formContent = new MultipartFormDataContent
                {
                    { new StringContent(project.Name), "Name" },

                    { new StringContent(project.Description), "Description" },
                    { new StringContent((project.IsPrivate ? "true" : "false")), "IsPrivate" }
                };
                var response = await _httpClient.PostAsync("project/add", formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Project>(responseContent);
                    int projectId = result.id;
                    List<string> emails = new List<string>();
                    foreach (var i in selectedUsersToProject)
                    {
                        emails.Add(i.email);
                    }
                    var addUsersToProjectDTO = new AddUsersToProjectDTO
                    {
                        ProjectId = projectId,
                        UserIds = emails
                    };

                    // Формирование JSON-тела для второго запроса
                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(addUsersToProjectDTO),
                        Encoding.UTF8,
                        "application/json"
                    );
                    var addUserResponse = await _httpClient.PostAsync("project/add-users-project", jsonContent);

                    if (addUserResponse.IsSuccessStatusCode)
                    {
                        // Успешное добавление пользователей
                        return RedirectToPage("/Projects");
                    }
                    else
                    {
                        // Ошибка при добавлении пользователей
                        Console.WriteLine($"Ошибка при добавлении пользователей: {addUserResponse.StatusCode}");
                        return Page();
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
