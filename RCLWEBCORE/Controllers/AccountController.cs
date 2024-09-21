using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCLWEB.DATA.ViewModels;
using RCLWEBCORE.Filters;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using System.Security.Claims;

namespace RCLWEBCORE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitofWork;
        public AccountController(IUnitOfWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string? returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            HttpContext.Session.Remove("_username");
            LoginVM model = new()
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            string returnUrl = model.ReturnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                if(model.UserType == UserType.User)
                {
                    var user=_unitofWork.UserLogin.GetFirstOrDefault(u=>u.Mobile==model.UserName.Trim());
                    if(user!=null)
                    {
                        if(user.Mobile==model.UserName.Trim() && user.Password==model.Password.Trim())
                        {
                            if(user.Lock==0)
                            {
                                ModelState.AddModelError("", "Your Account Is Locked. Please Contact with Authority.");
                                return View(model);
                            }
                            else if(user.Lock==1)
                            {
                               // HttpContext.Session.SetString("_username", model.UserName);
                                //var identity = new ClaimsIdentity("Username");
                                //                                var identity = new ClaimsIdentity(new List<Claim>
                                //{
                                //    new Claim("Username", model.UserName, ClaimValueTypes.String)
                                //}, "Username");
                                //                                HttpContext.User = new ClaimsPrincipal(identity);

                                var claims = new List<Claim>() {
                    //new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Mobile)),
                        new Claim(ClaimTypes.Name, user.Mobile),
                        new Claim(ClaimTypes.Role, "Client"),
                        //new Claim("Locked", user.Lock.GetValueOrDefault().ToString()),
                        new Claim("Role", "Client")
                };
                                //Initialize a new instance of the ClaimsIdentity with the claims and authentication scheme    
                                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                //Initialize a new instance of the ClaimsPrincipal with ClaimsIdentity    
                                var principal = new ClaimsPrincipal(identity);
                                //SignInAsync is a Extension method for Sign in a principal for the specified scheme.    
                               await  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                                {
                                    //IsPersistent = objLoginModel.RememberLogin
                                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60),
                                });
                                //return LocalRedirect(returnUrl);
                                return RedirectToAction("ViewAllCodes", "Home");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid Login Attempt!!");
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid Login Attempt!!");
                        return View(model);
                    }
                }
                else if(model.UserType == UserType.Branch)
                {

                }
            }
            return View(model);
        }
      //  [TypeFilter(typeof(CustomAuthorize))]
       // [Authorize]
        public IActionResult Check()
        {
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            //if(HttpContext.Session != null)
            //{
            //    HttpContext.Session.Remove("_username");
            //    return RedirectToAction("Login", "Account");
            //}
            if(User !=null && User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                HttpContext.Session?.Remove("_clientCode");


                return RedirectToAction("Login", "Account");
            }
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
