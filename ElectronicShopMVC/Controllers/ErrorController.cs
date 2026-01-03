using Microsoft.AspNetCore.Mvc;

namespace ElectronicShopMVC.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("Error/{statusCode}")]
        public IActionResult Error(int? statusCode)
        {
            try
            {
                if (statusCode.HasValue)
                {
                    _logger.LogWarning("Error occurred with status code: {StatusCode}", statusCode);

                    return statusCode switch
                    {
                        404 => View("NotFound"),
                        500 => View("GenError"),
                        _ => View("GenError")
                    };
                }

                _logger.LogWarning("Error occurred without status code");
                return View("GenError");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling error request");
                return View("GenError");
            }
        }
    }
}
