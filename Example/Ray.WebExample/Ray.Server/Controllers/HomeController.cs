using Microsoft.AspNetCore.Mvc;

namespace Ray.Server.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("Host 已启动");
        }
    }
}
