using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http.Headers;
using System.Net.Http;

namespace ProjManagmentSystem.Pages
{
    public class ProjectsModel : BasePageModel
    {

        private readonly HttpClient _httpClient;
        private readonly UserService _userService;
        public string Token { get; set; }
        public Users user = new Users();


        public ProjectsModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _userService = userService;
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

                    return RedirectToPage("/Projects");
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
