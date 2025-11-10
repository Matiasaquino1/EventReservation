using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventReservations.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        [HttpGet]
        public IActionResult HandleError()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            var response = new
            {
                message = "Ocurrió un error inesperado.",
                detail = exception?.Message,
                stackTrace = exception?.StackTrace
            };

            return Problem(
                detail: response.detail,
                title: response.message,
                statusCode: (int)HttpStatusCode.InternalServerError
            );
        }
    }
}

