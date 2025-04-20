using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace ProjManagmentSystem.Pages
{
    public class IndexModel : BasePageModel
    {
        public IndexModel(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public async Task<IActionResult> OnGet()
        {
            var isAuthenticated = await IsUserAuthenticated();
            return HandleAuthorization(isAuthenticated);
        }
    }
}
