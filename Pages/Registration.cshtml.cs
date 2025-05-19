using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Services;

namespace ProjManagmentSystem.Pages
{
    public class RegistrationModel : BasePageModel
    {
        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public string Surname { get; set; }
        [BindProperty]
        public string LastName { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty]
        public string RepeatPassword { get; set; }


        public RegistrationModel(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<IActionResult> OnGet()
        {
            ViewData["ShowSidebar"] = false;
            if (await IsUserAuthenticated())
            {
                return Redirect("/Profile");
            }
            else return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                ViewData["ShowSidebar"] = false;
                if (!ValidateForm()) return Page();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", Email),
                    new KeyValuePair<string, string>("password", Password),
                    new KeyValuePair<string, string>("name", Name),
                    new KeyValuePair<string, string>("surname", Surname),
                    new KeyValuePair<string, string>("patronymic", LastName)
                });

                var response = await _httpClient.PostAsync("registr", content);
                if (response.IsSuccessStatusCode)
                {
                    return Redirect("/Authorization");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Такой пользователь уже существует.";
                    return Page();
                }
                else
                {
                    return StatusCode(400);
                }
            }
            catch (Exception ex)
            {

            }


            return Page();
        }

        private bool ValidateForm()
        {
            if (!RegexService.IsValidLatinName(Name))
            {
                TempData["ErrorMessage"] = "Имя должно включать только латинские буквы.";
                return false;
            }
            else if (!RegexService.IsValidLatinName(Surname))
            {
                TempData["ErrorMessage"] = "Фамилия должна включать только латинские буквы.";
                return false;
            }
            else if (!RegexService.IsValidLatinName(LastName))
            {
                TempData["ErrorMessage"] = "Отчество должно включать только латинские буквы.";
                return false;
            }
            else if (!RegexService.IsValidEmail(Email))
            {
                TempData["ErrorMessage"] = "Введите почту в формате xx@xx.xx";
                return false;
            }
            else if (!RegexService.IsValidPassword(Password))
            {
                TempData["ErrorMessage"] = "Пароль должен содержать в себе не менее 8 символов, а также как минимум 1 цифру и 1 заглавную букву.";
                return false;
            }
            else if (Password != RepeatPassword)
            {
                TempData["ErrorMessage"] = "Пароли не совпадают.";
                return false;
            }
            else if (String.IsNullOrEmpty(Name) || String.IsNullOrEmpty(Surname) || String.IsNullOrEmpty(LastName) || String.IsNullOrEmpty(Email) || String.IsNullOrEmpty(Password) || String.IsNullOrEmpty(RepeatPassword))
            {
                TempData["ErrorMessage"] = "Заполните все поля.";
                return false;
            }
            else return true;
        }
    }
}