using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class BasePageModel : PageModel
    {
        protected readonly HttpClient _httpClient;

        public BasePageModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuthClient");
        }

        protected async Task<bool> IsUserAuthenticated()
        {
            if (Request.Cookies.TryGetValue("token", out var token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("validate-token");

                return response.IsSuccessStatusCode;
            }

            return false;
        }

        protected IActionResult HandleAuthorization(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                return RedirectToPage("/Profile"); 
            }
            else
            {
                Response.Cookies.Delete("token"); 
                return RedirectToPage("/Authorization");
            }
        }
    }
}
