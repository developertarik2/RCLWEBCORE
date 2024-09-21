using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using System.Security.Claims;

namespace RCLWEBCORE.Filters
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomCookieAuthenticationEvents(IUnitOfWork unitofWork, IHttpContextAccessor httpContextAccessor)
        {
            // Get the database from registered DI services.
            _unitofWork = unitofWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            // Look for the LastChanged claim.
            //var lastChanged = (from c in userPrincipal?.Claims
            //                   where c.Type == "Locked"
            //                   select c.Value).FirstOrDefault();

            var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

            var role2 = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Role);


            //var username = (from c in userPrincipal?.Claims
            //                  where c.Type == "Name"
            //                  select c.Value).FirstOrDefault();

            var username= userPrincipal?.Identities?.FirstOrDefault()?.Name;

            var role = (from c in userPrincipal?.Claims
                            where c.Type == "Role"
                            select c.Value).FirstOrDefault();

            //var p = userPrincipal?.Claims.Where(u=>u).Select(claim => new { claim.Subject, claim.Value }).ToList();

            //var p = userPrincipal?.Identities?.FirstOrDefault()?.Name;
           // var r = userPrincipal?.Identities?.FirstOrDefault()?.Claims?.FirstOrDefault()?.Value;

            //var claimsIdentity = (ClaimsIdentity?)this.Name.Identity;
            //var claims = claimsIdentity?.FindFirst(ClaimTypes.Name);
            if (role != null && role == "Client")
            {

                var user = _unitofWork.UserLogin.GetFirstOrDefault(u => u.Mobile == username);

                if (user == null ||
                   user.Lock == 0)
                {
                    context.RejectPrincipal();

                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }

            else
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }

            //if (string.IsNullOrEmpty(lastChanged) ||
            //    _unitofWork.UserLogin.GetFirstOrDefault().Lock==0)
            //{
            //    context.RejectPrincipal();

            //    await context.HttpContext.SignOutAsync(
            //        CookieAuthenticationDefaults.AuthenticationScheme);
            //}
        }
    }
}
