using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventReservations.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(IWebHostEnvironment env, ILogger<ErrorController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [Route("/error")]
        [HttpGet]
        [HttpPost]
        public IActionResult HandleError()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            _logger.LogError(exception, "Error no controlado en la aplicación.");

            bool showDetails = _env.IsDevelopment();

            var errorResponse = new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "Ocurrió un error inesperado.",
                detail = showDetails ? exception?.Message : null,
                stackTrace = showDetails ? exception?.StackTrace : null
            };

            return StatusCode(errorResponse.statusCode, errorResponse);
        }
    }
}


