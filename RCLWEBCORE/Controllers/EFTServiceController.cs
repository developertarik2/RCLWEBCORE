using Microsoft.AspNetCore.Mvc;
using RCLWEB.DATA.ViewModels;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using RCLWEB.DATA.Models;
using RCLWEB.Utility;
using Microsoft.AspNetCore.Authorization;

namespace RCLWEBCORE.Controllers
{
    [Authorize]
    public class EFTServiceController : Controller
    {
        private readonly IUnitOfWork _unitofWork;

        private IClientService _clientServie;

        public EFTServiceController(IUnitOfWork unitofWork, IClientService clientService)
        {
            _unitofWork = unitofWork;
            _clientServie = clientService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult EFTStatus()
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
            var client = _clientServie.GetClientDetails(code);
            //string query2 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query2).AsQueryable().FirstOrDefault();

            if (client == null)
            {
                return NotFound();
            }
            //string query = "SELECT tdate AS Date,acode AS Code,amount AS Amount,posted,approved,rejected FROM T_EFT WHERE acode='" + code + "' ORDER BY tdate desc";
          string  query = @"SELECT TOP 10 * FROM (SELECT RCODE,amount,dat,flag1,flag2,CONVERT(VARCHAR(3),status) [status],clr FROM T_SMS_TRANSECTION WHERE 
RCODE='" + code + "' UNION ALL SELECT acode RCODE,amount AS amount, tdate AS dat,1 flag1,rejected flag2,(CASE approved " +
"WHEN 1 THEN 'Yes' ELSE 'No' END) [status],0 clr FROM SISROYALU.dbo.T_EFT WHERE acode='" + code + "') a ORDER BY a.dat desc";
            var reqReceive = _unitofWork.SP_Call.ListByRawQuery<RequisitionReceiveVM>(query).AsQueryable().ToList();

             query = @"SELECT tdate AS Date,acode AS Code,amount AS Amount,posted,approved,rejected FROM T_EFT WHERE acode='" + code + "' ORDER BY tdate desc";
            var reqApprove = _unitofWork.SP_Call.ListByRawQueryBySis<RequisitionApproveVM>(query).AsQueryable().ToList();


            RequisitionVM model = new()
            {
                ClientDetails= client,
                RequisitionReceive=reqReceive,
                RequisitionApprove=reqApprove
            };
            return View(model);
        }

        public IActionResult EftSubmit()
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
            var client = _clientServie.GetClientDetails(code);
            //string query2 = @"SELECT acode,aname,boid,(addr1+' '+addr2+' ' +city) address,faname,moname FROM T_CLIENT WHERE acode='" + code + "'";
            //var client = _unitofWork.SP_Call.ListByRawQuery<ClientDetailsVM>(query2).AsQueryable().FirstOrDefault();

            if (client == null)
            {
                return NotFound();
            }

            string query2 = @"SELECT matbalP,ldgbal FROM T_Tkbal WHERE acode='" + code + "'";
            var clientBalance = _unitofWork.SP_Call.ListByRawQueryBySis<dynamic>(query2).AsQueryable().FirstOrDefault();

            var list = _unitofWork.T_SMS_Transection.GetAll(u => u.RCODE == code && u.Flag1 == 1 && u.Flag2 == 0 && u.Clr == 0 && u.Status == "NO");

            EftSubmitVM model = new()
            {
                RCODE=code,
                LedgrBal=clientBalance?.ldgbal,
                MatureBal=clientBalance?.matbalP,
                PendingTrans=new List<T_SMS_TRANSECTION>(),
            };
            if(list.Any())
            {
                model.PendingTrans = list.ToList();
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult EftSubmit(EftSubmitVM eftSubmit)
        {
            if (eftSubmit.Amount == 0)
            {
                ModelState.AddModelError("", "Oops...!!! Amount is too low");
                eftSubmit.PendingTrans = _unitofWork.T_SMS_Transection.GetAll(u => u.RCODE == eftSubmit.RCODE && u.Flag1 == 1 
                && u.Flag2 == 0 && u.Clr == 0 && u.Status == "NO").ToList();

                return View(eftSubmit);
            }
            if(eftSubmit.MatureBal > eftSubmit.Amount)
            {
                var list = _unitofWork.T_SMS_Transection.GetAll(u => u.RCODE == eftSubmit.RCODE && u.Flag1==1 && u.Flag2==0 && u.Clr == 0 && u.Status=="NO");

                if(list.Any())
                {
                    ModelState.AddModelError("", "You have " + list.Count() + " unprocessed EFT application(s). For any query contact with our helpline: 09606555333");

                    eftSubmit.PendingTrans = list.ToList();
                    return View(eftSubmit);
                }
            }
            else
            {
                ModelState.AddModelError("", "You do not have sufficient balance to submit an EFT requisition");

                eftSubmit.PendingTrans = _unitofWork.T_SMS_Transection.GetAll(u => u.RCODE == eftSubmit.RCODE && u.Flag1 == 1
                && u.Flag2 == 0 && u.Clr == 0 && u.Status == "NO").ToList();


                return View(eftSubmit);
            }
            if (ModelState.IsValid)
            {
                string query = @"INSERT INTO T_SMS_TRANSECTION(SMS_Number,RCODE,amount,dat,flag1,flag2,status,clr) 
VALUES('WEB','" + eftSubmit.RCODE + "','" + eftSubmit.Amount + "','" + string.Format("{0:MM/dd/yyyy}", DateTime.Now) + ' ' + string.Format("{0:hh:mm:ss tt}", DateTime.Now) + "','1','0','NO','0')";
                _unitofWork.SP_Call.ExecuteWithoutReturnByQuery(query);


                insert_to_tracker("EFT Submit", eftSubmit.RCODE, Convert.ToInt32(eftSubmit.Amount));
                ViewData["msg"] = "Transaction Submitted...";

                eftSubmit.PendingTrans = _unitofWork.T_SMS_Transection.GetAll(u => u.RCODE == eftSubmit.RCODE && u.Flag1 == 1
                && u.Flag2 == 0 && u.Clr == 0 && u.Status == "NO").ToList();


                return View(eftSubmit);
            }
            else
            {
                eftSubmit.PendingTrans = _unitofWork.T_SMS_Transection.GetAll(u => u.RCODE == eftSubmit.RCODE && u.Flag1 == 1
                && u.Flag2 == 0 && u.Clr == 0 && u.Status == "NO").ToList();
                return View(eftSubmit);
            }
        }

        public int eft_posting_check(decimal matBal,decimal amountBal)
        {
            double amount = 0;
            amount = Convert.ToDouble(matBal) - Convert.ToDouble(amountBal);
            return 0;
        }

        void insert_to_tracker(string action_type, string user_id, int status = 1)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;


            string query = @"INSERT INTO T_WEB_USER_TRACKER (user_id, IP_address, action_type, status) VALUES ('" + user_id + "', '" + remoteIpAddress + "', '" + action_type + "', "+status+")";
            _unitofWork.SP_Call.ExecuteWithoutReturnByQuery(query);
        }

    }
}
