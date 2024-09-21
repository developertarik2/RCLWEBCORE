using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using RCLWEB.DATA.ViewModels;
using RCLWEB.Utility;
using RCLWEBCORE.Filters;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using Rotativa.AspNetCore;
using System;
using System.Drawing.Printing;
using System.Net;
using System.Net.Http.Headers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace RCLWEBCORE.Controllers
{
    public class ReportController : Controller
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly IClientService _clientServie;
        public ReportController(IUnitOfWork unitofWork,IClientService clientService)
        {
            _unitofWork = unitofWork;
            _clientServie= clientService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult Portfolio()
        {
            //if(code == null)
            //{
            //    return NotFound();
            //}
            string? code = string.Empty;
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
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@acode", code);
            var list = _unitofWork.SP_Call.ReturnListFromSis<PortfolioCompanyVM>("inv_pfs_td", dynamicParameters).ToList();

             list.RemoveAll(x => x.Grp != 0);

            string? query = string.Empty;


            ClientPortfolioVM model = new()
            {
                PortfolioCompanyVMs= list,
                ClientDetails= client,
                RglBal=0
            };
            query = @"SELECT rgl FROM T_RGL WHERE acode='" + code + "' " ;
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

            model.SaleRec =Convert.ToDecimal(0.00);

            query = "SELECT BAL FROM T_SRTD WHERE ACODE='" + code + "'";
            var bal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            model.SaleRec = (bal?.BAL);

            if(model.SaleRec.ToString() !="0.00")
            {
                model.LedgerBal = model.LedgerBal;
            }
            else
            {
                model.LedgerBal = model.LedgerBal + bal?.BAL;
            }

            query = @"SELECT IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'AppliedDate' FROM RCLWEB.dbo.T_IPO_REC " +
                "WHERE RCODE='"+code+"' AND IPO_RESULT='' and DATEDIFF(DAY, ENTRY_DATE, GETDATE())<100 UNION ALL SELECT " +
                "IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'AppliedDate' FROM RCLWEB.dbo.T_IPO_REC_MASTER " +
                "WHERE RCODE='"+code+"' AND IPO_RESULT='' and DATEDIFF(DAY,ENTRY_DATE,GETDATE())<100";

            var ipoList= _unitofWork.SP_Call.ListByRawQuery<PendingIPOVM>(query).AsQueryable().ToList();

             query = @"SELECT SUM(CONVERT(decimal(20,2),totcost)) AS 'TotalCost',SUM(CONVERT(decimal(20,2),(a.totqty*c.iclose))) AS 
'MarketValue',SUM(CONVERT(decimal(20,2),((a.totqty*c.iclose)-(a.totcost)))) AS 'GainLoss' FROM T_PF a,T_idxshrp c WHERE a.acode='" + code + "' " +
"AND a.firmscd=c.firmscd AND c.tdate=(SELECT MAX(tdate) FROM T_idxshrp where firmscd= c.firmscd)";
            var totalBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();

            model.TotalBuyCost = totalBal?.TotalCost;
            model.MarketVal = totalBal?.MarketValue;
            model.EquityBal = totalBal?.MarketValue + model.LedgerBal - model.AccruedBal;
            model.UnrealiseBal=totalBal?.GainLoss;
            model.TotalCapital = model.UnrealiseBal + model.RglBal;

            model.PendingShares = ipoList;

            //ClientPortfolioVM model = new()
            //{
            //    PortfolioCompanyVMs=list,
            //    RglBal=rglBal?.rgl,
            //    AccruedBal=accured?.intamt,
            //    MaturedBal=mat?.matbalP,
            //    //LedgerBal=mat?.ldgbal,
            //    SaleRec=bal?.BAL,
            //    TotalBuyCost=totalBal?.TotalCost,
            //    MarketVal= totalBal?.MarketValue,
            //    EquityBal= (totalBal?.MarketValue + mat?.ldgbal) - accured?.intamt,
            //    UnrealiseBal=totalBal?.GainLoss,
            //    TotalCapital= totalBal?.GainLoss + mat?.ldgbal,
            //    ChargeFee=0.00
            //};
            //if(Convert.ToDouble(bal?.BAL) != 0.00)
            //{
            //    model.LedgerBal = mat?.ldgbal;
            //}
            //else
            //{
            //    model.LedgerBal = mat?.ldgbal + bal?.BAL;
            //}
            return View(model);
        }
        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult LivePortfolio()
        {
            double mktValLive = 0;
            string? code=string.Empty;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }

            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='"+ code+"'";

            //var client= _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@acode", code);
            var list = _unitofWork.SP_Call.ReturnListFromSis<PortfolioCompanyVM>("inv_pfs_td", dynamicParameters).ToList();
            List<LivePortfolioCompanyVM> list2 = new();
            foreach(var item in list )
            {
                if (Convert.ToDouble(item.Grp) == 0)
                {
                    LivePortfolioCompanyVM model = new()
                    {
                        Firmsnm1 = item.Firmsnm1,
                        Quantity = item.Quantity,
                        Slbqty = item.Slbqty,
                        Pldqty = item.Pldqty,
                        Rate = item.Rate,
                        Amount = item.Amount,
                    };

                    string query = @"SELECT MKISTAT_INSTRUMENT_CODE,MKISTAT_PUB_LAST_TRADED_PRICE FROM T_DSE_MKISTAT WHERE MKISTAT_INSTRUMENT_CODE='" + item.Firmsnm1 + "'";
                    var marketValues = _unitofWork.SP_Call.ListByRawQuery<dynamic>(query).AsQueryable().FirstOrDefault();

                    model.MarketRate = Convert.ToDouble(marketValues?.MKISTAT_PUB_LAST_TRADED_PRICE);
                    model.MarketValue = item.Quantity * Convert.ToDouble(marketValues?.MKISTAT_PUB_LAST_TRADED_PRICE);
                    model.GainLoss = model.MarketValue - model.Amount;

                    mktValLive += model.MarketValue.GetValueOrDefault();

                    list2.Add(model);
                }
            }

            LivePortfolioVM livePortfolioVM = new()
            {
                LivePortfolioLists= list2,
                ClientDetails=client
            };
            string query2 = "SELECT rgl FROM T_RGL WHERE acode='" + code + "'";
            var rglBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();
            livePortfolioVM.GainLossBalance = rglBal?.rgl;

             query2 = "SELECT intamt FROM T_intsum WHERE acode='" + code + "'";
            var accured = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();
            livePortfolioVM.AccruedBal = accured?.intamt;

            query2 = "SELECT matbalP,ldgbal FROM T_Tkbal WHERE acode='" + code + "'";
            var maturedBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();
            livePortfolioVM.MaturedBal = maturedBal?.matbalP;
            livePortfolioVM.LedgerBal=maturedBal?.ldgbal;

            livePortfolioVM.SaleRec ="0.00";
            query2 = "SELECT BAL FROM T_SRTD WHERE ACODE='" + code + "'";
            var sale = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();
            livePortfolioVM.SaleRec = (Convert.ToDouble(sale?.BAL)).ToString();


            if (livePortfolioVM.SaleRec != "0.00")
            {
                livePortfolioVM.LedgerBal = livePortfolioVM.LedgerBal;
            }
            else
            {
                livePortfolioVM.LedgerBal=Convert.ToDouble( livePortfolioVM.LedgerBal) + Convert.ToDouble(sale?.BAL);
            }

            query2 = @"SELECT SUM(CONVERT(decimal(20,2),totcost)) AS 'TotalCost',SUM(CONVERT(decimal(20,2),(a.totqty*c.iclose))) AS  
                'MarketValue',SUM(CONVERT(decimal(20,2),((a.totqty*c.iclose)))) AS 'GainLoss' FROM T_PF a,T_idxshrp c  
                WHERE a.acode='" + code + "' AND a.firmscd=c.firmscd AND c.tdate=(SELECT MAX(tdate) FROM T_idxshrp)";
            var totalBuy = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();

            livePortfolioVM.TotalBuyCost= Convert.ToDecimal(totalBuy?.TotalCost);
            livePortfolioVM.MarketVal =Convert.ToDecimal( mktValLive);  //totalBuy?.MarketValue;
            livePortfolioVM.EquityBal= livePortfolioVM.MarketVal + livePortfolioVM.LedgerBal - livePortfolioVM.AccruedBal;
            livePortfolioVM.UnrealiseBal =Convert.ToDecimal(Convert.ToDouble( mktValLive) -Convert.ToDouble( totalBuy?.TotalCost));
            livePortfolioVM.TotalCapital = livePortfolioVM.UnrealiseBal + livePortfolioVM.GainLossBalance;
            livePortfolioVM.ChargeFee = 0.00;


            return View(livePortfolioVM);
        }

        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult Ledger()
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
            //string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if(client ==null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            LedgerVM model = new()
            {
                ClientDetails=client,
                Code=code,
                LedgerDetails=new List<LedgerDetailsVM>(),
                FromDate=firstDayOfMonth,
                ToDate=DateTime.Now
            };


            return View(model);
        }

        [Authorize]
        public IActionResult LedgerDateToDate(string? fromDate,string? toDate,string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return NotFound();
            }
            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            double bal, opbal1;
            string query2 = @"SELECT sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' 
           then a.amount when '4' then -a.amount end) -a.commsn) as opbal  from t_sh a where a.acode ='" + code + "' AND a.tdate<'" + fromDate + "'";

            var priBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();
            opbal1 =Convert.ToDouble(priBal?.opbal);

            query2 = @"SELECT a.vno as vno,a.tdate AS tdate,(case a.b_or_s when '1' then 'Buy' when '2' then 
