using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjManagmentSystem.Services;

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
                        }
                        break;

                    case 3:
                        if (NewPassword != ConfirmPassword)
                        {
                            TempData["ErrorMessage"] = "Пароли не совпадают.";
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
