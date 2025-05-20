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
					_userService.SetUserData(user.email, $"{user.surname} {user.name} {user.patronymic}", token, user.image);

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

        public async Task<IActionResult> OnPostUpdateProfile([FromForm] Users updatedUser, [FromForm] IFormFile imageFile)
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
                
                if (imageFile != null && imageFile.Length > 0)
                {
                    byte[] fileBytes;
                    using (var ms = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }
                    var imageContent = new MultipartFormDataContent();
                    imageContent.Add(new ByteArrayContent(fileBytes), "imageFile", imageFile.FileName);
                    var imageResponse = await _httpClient.PostAsync("upload-image", imageContent);

                    if (!imageResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await imageResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Ошибка при загрузке изображения: {imageResponse.StatusCode}, {errorContent}");
                        return Page();
                    }
                    updatedUser.image = fileBytes;
                }

                var response = await _httpClient.PutAsync("update-profile", formContent);

                if (response.IsSuccessStatusCode)
                {
                    user.surname = updatedUser.surname;
                    user.name = updatedUser.name;
                    user.patronymic = updatedUser.patronymic;
                    user.description = updatedUser.description;

                    return RedirectToPage("/Profile");
                }
                else
                {
                    Console.WriteLine($"Ошибка при обновлении данных: {response.StatusCode}");
                    TempData["ErrorMessage"] = $"Ошибка при обновлении данных: {response.StatusCode}";
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
