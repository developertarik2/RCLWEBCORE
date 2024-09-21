using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCLWEB.DATA.ViewModels;
using RCLWEB.Utility;
using RCLWEBCORE.Filters;
using RCLWEBCORE.Insfrastructures.InterfaceRepo;
using RCLWEBCORE.Insfrastructures.Services.Interfaces;
using RCLWEBCORE.SSL;
using System.Collections.Specialized;
using System.Security.Claims;

namespace RCLWEBCORE.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IUnitOfWork _unitofWork;

        private IClientService _clientServie;
        private ITrackerService _trackerService;
        public PaymentController(IUnitOfWork unitofWork, IClientService clientServie, ITrackerService trackerService)
        {
            _unitofWork = unitofWork;
            _clientServie = clientServie;
            _trackerService = trackerService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [TypeFilter(typeof(CustomAuthorize))]
        public IActionResult PaymentSSL()
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
            PaymentVM model = new()
            {
                Code=code,
                Amount=10
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult PaymentSSL(PaymentVM paymentVM)
        {
            if(ModelState.IsValid)
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
                if(paymentVM.Code != code)
                {
                    ModelState.AddModelError("", "Something went error!!!");
                    return View();
                }
                var client = _clientServie.GetClientDetails(code);
                if (client == null)
                {
                    return NotFound();
                }
                var username = HttpContext?.User.FindFirstValue(ClaimTypes.Name);
                var remoteIpAddress = HttpContext?.Connection.RemoteIpAddress;
                string amount = paymentVM.Amount.ToString();
                string client_name = client.Aname;
                //Session["ssl_amount"] = txttransferAmount.Value;
                HttpContext?.Session.SetString("ssl_amount", amount);

                //string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
                string baseUrl = Request.Scheme + "://" + Request.Host; //+ HttpContext.Request.Path.ToString().TrimEnd('/') + "/";


                //CREATING LIST OF POST DATA
                   NameValueCollection PostData = new()
               {
                    { "total_amount", $"{amount}" },
                    { "tran_id", GenerateUniqueId() },
                    { "success_url", baseUrl + "/Payment/PaymentSuccess" },
                    { "fail_url", baseUrl + "/Payment/PaymentFail" }, // "Fail.aspx" page needs to be created
                    { "cancel_url", baseUrl + "/Payment/PaymentError" }, // "Cancel.aspx" page needs to be created
                    { "version", "3.00" },
                    { "cus_name", $"{client_name}" },
                    { "cus_email", "abc.xyz@mail.co" },
                    { "cus_add1", "Address Line On" },
                    { "cus_add2", "Address Line Tw" },
                    { "cus_city", "City Nam" },
                    { "cus_state", "State Nam" },
                    { "cus_postcode", "Post Cod" },
                    { "cus_country", "Countr" },
                    { "cus_phone", "0111111111" },
                    { "cus_fax", "0171111111" },
                    { "ship_name", "ABC XY" },
                    { "ship_add1", "Address Line On" },
                    { "ship_add2", "Address Line Tw" },
                    { "ship_city", "City Nam" },
                    { "ship_state", "State Nam" },
                    { "ship_postcode", "Post Cod" },
                    { "ship_country", "Countr" },
                    { "value_a", $"{code}" },
                    { "value_b", "Secondary" },
                    { "value_c", "ggg" + ", WebPage" },
                    { "value_d", $"{username}" },
                     { "shipping_method", "NO" },
                    { "num_of_item", "1" },
                    { "product_name", $"{client_name}" },
                    { "product_profile", "general" },
                    { "product_category", "Demo" }
               };
                //    NameValueCollection PostData = new()
                //{
                //    { "total_amount", $"{amount}" },
                //    { "tran_id", GenerateUniqueId() },
                //    //PostData.Add("tran_id", "TESTASPNET1234");
                //    { "success_url", baseUrl + "/Payment/PaymentSuccess" },
                //    { "fail_url", baseUrl + "/Payment/CheckoutFail" },
                //    { "cancel_url", baseUrl + "/Payment/CheckoutCancel" },

                //    { "version", "3.00" },
                //    { "cus_name", $"{client_name}" },
                //    { "cus_email", "abc.xyz@mail.co" },
                //    { "cus_add1", "Address Line On" },
                //    { "cus_add2", "Address Line Tw" },
                //    { "cus_city", "City Nam" },
                //    { "cus_state", "State Nam" },
                //    { "cus_postcode", "Post Cod" },
                //    { "cus_country", "Countr" },
                //    { "cus_phone", "0111111111" },
                //    { "cus_fax", "0171111111" },
                //    { "ship_name", "ABC XY" },
                //    { "ship_add1", "Address Line On" },
                //    { "ship_add2", "Address Line Tw" },
                //    { "ship_city", "City Nam" },
                //    { "ship_state", "State Nam" },
                //    { "ship_postcode", "Post Cod" },
                //    { "ship_country", "Countr" },
                //    { "value_a", $"{code}" },
                //    { "value_b", "Secondary" },
                //    { "value_c", remoteIpAddress + ", WebPage" },
                //    { "value_d", $"{username}" },
                //    { "shipping_method", "NO" },
                //    { "num_of_item", "1" },
                //    { "product_name", $"{client_name}" },
                //    { "product_profile", "general" },
                //    { "product_category", "Demo" }
                //};
                var isSandboxMood = true;
                SSL.SSLCommerz sslcz = new(
                    ExtraAct.ssl_store_demo_id2,
                    ExtraAct.ssl_store_demo_pass2,
                    isSandboxMood
                    );
                //SSLCommerz sslcz = new SSLCommerz("royal5ea12c33f3985", "royal5ea12c33f3985@ssl");
               // SSLCommerz sslcz = new SSLCommerz("", "");
                string response = sslcz.InitiateTransaction(PostData);
                //Response.Redirect(response);

                return Redirect(response);
            }
           else
            return View();
        }

        private string GenerateUniqueId()
        {
            long i = 1;

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= (b + 1);
            }

            return string.Format("{0:x}", i - DateTime.Now.Ticks).ToUpper();
        }

        [AllowAnonymous]
        public IActionResult PaymentSuccess() 
        {
            if (!(!string.IsNullOrEmpty(Request.Form["status"]) && Request.Form["status"] == "VALID"))
            {
                ViewBag.SuccessInfo = "There some error while processing your payment. Please try again.";
                return View();
            }

            string TrxID = Request.Form["tran_id"];
            // AMOUNT and Currency FROM DB FOR THIS TRANSACTION
            string amount = "85000";
            string currency = "BDT";

            var storeId = ExtraAct.ssl_store_demo_id2;
            var storePassword = ExtraAct.ssl_store_demo_pass2;

            SSL.SSLCommerz sslcz = new(storeId, storePassword, true);
            var response = sslcz.OrderValidate(TrxID, amount, currency, Request);
            var successInfo = $"Validation Response: {response}";
            ViewBag.SuccessInfo = successInfo;
            return View();
        }
        public IActionResult PaymentFail()
        {
            return View();
        }
        public IActionResult PaymentError()
        {
            return View();
        }

        public IActionResult Checkout()
        {
            var productName = "HP Pavilion Series Laptop";
            var price = 85000;

            var baseUrl = Request.Scheme + "://" + Request.Host;

            // CREATING LIST OF POST DATA
            NameValueCollection PostData = new()
            {
                { "total_amount", $"{price}" },
                { "tran_id", "TESTASPNET12345" },
                //PostData.Add("tran_id", "TESTASPNET1234");
                { "success_url", baseUrl + "/Payment/CheckoutConfirmation" },
                { "fail_url", baseUrl + "/Payment/CheckoutFail" },
                { "cancel_url", baseUrl + "/Payment/CheckoutCancel" },

                { "version", "3.00" },
                { "cus_name", "ABC XY" },
                { "cus_email", "abc.xyz@mail.co" },
                { "cus_add1", "Address Line On" },
                { "cus_add2", "Address Line Tw" },
                { "cus_city", "City Nam" },
                { "cus_state", "State Nam" },
                { "cus_postcode", "Post Cod" },
                { "cus_country", "Countr" },
                { "cus_phone", "0111111111" },
                { "cus_fax", "0171111111" },
                { "ship_name", "ABC XY" },
                { "ship_add1", "Address Line On" },
                { "ship_add2", "Address Line Tw" },
                { "ship_city", "City Nam" },
                { "ship_state", "State Nam" },
                { "ship_postcode", "Post Cod" },
                { "ship_country", "Countr" },
                { "value_a", "ref00" },
                { "value_b", "ref00" },
                { "value_c", "ref00" },
                { "value_d", "ref00" },
                { "shipping_method", "NO" },
                { "num_of_item", "1" },
                { "product_name", $"{productName}" },
                { "product_profile", "general" },
                { "product_category", "Demo" }
            };

            //we can get from email notificaton
            var storeId = ExtraAct.ssl_store_demo_id2;
            var storePassword = ExtraAct.ssl_store_demo_pass2;
            var isSandboxMood = true;

            PaymentGateway.SSLCommerz sslcz = new(storeId, storePassword, isSandboxMood);

            string response = sslcz.InitiateTransaction(PostData);

            return Redirect(response);
        }
        [AllowAnonymous]
        public IActionResult CheckoutConfirmation()
        {
            if (!(!string.IsNullOrEmpty(Request.Form["status"]) && Request.Form["status"] == "VALID"))
            {
                ViewBag.SuccessInfo = "There some error while processing your payment. Please try again.";
                return View();
            }

            string TrxID = Request.Form["tran_id"];
            // AMOUNT and Currency FROM DB FOR THIS TRANSACTION
            string amount = "85000";
            string currency = "BDT";

            var storeId = ExtraAct.ssl_store_demo_id2;
            var storePassword = ExtraAct.ssl_store_demo_pass2;

            PaymentGateway.SSLCommerz sslcz = new (storeId, storePassword, true);
            var response = sslcz.OrderValidate(TrxID, amount, currency, Request);
            var successInfo = $"Validation Response: {response}";
            ViewBag.SuccessInfo = successInfo;

            return View();
        }
        public IActionResult CheckoutFail()
        {
            ViewBag.FailInfo = "There some error while processing your payment. Please try again.";
            return View();
        }
        public IActionResult CheckoutCancel()
        {
            ViewBag.CancelInfo = "Your payment has been cancel";
            return View();
        }
    }
}
