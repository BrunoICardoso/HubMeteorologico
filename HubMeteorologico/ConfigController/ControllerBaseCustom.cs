using HubMeteorologico.Domain.ResponseDefault;
using Microsoft.AspNetCore.Mvc;

namespace HubMeteorologico.API.ConfigController
{
    public class ControllerBaseCustom : ControllerBase
    {
        public ActionResult<T> StatusCode<T>(T returnAPI) where T : ReturnDefault
        {
            return new ObjectResult(returnAPI) { StatusCode = (int)returnAPI.StatusCode };
        }

        public IActionResult IStatusCode<T>(T returnAPI) where T : ReturnDefault
        {
            return new ObjectResult(returnAPI) { StatusCode = (int)returnAPI.StatusCode };
        }
    }
}
