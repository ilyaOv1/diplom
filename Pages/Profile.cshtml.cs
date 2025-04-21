using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Models;
using ProjManagmentSystem.Services;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class ProfileModel : BasePageModel
    {

		private readonly HttpClient _httpClient;
		private readonly UserService _userService;
		public string Token { get; set; }
		public Users user = new Users();
		

        public ProfileModel(IHttpClientFactory httpClientFactory, UserService userService) : base(httpClientFactory) 
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
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			try
			{
				var response = await _httpClient.GetAsync("users/current");

				if (response.IsSuccessStatusCode)
				{
					var user = await response.Content.ReadFromJsonAsync<Users>();

					_userService.SetUserData($"{user.surname} {user.name} {user.patronymic}", token);

					ViewData["SideBarFIO"] = _userService.FIO;
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

		public async Task<IActionResult> OnPostButtonClick()
        {
            Response.Cookies.Delete("token");


            return RedirectToPage("/Authorization");
        }
    }
}
