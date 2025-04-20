using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", Email),
                    new KeyValuePair<string, string>("password", Password),
                    new KeyValuePair<string, string>("name", Name),
                    new KeyValuePair<string, string>("surname", Surname),
                    new KeyValuePair<string, string>("patronymic", LastName)
                });
                if (Password != RepeatPassword)
                {
                    ModelState.AddModelError("RepeatPassword", "Пароли не совпадают.");
                    return Page();
                }
                var response = await _httpClient.PostAsync("registr", content);
                if (response.IsSuccessStatusCode)
                {
                    return Redirect("/Authorization");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return StatusCode(500);
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
    }
}