'Sale' when '3' then 'Receipt' when '4' then 'Payment' end) as  type,(case a.tran_type when 'T' then RTRIM(a.narr)+' '+rtrim(ISNULL(A.DOC_NO, '')) + (case when len(a.chqno) > 0 then ' [' +rtrim(a.chqno)+' ]' else '' end) when 'S' then b.firmsnm1 end) as narr,sum(a.quantity) 
as quantity,(CASE sum(a.quantity) WHEN 0 THEN 0 ELSE (SUM(a.amount)/sum(a.quantity)) END) 
as rate,sum(case a.b_or_s when '1' then a.amount when '2' then 0 when '3' then 0 when '4' then a.amount end) 
as debit, sum(case a.b_or_s when '1' then 0 when '2' then a.amount when '3' then a.amount when '4' then 0 end) 
as credit,sum(a.commsn) as commission, sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' 
then a.amount when '4' then -a.amount end) -a.commsn)  as balance,a.b_or_s,a.ttype FROM t_sh a, t_firms b 
WHERE a.firmscd=b.firmscd AND a.acode ='" + code + "' AND a.tdate BETWEEN '" + fromDate + "' AND '" + toDate + "'"+ 
"group by a.acode, a.tdate, b.firmsnm1, a.b_or_s, a.narr, a.vno, a.tran_type, A.DOC_NO, a.ttype, a.chqno ORDER BY a.tdate,a.vno, b.firmsnm1,a.b_or_s";


            bal = opbal1;
            var list = _unitofWork.SP_Call.ListByRawQueryBySis<LedgerDetailsVM>(query2).AsQueryable().Select(c=> new LedgerDetailsVM
            {
                Tdate= c.Tdate,
                Type= c.Type,
                Narr= c.Narr,
                Quantity= c.Quantity,
                Rate= c.Rate,
                Debit= c.Debit,
                Credit= c.Credit,
                Commission= c.Commission,
                Balance= c.Balance,
            }).ToList();

            foreach(var item in list) 
            {
                bal = bal + item.Balance.GetValueOrDefault();
                item.TotalBalance= bal;
            }
            ViewData["from"] = fromDate;
            ViewData["to"] = toDate;
            return PartialView("_LedgerDetails",list);
        }

        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult Confirmation()
        {
            string code=string.Empty;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            //string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            ConfirmationVM model = new()
            {
                ClientDetails= client,
                Code= code,
                FromDate=DateTime.Now,
                ConfirmationByDateVM=new ConfirmationByDateVM()
                {
                    ConfirmationDetailsList=new List<ConfirmationDetailsVM>()
                }
            };
           
            return View(model);
        }
        [Authorize]
        public IActionResult ConfirmationByDate(string? code,string? fromDate) 
        {
            try
            {
                double noat = 0;

                string query = @"SELECT acode,cocd,firmsnm1,SUM((case when b_or_s ='1' then quantity else 0.00 end)) AS buyQnty,  
SUM((case when b_or_s='1' then amount else 0.00 end)) AS Bamnt,SUM((case when b_or_s ='2' then quantity else 0.00 end))" +
    "AS saleQnty, SUM((case when b_or_s='2' then amount else 0.00 end)) AS Samnt,(SUM((case when b_or_s ='1' then quantity else 0.00 end))-SUM((case when b_or_s ='2' then quantity else 0.00 end))) AS 'BalQnty'," +
    "SUM(commsn) AS commsn,((SUM((case when b_or_s='2' then amount else 0.00 end)) - SUM((case when b_or_s='1' then amount else 0.00 end)))-SUM(commsn))" +
    "AS balance FROM T_Sh WHERE acode='" + code + "' AND tdate = '" + fromDate + "' GROUP BY acode,firmsnm1,cocd";

                var list = _unitofWork.SP_Call.ListByRawQueryBySis<ConfirmationDeatils>(query).AsQueryable().ToList();

                List<ConfirmationDetailsVM> list2 = new List<ConfirmationDetailsVM>();

                foreach (var item in list)
                {
                    ConfirmationDetailsVM confirmationDetails = new();
                    if (item.Cocd == "01")
                    {
                        confirmationDetails.Exch = "DSE";
                    }
                    if (item.Cocd == "02")
                    {
                        confirmationDetails.Exch = "CSE";
                    }
                    confirmationDetails.CODE = item?.Acode;
                    confirmationDetails.Instrument = item?.Firmsnm1;

                    if (Convert.ToDouble(item.BuyQnty) != 0.00)
                    {
                        confirmationDetails.BuyQty = item?.BuyQnty.ToString();
                        confirmationDetails.BuyAmt = item?.Bamnt.ToString();
                        confirmationDetails.BuyRate = (Convert.ToDouble(item?.Bamnt) / Convert.ToDouble(item?.BuyQnty)).ToString();
                    }
                    if (Convert.ToDouble(item.BuyQnty) == 0.00)
                    {
                        confirmationDetails.BuyQty = "0.00";
                        confirmationDetails.BuyAmt = "0.00";
                        confirmationDetails.BuyRate = "0.00";
                    }
                    if (Convert.ToDouble(item.SaleQnty) != 0.00)
                    {
                        confirmationDetails.SaleQty = item?.SaleQnty.ToString();
                        confirmationDetails.SaleAmt = item?.Samnt.ToString();
                        confirmationDetails.SaleRate = (Convert.ToDouble(item?.Samnt) / Convert.ToDouble(item?.SaleQnty)).ToString();
                    }
                    if (Convert.ToDouble(item.SaleQnty) == 0.00)
                    {
                        confirmationDetails.SaleQty = "0.00";
                        confirmationDetails.SaleAmt = "0.00";
                        confirmationDetails.SaleRate = "0.00";
                    }
                    confirmationDetails.BalQty = item.BalQnty.ToString();
                    confirmationDetails.Com_B_S = item.Commsn.ToString();
                    confirmationDetails.Balance = item.Balance.ToString();

                    noat += Convert.ToDouble(item.Balance);

                    list2.Add(confirmationDetails);
                }
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@ACODE", code);
                dynamicParameters.Add("@TDATE", Convert.ToDateTime(fromDate));
                dynamicParameters.Add("@COCD", "00");
                var listBal = _unitofWork.SP_Call.ReturnListFromSis<dynamic>("SP_CLIENT_BALANCE_SINGLE", dynamicParameters).FirstOrDefault();
                ConfirmationByDateVM model = new()
                {
                    ConfirmationDetailsList = list2,
                    Ledger = Convert.ToString(listBal.prebal),
                    Reciept = Convert.ToString(listBal.recamt),
                    Payment = Convert.ToString(listBal.Payamt),
                    NetAmountTrading = noat.ToString(),
                    ClosingBalance = Convert.ToString(listBal.todbal),
                    
                };

                return PartialView("_ConfirmationByDate", model);
            }
            catch(Exception ex) 
            {
                return Json( new { success = false, msg = ex.Message });
            }
        }

        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult Receipt()
        {
            string code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            ReceiptVM model = new()
            {
                ClientDetails= client,
                Code= code,
                ReceiptDetails=new List<ReceiptDetails>(),
                FromDate=DateTime.Now,
                ToDate=DateTime.Today
            };
            return View(model);
        }

        [Authorize]
        public IActionResult ReceiptByDate(string? code, string? fromDate,string? toDate)
        {
            try
            {
                string query = @"SELECT vno AS 'VoucherNo',tdate AS 'Date',(case b_or_s when 3 then amount end) AS 
     Deposit, (case b_or_s when 4 then amount end) AS Withdraw FROM T_SH WHERE acode='" + code + "' " +
     "AND tran_type='T' AND tdate BETWEEN '" + fromDate + "' AND '" + toDate + "' ORDER BY tdate ";
                var list = _unitofWork.SP_Call.ListByRawQueryBySis<ReceiptDetails>(query).AsQueryable().ToList();


                return PartialView("_ReceiptByDate", list);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            
        }
        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult Tax()
        {
            string code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = DateTime.Now;

            string query = @"EXEC uSP_Tax_report '" + code + "','" + startDate + "','" + endDate + "'";
            var tax = _unitofWork.SP_Call.ListByRawQuery<TaxDateToDateVM>(query).AsQueryable().FirstOrDefault();


            TaxVM model = new()
            {
                ClientDetails= client,
                Code= code,
                FromDate= startDate,
                ToDate= endDate,
                TaxPartial= new TaxPartialVM()
                {
                    ClientDetails=client,
                    TaxDateToDate=tax,
                    FromDate=startDate,
                    ToDate=endDate,
                }
            };
            return View(model);
        }

        [Authorize]
        public IActionResult TaxDateToDate(string? code, string? fromDate, string? toDate)
        {
            if (code == null)
            {
                return NotFound();
            }
            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }

            string query = @"EXEC uSP_Tax_report '" + code + "','" + fromDate + "','" + toDate + "'";
            var tax = _unitofWork.SP_Call.ListByRawQuery<TaxDateToDateVM>(query).AsQueryable().FirstOrDefault();

             if(tax?.Charge==null)
            {
                tax.RG = (tax.ClosingEquity + tax.Withdraw + tax.SW) - (tax.OpeningEquity + tax.Deposit + tax.SD);
                tax.Charge = 0;
            }

            TaxPartialVM model = new()
            {
                ClientDetails=client,
                TaxDateToDate=tax,
                FromDate=Convert.ToDateTime(fromDate),
                ToDate=Convert.ToDateTime(toDate)
            };
            return PartialView("_TaxDateToDate",model);
        }


        [Authorize]
        public IActionResult PortfolioDetails()
        {
            string code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = DateTime.Now;


            // double portfolioValueMarketPrice = 0;
            // double PortfolioValueCostPrice = 0;
            // //double PortfolioOpeningShareBalance = 0;
            // double realiseGain = 0;
            // double OpeningShareBal = 0;



            // int flag = 0;
            // int findFlag = 0;
            // double totalBCost = 0;
            // double totalSCost = 0;
            // double totalRG = 0;
            // double totalBCost1 = 0;
            // double totalSCost1 = 0;
            // double totalRG1 = 0;
            // double TotalBA = 0;
            // double TotalBA1 = 0;
            // double TotalMA = 0;
            // double TotalMA1 = 0;
            // double TotalUR = 0;
            // double TotalUR1 = 0;

            // DateTime now = DateTime.Now;
            // var startDate = new DateTime(now.Year, now.Month, 1);
            // var endDate = DateTime.Now;


            // string query = @"SELECT DISTINCT firmscd FROM SISROYALU.dbo.T_COS ab 
            //WHERE ab.acode='" + code + "' AND ab.tdate<= '" + endDate + "' AND ab.tran_type='S' GROUP BY ab.firmscd";

            // var firms = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().ToList(); //.Select(c => new string

            // foreach(var firm in firms)
            // {
            //     string quer = @"INSERT INTO OpeningShare EXEC [SISROYALU].[dbo].[sp_cos_ss_dt] '" + code + "','" + firm.firmscd + "' ,'" + startDate + "','" + endDate + "' ";
            //      _unitofWork.SP_Call.ExecuteWithoutReturnByQuerySis(quer);
            // }

            // string query2 = @"EXEC [dbo].[uSP_PL] '" + code + "','" + startDate + "','" + endDate + "'";

            // var plList = _unitofWork.SP_Call.ListByRawQuery<PlDetails>(query2).AsQueryable().ToList();
            // List<PlDetailList> list = new List<PlDetailList>();
            // foreach(var pl in plList) 
            // {
            //     if (Convert.ToDouble(pl.BuyQnty.ToString()) != 0 && (pl?.SaleQnt?.ToString() != null || pl?.SaleQnt?.ToString() != ""))
            //     {
            //         flag++;
            //         PlDetailList model = new()
            //         {
            //             SL = flag,
            //             Firmsnm1 = pl?.Firmsnm1,
            //             BuyQnty=pl.BuyQnty,
            //             BuyAmount=pl?.BuyAmount,
            //             SaleQnt=pl?.SaleQnt,
            //             SaleAmnt=pl?.SaleAmnt,
            //             RG=pl?.RG,
            //             BQ=pl?.BQ,
            //             BR=pl?.BR,
            //             BA=pl?.BA,
            //             TMR=pl?.TMR,
            //             TMA=pl?.TMA,
            //             TUG=pl?.TUG,
            //         };

            //         if (pl?.BuyAmount?.ToString() == "") totalBCost1 = 0;
            //         else totalBCost1 = Convert.ToDouble(pl?.BuyAmount?.ToString());
            //         totalBCost += totalBCost1;

            //         if (pl?.SaleAmnt?.ToString() == "") totalSCost1 = 0;
            //         else totalSCost1 = Convert.ToDouble(pl?.SaleAmnt?.ToString());
            //         totalSCost += totalSCost1;

            //         if (pl?.RG?.ToString() == "") totalRG1 = 0;
            //         else totalRG1 = Convert.ToDouble(pl?.RG?.ToString());
            //         totalRG += totalRG1;

            //         if (pl?.BA?.ToString() == "") TotalBA1 = 0;
            //         else TotalBA1 = Convert.ToDouble(pl?.BA?.ToString());
            //         TotalBA += TotalBA1;

            //         if (pl?.TMA?.ToString() == "") TotalMA1 = 0;
            //         else TotalMA1 = Convert.ToDouble(pl?.TMA?.ToString());
            //         TotalMA += TotalMA1;

            //         if (pl?.TUG?.ToString() == "") TotalUR1 = 0;
            //         else TotalUR1 = Convert.ToDouble(pl?.TUG?.ToString());
            //         TotalUR += TotalUR1;


            //         list.Add(model);
            //     }
            // }

            // PlDetailsPartialVM plDetailsPartial = new()
            // {
            //     PlDetailList=list,
            //     BoughtCost=Convert.ToDecimal(totalBCost),
            //     SoldCost=Convert.ToDecimal(totalSCost),
            //     RealisedGain_Loss=Convert.ToDecimal(totalRG),
            //     BalanceAmnt=Convert.ToDecimal(TotalBA),
            //     MarketAmnt=Convert.ToDecimal(TotalMA),
            //     UnrealisedGain=Convert.ToDecimal(TotalUR)
            // };

            // portfolioValueMarketPrice = TotalMA;
            // PortfolioValueCostPrice = TotalBA;
            // realiseGain = totalRG;


            //  query2 = @"SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + code + "' AND cert_no='IPO' AND a.dtofent BETWEEN '" + startDate + "' AND '" + endDate + "'";


            //  var ipoShareList = _unitofWork.SP_Call.ListByRawQuery<IPOShareList>(query2).AsQueryable().ToList();
            //  plDetailsPartial.IPOShareLists = ipoShareList;

            // query2 = @"SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + code + "' AND cert_no='BONUS' AND a.dtofent BETWEEN '" + startDate + "' AND '" + endDate + "'";


            // var bonusShareList = _unitofWork.SP_Call.ListByRawQuery<IPOShareList>(query2).AsQueryable().ToList();
            // plDetailsPartial.BonusShareLists = bonusShareList;

            // query2 = @"SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + code + "' AND cert_no='RIGHT' AND a.dtofent BETWEEN '" + startDate + "' AND '" + endDate + "'";


            // var rightShareList = _unitofWork.SP_Call.ListByRawQuery<IPOShareList>(query2).AsQueryable().ToList();
            // plDetailsPartial.RightShareLists = rightShareList;

            // query2 = @"EXEC [dbo].[uSP_PL_CAL] '" + code + "','" + startDate + "','" + endDate + "'";
            // var calculation = _unitofWork.SP_Call.ListByRawQuery<dynamic>(query2).AsQueryable().FirstOrDefault();

            // plDetailsPartial.LedgerBal = calculation?.Ledger;
            // plDetailsPartial.PortfolioValueMarket=Convert.ToDecimal( portfolioValueMarketPrice);
            // plDetailsPartial.PortfolioValueCost = Convert.ToDecimal(PortfolioValueCostPrice);
            // plDetailsPartial.Deposit=calculation?.deposit;
            // plDetailsPartial.WithdrawnAmount=calculation?.withdraw;
            // plDetailsPartial.Charges=calculation?.charge;
            // plDetailsPartial.NetDeposit=calculation?.NetDeposit;
            // plDetailsPartial.RealisedCapitalGainLoss = totalRG;

            // query2 = @"EXEC SISROYALU.[dbo].[SP_INV_TCOS] '" + startDate + "','" + endDate + "'";
            // var openingBal = _unitofWork.SP_Call.ListByRawQuery<dynamic>(query2).AsQueryable().ToList();

            // foreach(var item in openingBal)
            // {
            //     if (Convert.ToDouble(item?.rate) != 0)
            //     {
            //         OpeningShareBal += Convert.ToDouble(item?.amount);
            //     }
            // }
            // plDetailsPartial.OpeningShareBal = OpeningShareBal;

            PlDetailVM plDetailVM = new()
            {
                PlDetailsPartial= GetPlDetailsPartial(code,startDate,endDate),
                ClientDetails=client,
                Code=code,
                FromDate=startDate,
                ToDate=endDate,
            };
            return View(plDetailVM);
        }


        [Authorize]
        public IActionResult PlDetailsByDate(string fromDate,string toDate)
        {
            string code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }
            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }

            var model = GetPlDetailsPartial(code,Convert.ToDateTime(fromDate),Convert.ToDateTime(toDate));
            return PartialView("_PlDetailsByDate", model);
        }
        private PlDetailsPartialVM GetPlDetailsPartial(string code,DateTime startDate,DateTime endDate)
        {
            //if (code == null)
            //{
            //    return NotFound();
            //}
            //string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();

            //if (client == null)
            //{
            //    return NotFound();
            //}
            double portfolioValueMarketPrice = 0;
            double PortfolioValueCostPrice = 0;
            //double PortfolioOpeningShareBalance = 0;
            double realiseGain = 0;
            double OpeningShareBal = 0;



            int flag = 0;
            int findFlag = 0;
            double totalBCost = 0;
            double totalSCost = 0;
            double totalRG = 0;
            double totalBCost1 = 0;
            double totalSCost1 = 0;
            double totalRG1 = 0;
            double TotalBA = 0;
            double TotalBA1 = 0;
            double TotalMA = 0;
            double TotalMA1 = 0;
            double TotalUR = 0;
            double TotalUR1 = 0;

            //DateTime now = DateTime.Now;
            //var startDate = new DateTime(now.Year, now.Month, 1);
            //var endDate = DateTime.Now;


            string query = @"SELECT DISTINCT firmscd FROM SISROYALU.dbo.T_COS ab 
           WHERE ab.acode='" + code + "' AND ab.tdate<= '" + endDate + "' AND ab.tran_type='S' GROUP BY ab.firmscd";

            var firms = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().ToList(); //.Select(c => new string

            foreach (var firm in firms)
            {
                string quer = @"INSERT INTO OpeningShare EXEC [SISROYALU].[dbo].[sp_cos_ss_dt] '" + code + "','" + firm.firmscd + "' ,'" + startDate + "','" + endDate + "' ";
                _unitofWork.SP_Call.ExecuteWithoutReturnByQuery(quer);
            }

            string query2 = @"EXEC [dbo].[uSP_PL] '" + code + "','" + startDate + "','" + endDate + "'";

            var plList = _unitofWork.SP_Call.ListByRawQuery<PlDetails>(query2).AsQueryable().ToList();
            List<PlDetailList> list = new List<PlDetailList>();
            foreach (var pl in plList)
            {
                if (Convert.ToDouble(pl.BuyQnty.ToString()) != 0 && (pl?.SaleQnt?.ToString() != null || pl?.SaleQnt?.ToString() != ""))
                {
                    flag++;
                    PlDetailList model = new()
                    {
                        SL = flag,
                        Firmsnm1 = pl?.Firmsnm1,
                        BuyQnty = pl.BuyQnty,
                        BuyAmount = pl?.BuyAmount,
                        SaleQnt = pl?.SaleQnt,
                        SaleAmnt = pl?.SaleAmnt,
                        RG = pl?.RG,
                        BQ = pl?.BQ,
                        BR = pl?.BR,
                        BA = pl?.BA,
                        TMR = pl?.TMR,
                        TMA = pl?.TMA,
                        TUG = pl?.TUG,
                    };

                    if (pl?.BuyAmount?.ToString() == "") totalBCost1 = 0;
                    else totalBCost1 = Convert.ToDouble(pl?.BuyAmount?.ToString());
                    totalBCost += totalBCost1;

                    if (pl?.SaleAmnt?.ToString() == "") totalSCost1 = 0;
                    else totalSCost1 = Convert.ToDouble(pl?.SaleAmnt?.ToString());
                    totalSCost += totalSCost1;

                    if (pl?.RG?.ToString() == "") totalRG1 = 0;
                    else totalRG1 = Convert.ToDouble(pl?.RG?.ToString());
                    totalRG += totalRG1;

                    if (pl?.BA?.ToString() == "") TotalBA1 = 0;
                    else TotalBA1 = Convert.ToDouble(pl?.BA?.ToString());
                    TotalBA += TotalBA1;

                    if (pl?.TMA?.ToString() == "") TotalMA1 = 0;
                    else TotalMA1 = Convert.ToDouble(pl?.TMA?.ToString());
                    TotalMA += TotalMA1;

                    if (pl?.TUG?.ToString() == "") TotalUR1 = 0;
                    else TotalUR1 = Convert.ToDouble(pl?.TUG?.ToString());
                    TotalUR += TotalUR1;


                    list.Add(model);
                }
            }

            PlDetailsPartialVM plDetailsPartial = new()
            {
                PlDetailList = list,
                BoughtCost = Convert.ToDecimal(totalBCost),
                SoldCost = Convert.ToDecimal(totalSCost),
                RealisedGain_Loss = Convert.ToDecimal(totalRG),
                BalanceAmnt = Convert.ToDecimal(TotalBA),
                MarketAmnt = Convert.ToDecimal(TotalMA),
                UnrealisedGain = Convert.ToDecimal(TotalUR),
                FromDate=startDate,
                ToDate=endDate
            };

            portfolioValueMarketPrice = TotalMA;
            PortfolioValueCostPrice = TotalBA;
            realiseGain = totalRG;


            query2 = @"SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + code + "' AND cert_no='IPO' AND a.dtofent BETWEEN '" + startDate + "' AND '" + endDate + "'";


            var ipoShareList = _unitofWork.SP_Call.ListByRawQueryBySis<IPOShareList>(query2).AsQueryable().ToList();
            plDetailsPartial.IPOShareLists = ipoShareList;

            query2 = @"SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + code + "' AND cert_no='BONUS' AND a.dtofent BETWEEN '" + startDate + "' AND '" + endDate + "'";


            var bonusShareList = _unitofWork.SP_Call.ListByRawQueryBySis<IPOShareList>(query2).AsQueryable().ToList();
            plDetailsPartial.BonusShareLists = bonusShareList;

            query2 = @"SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + code + "' AND cert_no='RIGHT' AND a.dtofent BETWEEN '" + startDate + "' AND '" + endDate + "'";


            var rightShareList = _unitofWork.SP_Call.ListByRawQueryBySis<IPOShareList>(query2).AsQueryable().ToList();
            plDetailsPartial.RightShareLists = rightShareList;

            query2 = @"EXEC [dbo].[uSP_PL_CAL] '" + code + "','" + startDate + "','" + endDate + "'";
            var calculation = _unitofWork.SP_Call.ListByRawQuery<dynamic>(query2).AsQueryable().FirstOrDefault();

            plDetailsPartial.LedgerBal = calculation?.Ledger;
            plDetailsPartial.PortfolioValueMarket = Convert.ToDecimal(portfolioValueMarketPrice);
            plDetailsPartial.PortfolioValueCost = Convert.ToDecimal(PortfolioValueCostPrice);
            plDetailsPartial.Deposit = calculation?.deposit;
            plDetailsPartial.WithdrawnAmount = calculation?.withdraw;
            plDetailsPartial.Charges = calculation?.charge;
            plDetailsPartial.NetDeposit = calculation?.NetDeposit;
            plDetailsPartial.RealisedCapitalGainLoss = totalRG;

            query2 = @"EXEC SISROYALU.[dbo].[SP_INV_TCOS] '" + startDate + "','" + code + "'";
            var openingBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().ToList();

            foreach (var item in openingBal)
            {
                if (Convert.ToDouble(item?.rate) != 0)
                {
                    OpeningShareBal += Convert.ToDouble(item?.amount);
                }
            }
            plDetailsPartial.OpeningShareBal = OpeningShareBal;

            return plDetailsPartial;
        }


        //////////////// Report Section //////////////////////
        [Authorize]
        public IActionResult PortfolioPDF()
        {
            string? code = string.Empty;
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

            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@acode", code);
            var list = _unitofWork.SP_Call.ReturnListFromSis<PortfolioCompanyVM>("inv_pfs_td", dynamicParameters).ToList();

            list.RemoveAll(x => x.Grp != 0);

            string? query = string.Empty;


            ClientPortfolioVM model = new()
            {
                PortfolioCompanyVMs = list,
                ClientDetails = client,
                RglBal=0
            };
            //            query = @"SELECT rgl FROM T_RGL WHERE acode='" + code + "' ";
            //            var rglBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            //            // model.RglBal = rglBal?.rgl;
            //            if (rglBal != null)
            //            {
            //                model.RglBal = rglBal?.rgl;
            //            }

            //            query = "SELECT intamt FROM T_intsum WHERE acode='" + code + "'";
            //            var accured = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            //            model.AccruedBal = accured?.intamt;


            //            query = "SELECT matbalP,ldgbal FROM T_Tkbal WHERE acode = '" + code + "'";
            //            var mat = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            //            model.MaturedBal = mat?.matbalP;
            //            model.LedgerBal = mat?.ldgbal;

            //            model.SaleRec = Convert.ToDecimal(0.00);
            //            model.ChargeFee = Convert.ToDecimal(0.00);

            //            query = "SELECT BAL FROM T_SRTD WHERE ACODE='" + code + "'";
            //            var bal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();
            //            //  model.SaleRec = (bal?.BAL);
            //            if (bal != null)
            //            {
            //                model.SaleRec = bal?.BAL;
            //            }

            //            // if (Convert.ToString( model.SaleRec) != "0.00")
            //            if (model.SaleRec != 0)
            //            {
            //                model.LedgerBal = model.LedgerBal;
            //            }
            //            else
            //            {
            //                //  model.LedgerBal += bal?.BAL;
            //                model.LedgerBal = model.LedgerBal + model.SaleRec;
            //            }

            //            query = @"SELECT IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'AppliedDate' FROM RCLWEB.dbo.T_IPO_REC " +
            //                "WHERE RCODE='" + code + "' AND IPO_RESULT='' and DATEDIFF(DAY, ENTRY_DATE, GETDATE())<100 UNION ALL SELECT " +
            //                "IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'AppliedDate' FROM RCLWEB.dbo.T_IPO_REC_MASTER " +
            //                "WHERE RCODE='" + code + "' AND IPO_RESULT='' and DATEDIFF(DAY,ENTRY_DATE,GETDATE())<100";

            //            var ipoList = _unitofWork.SP_Call.ListByRawQuery<PendingIPOVM>(query).AsQueryable().ToList();

            //            query = @"SELECT SUM(CONVERT(decimal(20,2),totcost)) AS 'TotalCost',SUM(CONVERT(decimal(20,2),(a.totqty*c.iclose))) AS 
            //'MarketValue',SUM(CONVERT(decimal(20,2),((a.totqty*c.iclose)-(a.totcost)))) AS 'GainLoss' FROM T_PF a,T_idxshrp c WHERE a.acode='" + code + "' " +
            //"AND a.firmscd=c.firmscd AND c.tdate=(SELECT MAX(tdate) FROM T_idxshrp)";
            //            var totalBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query).AsQueryable().FirstOrDefault();

            //            model.TotalBuyCost = totalBal?.TotalCost;
            //            model.MarketVal = totalBal?.MarketValue;
            //            model.EquityBal = totalBal?.MarketValue + model.LedgerBal - model.AccruedBal;
            //            model.UnrealiseBal = totalBal?.GainLoss;
            //            model.TotalCapital = model.UnrealiseBal + model.RglBal;

            //            model.PendingShares = ipoList;

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


            string cusomtSwitches = string.Format("--print-media-type --allow {0} --footer-html {0}", Url.Action("ReportFooter", "Report", new {  }, "https"));
            return new ViewAsPdf("PortfolioPDF",model)
            {
                //CustomSwitches = "--page-offset 0 --footer-left [page] --footer-font-size 12"
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(3, 3, 9, 3)
            };
            //return View();
        }

        [AllowAnonymous]
        public IActionResult ReportFooter()
        {
            //var product = _unitofWork.Product.GetFirstOrDefault(u => u.Id == productId);
            var print = DateTimeOffset.Now.ToString("dd MMM, yyyy hh:mm:ss tt K");
            return PartialView("_ReportFooter",print);
        }

        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult LedgerPdf(string? fromDate, string? toDate)
        {
            string code=string.Empty;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            if (string.IsNullOrEmpty(code))
            {
                return NotFound();
            }
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            double bal, opbal1;
            string query2 = @"SELECT sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' 
           then a.amount when '4' then -a.amount end) -a.commsn) as opbal  from t_sh a where a.acode ='" + code + "' AND a.tdate<'" + fromDate + "'";

            var priBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();
            opbal1 = Convert.ToDouble(priBal?.opbal);

            query2 = @"SELECT a.vno as vno,a.tdate AS tdate,(case a.b_or_s when '1' then 'Buy' when '2' then 
