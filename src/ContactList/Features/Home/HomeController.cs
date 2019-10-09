namespace ContactList.Features.Home
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Contact");

        public IActionResult Error() => View();
    }
}