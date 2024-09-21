using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RCLWEB.DATA.ViewModels;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using Rotativa.AspNetCore;

namespace RCLWEBCORE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly IClientService _clientServie;
        public TestController(IUnitOfWork unitofWork, IClientService clientService)
        {
            _unitofWork = unitofWork;
            _clientServie = clientService;
        }

        public async Task<byte[]> PortfolioPDFTest()
        {
            string? code = string.Empty;
            code = "FM855";
            //if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            //{
            //    code = HttpContext.Session.GetString(ExtraAct.clientCode);
            //}
            //else
            //{
            //    return RedirectToAction("ViewAllCodes", "Home");
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                //  return NotFound();
            }

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@acode", code);
            var list = _unitofWork.SP_Call.ReturnListFromSis<PortfolioCompanyVM>("inv_pfs_td", dynamicParameters).ToList();

            list.RemoveAll(x => x.Grp != 0);

            string? query = string.Empty;


            ClientPortfolioVM model = new()
            {
                PortfolioCompanyVMs = list,
                ClientDetails = client,
                RglBal = 0
            };


            query = @"SELECT rgl FROM T_RGL WHERE acode='" + code + "' ";
            var rglBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            // model.RglBal = rglBal?.rgl;
            if (rglBal != null)
            {
                model.RglBal = rglBal?.rgl;
            }

            query = "SELECT intamt FROM T_intsum WHERE acode='" + code + "'";
            var accured = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            model.AccruedBal = accured?.intamt;


            query = "SELECT matbalP,ldgbal FROM T_Tkbal WHERE acode = '" + code + "'";
            var mat = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            model.MaturedBal = mat?.matbalP;
            model.LedgerBal = mat?.ldgbal;

            model.SaleRec = Convert.ToDecimal(0.00);

            query = "SELECT BAL FROM T_SRTD WHERE ACODE='" + code + "'";
            var bal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            model.SaleRec = (bal?.BAL);

            if (model.SaleRec.ToString() != "0.00")
            {
                model.LedgerBal = model.LedgerBal;
            }
            else
            {
                model.LedgerBal = model.LedgerBal + bal?.BAL;
            }

            query = @"SELECT IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'AppliedDate' FROM RCLWEB.dbo.T_IPO_REC " +
                "WHERE RCODE='" + code + "' AND IPO_RESULT='' and DATEDIFF(DAY, ENTRY_DATE, GETDATE())<100 UNION ALL SELECT " +
                "IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'AppliedDate' FROM RCLWEB.dbo.T_IPO_REC_MASTER " +
                "WHERE RCODE='" + code + "' AND IPO_RESULT='' and DATEDIFF(DAY,ENTRY_DATE,GETDATE())<100";

            var ipoList = _unitofWork.SP_Call.ListByRawQuery<PendingIPOVM>(query).AsQueryable().ToList();

            query = @"SELECT SUM(CONVERT(decimal(20,2),totcost)) AS 'TotalCost',SUM(CONVERT(decimal(20,2),(a.totqty*c.iclose))) AS 
'MarketValue',SUM(CONVERT(decimal(20,2),((a.totqty*c.iclose)-(a.totcost)))) AS 'GainLoss' FROM T_PF a,T_idxshrp c WHERE a.acode='" + code + "' " +
"AND a.firmscd=c.firmscd AND c.tdate=(SELECT MAX(tdate) FROM T_idxshrp where firmscd= c.firmscd)";
            var totalBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();

            model.TotalBuyCost = totalBal?.TotalCost;
            model.MarketVal = totalBal?.MarketValue;
            model.EquityBal = totalBal?.MarketValue + model.LedgerBal - model.AccruedBal;
            model.UnrealiseBal = totalBal?.GainLoss;
            model.TotalCapital = model.UnrealiseBal + model.RglBal;

            model.PendingShares = ipoList;

            int n = 1;
            foreach (var item in model.PortfolioCompanyVMs)
            {
                // item.Sl = n;
                n++;
                if (item.Pldqty == 0)
                {
                    item.Pldqty = null;
                }
            }
            //var actionPDF = new Rotativa.AspNetCore.ViewAsPdf("YOUR_VIEW_NAME")
            //{

            //    PageSize = Rotativa.Options.Size.A4,
            //    PageOrientation = Rotativa.Options.Orientation.Landscape,
            //   PageMargins = { Left = 1, Right = 1 }
            //};


            string cusomtSwitches = string.Format("--print-media-type --allow {0} --footer-html {0}", Url.Action("ReportFooter", "Report", new { }, "https"));
            var actionPDF = new ViewAsPdf("PortfolioPDF","Report", model)
            {
                //CustomSwitches = "--page-offset 0 --footer-left [page] --footer-font-size 12"
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(3, 3, 9, 3)
            };
            byte[] applicationPDFData = await actionPDF.BuildFile(ControllerContext);
            return applicationPDFData;
        }

        [Route("getReport")]
        public async Task<IActionResult> Result()
        {
            var file = await PortfolioPDFTest();
            return File(file, "application/pdf", "sample.pdf");
            // MemoryStream ms = new MemoryStream(file);
            //  return new FileStreamResult(ms, "application/pdf");



        }
    }
}