'Sale' when '3' then 'Receipt' when '4' then 'Payment' end) as  type,(case a.tran_type when 'T' then RTRIM(a.narr)+' '+rtrim(ISNULL(A.DOC_NO, '')) + (case when len(a.chqno) > 0 then ' [' +rtrim(a.chqno)+' ]' else '' end) when 'S' then b.firmsnm1 end) as narr,sum(a.quantity) 
as quantity,(CASE sum(a.quantity) WHEN 0 THEN 0 ELSE (SUM(a.amount)/sum(a.quantity)) END) 
as rate,sum(case a.b_or_s when '1' then a.amount when '2' then 0 when '3' then 0 when '4' then a.amount end) 
as debit, sum(case a.b_or_s when '1' then 0 when '2' then a.amount when '3' then a.amount when '4' then 0 end) 
as credit,sum(a.commsn) as commission, sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' 
then a.amount when '4' then -a.amount end) -a.commsn)  as balance,a.b_or_s,a.ttype FROM t_sh a, t_firms b 
WHERE a.firmscd=b.firmscd AND a.acode ='" + code + "' AND a.tdate BETWEEN '" + fromDate + "' AND '" + toDate + "'" +
"group by a.acode, a.tdate, b.firmsnm1, a.b_or_s, a.narr, a.vno, a.tran_type, A.DOC_NO, a.ttype, a.chqno ORDER BY a.tdate,a.vno, b.firmsnm1,a.b_or_s";


            bal = opbal1;
            var list = _unitofWork.SP_Call.ListByRawQueryBySis<LedgerDetailsVM>(query2).AsQueryable().Select(c => new LedgerDetailsVM
            {
                Vno=c.Vno,
                Tdate = c.Tdate,
                Type = c.Type,
                Narr = c.Narr,
                Quantity = c.Quantity,
                Rate = c.Rate,
                Debit = c.Debit,
                Credit = c.Credit,
                Commission = c.Commission,
                Balance = c.Balance,
            }).ToList();

            foreach (var item in list)
            {
                bal += item.Balance.GetValueOrDefault();
                item.TotalBalance = bal;
            }
            LedgerVM model = new()
            {
                ClientDetails = client,
                Code = code,
                LedgerDetails = list,
                FromDate =Convert.ToDateTime(fromDate),
                ToDate = Convert.ToDateTime(toDate)
            };
           
            string cusomtSwitches = string.Format("--print-media-type --allow {0} --footer-html {0}", Url.Action("ReportFooter", "Report", new { }, "https"));
            return new ViewAsPdf("LedgerPDF", model)
            {
                //CustomSwitches = "--page-offset 0 --footer-left [page] --footer-font-size 12"
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(3, 3, 9, 3)
            };
        }

        [HttpPost]
        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult TaxPDF(string? fromDate, string? toDate)
        {
            string code = string.Empty;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            if (string.IsNullOrEmpty(code))
            {
                return NotFound();
            }
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
          //  DateTime now = DateTime.Now;
            var startDate = Convert.ToDateTime(fromDate);
            var endDate = Convert.ToDateTime(toDate);

            string query = @"EXEC uSP_Tax_report '" + code + "','" + startDate + "','" + endDate + "'";
            var tax = _unitofWork.SP_Call.ListByRawQuery<TaxDateToDateVM>(query).AsQueryable().FirstOrDefault();


            TaxVM model = new()
            {
                ClientDetails = client,
                Code = code,
                TaxPartial = new TaxPartialVM()
                {
                    ClientDetails = client,
                    TaxDateToDate = tax,
                    FromDate = startDate,
                    ToDate = endDate,
                }
            };
            string cusomtSwitches = string.Format("--print-media-type --allow {0} --footer-html {0}", Url.Action("ReportFooter", "Report", new { }, "https"));
            return new ViewAsPdf("TaxPDF", model)
            {
                //CustomSwitches = "--page-offset 0 --footer-left [page] --footer-font-size 12"
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(3, 3, 9, 3)
            };
        }

        [HttpPost]
        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult PortfolioDetailsPDF(string? fromDate, string? toDate)
        {
            string code;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            else
            {
                return RedirectToAction("ViewAllCodes", "Home");
            }

            //var client = _clientServie.GetClientDetails(code);
            string query3 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname,aatype,jname1 FROM T_CLIENT WHERE acode='" + code + "' ";
            var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query3).AsQueryable().FirstOrDefault();
            if (client == null)
            {
                return NotFound();
            }
            PlDetailVM plDetailVM = new()
            {
                PlDetailsPartial = GetPlDetailsPartial(code, Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)),
                ClientDetails = client,
                Code = code,
                FromDate = Convert.ToDateTime(fromDate),
                ToDate = Convert.ToDateTime(toDate),
            };
            //var model = GetPlDetailsPartial(code, Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate));

            string cusomtSwitches = string.Format("--print-media-type --allow {0} --footer-html {0}", Url.Action("ReportFooter", "Report", new { }, "https"));
            return new ViewAsPdf("PortfolioDetailsPDF", plDetailVM)
            {
                //CustomSwitches = "--page-offset 0 --footer-left [page] --footer-font-size 12"
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(3, 3, 9, 3)
            };
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
            var actionPDF = new ViewAsPdf("PortfolioPDF", model)
            {
                //CustomSwitches = "--page-offset 0 --footer-left [page] --footer-font-size 12"
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(3, 3, 9, 3)
            };
            byte[] applicationPDFData =await actionPDF.BuildFile(ControllerContext);
            return applicationPDFData;
        }

        public  HttpResponseMessage Pdf()
        {
            ReportController controller = new ReportController(_unitofWork, _clientServie);
            RouteData route = new RouteData();
            route.Values.Add("action", "PortfolioPDFTest"); // ActionName
            route.Values.Add("controller", "Report"); // Controller Name
            //var newContext = new System.Web.Mvc.ControllerContext(new HttpContextWrapper(System.Web.HttpContext.Current), route, controller);
            //controller.ControllerContext = newContext;
            var actionPDF = controller.PortfolioPDFTest();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(actionPDF.Result);// new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "SamplePDF.PDF";
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return response;
        }

        public async Task<IActionResult> Result()
        {
            var file =await PortfolioPDFTest();
            //return File(file, "application/pdf", "sample.pdf");
            MemoryStream ms = new MemoryStream(file);
            return new FileStreamResult(ms, "application/pdf");       
        }

        [Authorize]
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult LedgerAsSis(string? fromDate, string? toDate)
        {
            string code = string.Empty;
            if (HttpContext.Session.GetString(ExtraAct.clientCode) != null)
            {
                code = HttpContext.Session.GetString(ExtraAct.clientCode);
            }
            if (string.IsNullOrEmpty(code))
            {
                return NotFound();
            }
            var client = _clientServie.GetClientDetails(code);
            if (client == null)
            {
                return NotFound();
            }
            double bal, opbal1;
            string query2 = @"SELECT sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' 
           then a.amount when '4' then -a.amount end) -a.commsn) as opbal  from t_sh a where a.acode ='" + code + "' AND a.tdate<'" + fromDate + "'";

            var priBal = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();
            opbal1 = Convert.ToDouble(priBal?.opbal);

            query2 = @"SELECT a.vno as vno,a.tdate AS tdate,(case a.b_or_s when '1' then 'Buy' when '2' then 
