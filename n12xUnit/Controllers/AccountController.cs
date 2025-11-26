using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs;

namespace n12xUnit.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterDTO registerDTO)
        {
            return RedirectToAction(nameof(PersonsController.Index), "Person");
        }
    }
}
