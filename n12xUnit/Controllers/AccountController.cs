using Entities.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTOs;

namespace n12xUnit.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if(ModelState.IsValid == false)
            {
                ViewBag.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return View(registerDTO);
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                Name = registerDTO.Name
            };

            IdentityResult result = await _userManager.CreateAsync(newUser, registerDTO.Password!);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
                await _signInManager.SignInAsync(newUser, isPersistent: false); // false: each time browser is closed, sign out 
            }
            else
            {
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return View(registerDTO);
            }

        }
    }
}
