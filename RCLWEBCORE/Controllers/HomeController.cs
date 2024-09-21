using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCLWEB.DATA.ViewModels;
using RCLWEB.Utility;
using RCLWEBCORE.Filters;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Models;
using Rotativa.AspNetCore;
using System.Diagnostics;
using System.Security.Claims;

namespace RCLWEBCORE.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitofWork = unitOfWork;
        }

        // [TypeFilter(typeof(CustomAuthorize))]
        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult Index()
        {
            if(HttpContext?.Session !=null)
            {
                if(HttpContext.Session.GetString(ExtraAct.clientCode) !=null)
                {
                    var username = HttpContext?.User.FindFirstValue(ClaimTypes.Name);
                    var exist = _unitofWork.T_Client.GetFirstOrDefault(u => u.RCODE == HttpContext.Session.GetString(ExtraAct.clientCode) && u.mobile == username);
                }
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            string code = HttpContext?.Session.GetString(ExtraAct.clientCode);
            //if(code == null)
            //{
            //    return RedirectToAction("Login", "Account");
            //}


            //if(exist==null)
            //{
            //    return NotFound();
            //}
            //var claimsIdentity = (ClaimsIdentity?)this.User.Identity;
            //var claims = claimsIdentity?.FindFirst(ClaimTypes.Name);
            //var list = _unitofWork.UserLogin.GetFirstOrDefault(u=>u.Mobile== "01714645756");

            ClientDashboardVM model = new()
            {
                ClientCode =code  //exist.RCODE
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        //[TypeFilter(typeof(CustomAuthorize))]
        public IActionResult ViewAllCodes()
        {
            var claimsIdentity = (ClaimsIdentity?)this.User.Identity;
            var claims = claimsIdentity?.FindFirst(ClaimTypes.Name);

            var username = HttpContext?.User.FindFirstValue(ClaimTypes.Name);
           // var username = claims?.ToString();
            string query = string.Empty;

            query = @"SELECT ROW_NUMBER() OVER(ORDER BY a.RCODE) AS sl,  a.RCODE, a.fhName,b.matbalP AS matbalP,b.ldgbal 
                AS ldgbal FROM [RCLWEB].[dbo].[T_CLIENT1] a,[SISROYALU].[dbo].[T_Tkbal] b WHERE a.mobile='"+ username +"'"+ 
                "AND a.boStatus='Active' AND a.RCODE=b.acode UNION ALL SELECT ROW_NUMBER() OVER(ORDER BY a.RCODE)" + 
                "AS sl,  a.RCODE, a.fhName,0 AS matbalP,0 AS ldgbal FROM [RCLWEB].[dbo].[T_CLIENT1] a WHERE a.mobile='"+ username +"'"+
                "AND a.RCODE NOT IN (SELECT a.RCODE FROM [RCLWEB].[dbo].[T_CLIENT1] a,[SISROYALU].[dbo].[T_Tkbal] b WHERE a.mobile='" + username + "' " +
                "AND a.RCODE=b.acode)";

            var codeDetails = _unitofWork.SP_Call.ListByRawQuery<CodeListVM>(query).AsQueryable();

            ClientCodeListVM model = new ClientCodeListVM
            {
                CodeLists=codeDetails.ToList(),
            };
            return View(model);
        }
        [Authorize]
        public IActionResult SetCode(string code)
        {
            if(code ==null)
            {
                return RedirectToAction("ViewAllCodes");
            }
            var username = HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            var exist = _unitofWork.T_Client.GetFirstOrDefault(u => u.RCODE == code && u.mobile == username);

            if (exist == null)
            {
                return RedirectToAction("ViewAllCodes");
            }
            HttpContext?.Session.SetString("_clientCode", code);
            return RedirectToAction("Index");
        }
        public IActionResult BoAck()
        {
            return new ViewAsPdf("BoAck")
            {
                //PageSize=a
            };
        }

        public IActionResult Tables()
        {
            return View();
        }

        public IActionResult Reports() 
        {
            return View();
        }
    }
}