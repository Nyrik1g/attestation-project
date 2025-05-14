using Microsoft.AspNetCore.Mvc;

namespace AttestationProject.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
