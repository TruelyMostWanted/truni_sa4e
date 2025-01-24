using Microsoft.AspNetCore.Mvc;

namespace XmasWishes.Controllers
{
    public class GuiController : Controller
    {
        [Route("gui/make-a-wish")]
        public IActionResult MakeAWish()
        {
            return View();
        }
    }
}