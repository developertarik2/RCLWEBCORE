using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCLWEB.DATA.Models;
using RCLWEB.DATA.ViewModels;
using RCLWEB.Utility;
using RCLWEBCORE.Filters;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using Rotativa.AspNetCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RCLWEBCORE.Controllers
{
    [Authorize]
    public class CDBLController : Controller
    {
        private readonly IUnitOfWork _unitofWork;

        private IClientService _clientServie;

        public CDBLController(IUnitOfWork unitofWork, IClientService clientServie)
        {
            _unitofWork = unitofWork;
            _clientServie = clientServie;
        }

        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult Info()
        {
            string? code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }

            var boClient=_unitofWork.T_Client.GetFirstOrDefault(u=>u.RCODE== code);
            return View(boClient);
        }
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult BoInfo()
        {
            string? code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            var boClient = _unitofWork.T_Client.GetFirstOrDefault(u => u.RCODE == code);
            string cusomtSwitches = string.Format("--print-media-type --allow {0} --footer-html {0}", Url.Action("ReportFooter", "CDBL", new { rcode=code }, "https"));
            return new ViewAsPdf("BoInfo", boClient)
            {
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(0, 3, 6, 3)
                //PageSize=a
            };
            //return View(boClient);
        }

        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult CDBLChargeReport()
        {
            string? code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
           string query = @"select RCODE,[YEAR],FISCAL,AMOUNT,(convert(varchar(10),[DATE],101)) DATE,NOTE from T_CDBL_CHARGE WHERE rcode='"+ code+"' ORDER BY [YEAR] DESC";
            var charges = _unitofWork.SP_Call.ListByRawQuery<T_CDBL_CHARGE>(query).AsQueryable().ToList();

            CDBLChargeReportVM reportVM = new()
            {
                RCODE=code,
                CHARGEs=charges
            };
            return View(reportVM);
        }
        [AllowAnonymous]
        public IActionResult ReportFooter(string rcode)
        {
            //var product = _unitofWork.Product.GetFirstOrDefault(u => u.Id == productId);
            return PartialView("_ReportFooter",rcode);
        }
    }
}
