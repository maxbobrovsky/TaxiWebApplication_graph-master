using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaxiWebApplication.Models;
using TaxiWebApplication.ViewModels;

namespace TaxiWebApplication.Controllers
{
    public class AccountController : Controller
    {

        [Authorize(Roles = "admin,user")]
       
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "driver")]
        public IActionResult DriverIndex()
        {
            return View();
        }

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _rolle;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> rolle)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _rolle = rolle;
        }

        [HttpGet]
        public IActionResult RegisterPage()
        {
            // var roles = new List<string> { "user", "driver" };
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "user", Value = "user", Selected = false });
            items.Add(new SelectListItem() { Text = "driver", Value = "driver", Selected = true });

            SelectList sl = new SelectList(items, "Value", "Text");
            ViewData["Roles"] = sl;
            var io = (List<SelectListItem>)sl.Items;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPage(RegisterViewModel model)
            {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email, NativeCity = model.NativeCity };
                // добавляем пользователя
                var userRole = new IdentityRole(model.Role);
                await _rolle.CreateAsync(userRole);
                var result = await _userManager.CreateAsync(user, model.Password);
                var creator = await _userManager.AddToRoleAsync(user, model.Role);
                
                if (result.Succeeded && creator.Succeeded)
                {
                    // установка куки
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginPage(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> LoginPage(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Email);

                    if (await _userManager.IsInRoleAsync(user, "user") || await _userManager.IsInRoleAsync(user, "admin"))
                    {
                        return RedirectToAction("Index", "Account"); 
                    }
                    return RedirectToAction("DriverIndex", "Account");
      
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect login and (or) password");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
