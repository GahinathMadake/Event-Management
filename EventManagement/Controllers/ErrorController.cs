using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyMvcApp.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode?}")]
        public IActionResult Index(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                Response.StatusCode = statusCode.Value;
                _logger.LogError($"Error {statusCode} occurred at {Request.Path}");
                
                if(statusCode == 404 || statusCode==500)
                return View($"Error{statusCode}");
                else
                return View("Error");
            }
            return View("Error");
        }

        [Route("Error")]
        public IActionResult HandleError()
        {
            return View("Error");
        }
    }
}