'Sale' when '3' then 'Receipt' when '4' then 'Payment' end) as  type,(case a.tran_type when 'T' then RTRIM(a.narr)+' '+rtrim(ISNULL(A.DOC_NO, '')) + (case when len(a.chqno) > 0 then ' [' +rtrim(a.chqno)+' ]' else '' end) when 'S' then b.firmsnm1 end) as narr,sum(a.quantity) 
as quantity,(CASE sum(a.quantity) WHEN 0 THEN 0 ELSE (SUM(a.amount)/sum(a.quantity)) END) 
as rate,sum(case a.b_or_s when '1' then a.amount when '2' then 0 when '3' then 0 when '4' then a.amount end) 
as debit, sum(case a.b_or_s when '1' then 0 when '2' then a.amount when '3' then a.amount when '4' then 0 end) 
as credit,sum(a.commsn) as commission, sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' 
then a.amount when '4' then -a.amount end) -a.commsn)  as balance,a.b_or_s,a.ttype FROM t_sh a, t_firms b 
WHERE a.firmscd=b.firmscd AND a.acode ='" + code + "' AND a.tdate BETWEEN '" + fromDate + "' AND '" + toDate + "'" +
"group by a.acode, a.tdate, b.firmsnm1, a.b_or_s, a.narr, a.vno, a.tran_type, A.DOC_NO, a.ttype, a.chqno ORDER BY a.tdate,a.vno, b.firmsnm1,a.b_or_s";


            bal = opbal1;
            var list = _unitofWork.SP_Call.ListByRawQueryBySis<LedgerDetailsVM>(query2).AsQueryable().Select(c => new LedgerDetailsVM
            {
                Vno = c.Vno,
                Tdate = c.Tdate,
                Type = c.Type,
                Narr = c.Narr,
                Quantity = c.Quantity,
                Rate = c.Rate,
                Debit = c.Debit,
                Credit = c.Credit,
                Commission = c.Commission,
                Balance = c.Balance,
            }).ToList();

            foreach (var item in list)
            {
                bal += item.Balance.GetValueOrDefault();
                item.TotalBalance = bal;
            }
            LedgerVM model = new()
            {
                ClientDetails = client,
                Code = code,
                LedgerDetails = list,
                FromDate = Convert.ToDateTime(fromDate),
                ToDate = Convert.ToDateTime(toDate)
            };

            ReportHeaderVM reportHeader = new ()
            {
                aatype=client.aatype,
                Acode=client.Acode,
                Address=client.Address,
                Aname=client.Aname,
                Boid=client.Boid,
                Faname=client.Faname,
                jname1=client.jname1,
                moname=client.moname,
                FromDate=Convert.ToDateTime(fromDate),
                Title="Ledger",
                ToDate=Convert.ToDateTime(toDate),
            };

            string cusomtSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --footer-html {1}",
               Url.Action("ReportHeader", "Report", reportHeader, "https"), Url.Action("ReportFooter", "Report", new { }, "https"));
            return new ViewAsPdf("LedgerAsSis", model)
            {
                //CustomSwitches = "--page-offset 0 --footer-left [page] --footer-font-size 12"
                CustomSwitches = cusomtSwitches,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(50, 3, 16, 3)
            };
        }

        [AllowAnonymous]
        public IActionResult ReportHeader( ReportHeaderVM reportHeaderVM)
        {
            //var product = _unitofWork.Product.GetFirstOrDefault(u => u.Id == productId);
            return PartialView("_ReportHeader",reportHeaderVM);
        }


    }
}
