using Microsoft.AspNetCore.Mvc;

namespace RCLWEBCORE.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewData["Data"] = "No data has been found";
                    ViewBag.ErrorMessage = "Sorry The page could not be found";
                    break;
                case 401:
                    ViewBag.ErrorMessage = "You are not authorized to access it";
                    break;
                case 400:
                    ViewData["Data"] = "Server Error";
                    ViewData["result"] = "<a target='_blank' href='https://royalcapitalbd.com/'>Click here to return in Home Page </a>";
                    //  ViewBag.ErrorMessage = "Bad Request";
                    break;
                case 500:
                    ViewData["Data"] = "Server Error";

                    //  ViewBag.ErrorMessage = "Bad Request";
                    break;
            }
            return View("NotFound");
        }
    }
}
