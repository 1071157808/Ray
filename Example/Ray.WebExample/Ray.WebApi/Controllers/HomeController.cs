using Microsoft.AspNetCore.Mvc;

namespace Ray.WebApi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("http://localhost:59926/swagger/index.html");
        }
    }
}
