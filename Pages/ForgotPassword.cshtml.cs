using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Services;
using System.Reflection;

namespace ProjManagmentSystem.Pages
{
    public class ForgotPasswordModel : BasePageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Code { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }
        public ForgotPasswordModel(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
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

                int currentStep = 1;
                if (Code != null)
                    currentStep = 2;
                if (NewPassword != null || ConfirmPassword != null)
                    currentStep = 3;

                switch (currentStep)
                {
                    case 1:
                        if (!RegexService.IsValidEmail(Email))
                        {
                            TempData["ErrorMessage"] = "Введите почту в формате xx@xx.xx.";
                            Console.WriteLine(Email);
                            ViewData["CurrentStep"] = 1;

                            Email = null;

                            return Page();
                        }
                        var emailContent = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("email", Email)
                        });

                        var emailResponse = await _httpClient.PostAsync("reset_password", emailContent);

                        if (emailResponse.IsSuccessStatusCode)
                        {
                            ViewData["CurrentStep"] = 2;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Такого пользователя не существует.";
                        }
                        break;

                    case 2:
                        var codeContent = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("email", Email),
                            new KeyValuePair<string, string>("code", Code)
                        });

                        var codeResponse = await _httpClient.PostAsync("verify_code", codeContent);

                        if (codeResponse.IsSuccessStatusCode)
                        {
                            ViewData["CurrentStep"] = 3;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Код введен неверно.";
                            ViewData["CurrentStep"] = 2;
                        }
                        break;

                    case 3:
                        if ( NewPassword != ConfirmPassword)
                        {
                            TempData["ErrorMessage"] = "Пароли не совпадают.";
                            ViewData["CurrentStep"] = 3;
                            return Page();
                        }
                        else if (!RegexService.IsValidPassword(NewPassword))
                        {
                            TempData["ErrorMessage"] = "Пароль должен содержать в себе не менее 8 символов, а также как минимум 1 цифру и 1 заглавную букву.";
                            ViewData["CurrentStep"] = 3;
                            return Page();
                        }

                        var passwordContent = new FormUrlEncodedContent(new[]
                        {
                        new KeyValuePair<string, string>("email", Email),
                        new KeyValuePair<string, string>("password", NewPassword)
                    });

                        var passwordResponse = await _httpClient.PostAsync("update_password", passwordContent);

                        if (passwordResponse.IsSuccessStatusCode)
                        {
                            return RedirectToPage("/Authorization");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Не удалось изменить пароль.";
                            ViewData["CurrentStep"] = 3;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка.";
            }

            return Page();
        }
    }
}
