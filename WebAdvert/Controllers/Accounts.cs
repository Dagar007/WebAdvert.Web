using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Models.Accounts;


namespace WebAdvert.Controllers
{
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public Accounts(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }
        public ActionResult Signup()
        {
            var model = new SignupModel(); 
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            if(ModelState.IsValid)
            {
                var user = _pool.GetUser(signupModel.Email);
                if(user.Status != null)
                {
                    ModelState.AddModelError("User Exists", "User with this email already exsits");
                    return View(signupModel);
                }
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, signupModel.Email);
                var createdUser = await _userManager.CreateAsync(user, signupModel.Password);
                if(createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm");
                }
            }
            return View();
        }

        public IActionResult Confirm(ConfirmModel confirmModel)
        {
            return View(confirmModel);
        }
        [HttpPost]
        public async Task<IActionResult> Confirm_Post(ConfirmModel confirmModel)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(confirmModel.Email);
                if(user == null)
                {
                    ModelState.AddModelError("Not Found", "A User with the given user was not found.");
                    return View(confirmModel);
                }
                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, confirmModel.Code, true);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                        return View(confirmModel);
                    }
                }
            }
            return View(confirmModel);
        }

        public IActionResult Login(LoginModel loginModel)
        {
            return View(loginModel);
        }
        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel loginModel)
        {
            if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, false);
                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");
                else
                    ModelState.AddModelError("Login Error", "Email and Password doesn't match");
            }
            return View("Login", loginModel);
        }
         
    }
}
