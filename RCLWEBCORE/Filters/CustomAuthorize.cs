using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RCLWEB.Utility;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using System.Security.Claims;

namespace RCLWEBCORE.Filters
{
    public class CustomAuthorize : Attribute, IAuthorizationFilter
    {
        //private static UserManager<ApplicationUser> _userManager;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IUnitOfWork _unitofWork;

        public CustomAuthorize(IHttpContextAccessor httpContextAccessor, /*UserManager<ApplicationUser> userManager,*/ IUnitOfWork unitofWork)
        {
            _httpContextAccessor = httpContextAccessor;
            //_userManager = userManager;
            _unitofWork = unitofWork;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // string currentUserRole = Convert.ToString(context.HttpContext.Session.GetString("UserRole"));
            //context.HttpContext.

            //var userId = _userManager.GetUserId(User);
            //var user = _userManager.FindByIdAsync(userId).Result;

            var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? clientCode = Convert.ToString(context.HttpContext.Session.GetString(ExtraAct.clientCode));


            var username = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            //var user = _userManager.FindByIdAsync(userId).Result;

            //string currentUserRole = _userManager.GetRolesAsync(user).Result.First();

            //string productId = _httpContextAccessor.HttpContext.Request.Query["pid"].ToString();
            // string aa = Context.Request.Query["pageSize"];

            //  string aa = actionContext.Request.Query["id"].ToString();

            //var username = _httpContextAccessor?.HttpContext?.Session.GetString("_username");


            if (!string.IsNullOrEmpty(username))
            {
                var verify = _unitofWork.UserLogin.GetFirstOrDefault(u => u.Mobile == username);
                if (verify == null)
                {
                    context.Result = new RedirectToRouteResult
                       (
                           new RouteValueDictionary(new
                           {
                               //area = "ClientsUser",
                               action = "Login",
                               controller = "Account"
                           }));
                }

                if(!string.IsNullOrEmpty(clientCode))
                {
                    var codeVerify= _unitofWork.T_Client.GetFirstOrDefault(u => u.RCODE == clientCode && u.mobile == username);
                    if(codeVerify ==null)
                    {
                        context.Result = new RedirectToRouteResult
                      (
                          new RouteValueDictionary(new
                          {
                              //area = "ClientsUser",
                              action = "ViewAllCodes",
                              controller = "Home"
                          }));
                    }
                }
                else
                {
                    context.Result = new RedirectToRouteResult
                       (
                           new RouteValueDictionary(new
                           {
                               //area = "ClientsUser",
                               action = "ViewAllCodes",
                               controller = "Home"
                           }));
                }
            }
            else
            {
                context.Result = new RedirectToRouteResult
                       (
                           new RouteValueDictionary(new
                           {
                               //area = "ClientsUser",
                               action = "Login",
                               controller = "Account"
                           }));
            }
        
           
        }
    }
}
