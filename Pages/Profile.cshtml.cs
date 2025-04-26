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
            Token = token;
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			try
			{
				var response = await _httpClient.GetAsync("users/current");

				if (response.IsSuccessStatusCode)
				{
					var user = await response.Content.ReadFromJsonAsync<Users>();
					this.user = user;
					_userService.SetUserData(user.email, $"{user.surname} {user.name} {user.patronymic}", token);

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

        public async Task<IActionResult> OnPostUpdateProfile([FromForm] Users updatedUser)
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
                    { new StringContent(updatedUser.email), "email" },
                  
                    { new StringContent(updatedUser.name), "name" },
                    { new StringContent(updatedUser.surname), "surname" },
                    { new StringContent(updatedUser.patronymic), "patronymic" },
                    { new StringContent(updatedUser.description), "description" }
                };

                var response = await _httpClient.PutAsync("update-profile", formContent);

                if (response.IsSuccessStatusCode)
                {
                    user.surname = updatedUser.surname;
                    user.name = updatedUser.name;
                    user.patronymic = updatedUser.patronymic;
                    user.description = updatedUser.description;

                    _userService.SetUserData(updatedUser.email, $"{updatedUser.surname} {updatedUser.name} {updatedUser.patronymic}", Token);

                    ViewData["SideBarFIO"] = _userService.FIO;

                    return RedirectToPage("/Profile");
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

        public async Task<IActionResult> OnPostDeleteAccount()
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
                var response = await _httpClient.DeleteAsync("remove");
                if (response.IsSuccessStatusCode)
                {
                    Response.Cookies.Delete("token");
                    return RedirectToPage("/Authorization");
                }
                else
                {
                    Console.WriteLine($"Ошибка при удалении профиля: {response.StatusCode}");
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
