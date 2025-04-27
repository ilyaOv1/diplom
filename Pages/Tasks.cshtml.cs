using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjManagmentSystem.Pages
{
    public class TasksModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string ProjectName { get; set; }
        public void OnGet()
        {
        }
    }
}
